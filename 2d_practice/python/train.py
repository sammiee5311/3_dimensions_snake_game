import socket
import numpy as np
import matplotlib.pyplot as plt
from matplotlib import style
import time
import torch
import collections
import random

#########################################
from model import Model, Trainer
#########################################

MAX_MEMORY = 100000
EPISODES = 4000
BATCH_SIZE = 1000
COLLISION_PENALTY = 10
POINT_REWARD = 10
SHOW_EVERY = 10
STEP = 1800


class Agent:
    def __init__(self):
        self.n_games = 0
        self.epsilon = 0.9
        self.DISCOUNT = 0.9
        self.LEARNING_RATE = 0.001
        self.EPS_DECAY = 0.999
        self.q_table = collections.deque(maxlen=MAX_MEMORY)
        self.model = Model(11, 256, 3)
        self.trainer = Trainer(self.model, self.LEARNING_RATE, self.DISCOUNT)

    def remember(self, state, action, reward, next_state, game_over):
        self.q_table.append((state, action, reward, next_state, game_over))

    def train_long_memory(self):
        if len(self.q_table) > BATCH_SIZE:
            mini_sample = random.sample(self.q_table, BATCH_SIZE)
        else:
            mini_sample = self.q_table

        states, actions, rewards, next_states, game_overs = zip(*mini_sample)
        self.trainer.train_step(states, actions, rewards, next_states, game_overs)

    def train_short_memory(self, state, action, reward, next_state, game_over):
        self.trainer.train_step(state, action, reward, next_state, game_over)

    def get_action(self, state):
        final_choice = [0, 0, 0]
        if np.random.random() > self.epsilon:
            cur_state = torch.tensor(state, dtype=torch.float)
            prediction = self.model(cur_state)
            choice = torch.argmax(prediction).item()
            final_choice[choice] = 1
        else:
            choice = np.random.randint(0, 3)
            final_choice[choice] = 1

        return final_choice


def train():
    agent = Agent()
    episode_rewards = []
    record = 0

    IP, PORT = '', 12345
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.connect((IP, PORT))

    for episode in range(EPISODES):
        game_over = False
        episode_reward = 0

        if episode % SHOW_EVERY == 0:
            print("# : %d, epsilon : %f, score : %d" % (episode, agent.epsilon, record))
            print(f'{SHOW_EVERY} ep mean {np.mean(episode_rewards[-SHOW_EVERY:])}')

        while not game_over:
            reward = 0

            received_data = s.recv(1024).decode('UTF-8')
            old_state = list(map(int, received_data.split(',')))
            final_move = agent.get_action(old_state[:-2])
            pos_string = ','.join(map(str, final_move))
            s.sendall(pos_string.encode('UTF-8'))

            received_data = s.recv(1024).decode('UTF-8')
            new_state = list(map(int, received_data.split(',')))

            if new_state[-2] == 1:
                game_over = True
                reward = -COLLISION_PENALTY
            else:
                if new_state[-1] == 1:
                    reward = POINT_REWARD

            agent.train_short_memory(old_state[:-2], final_move, reward, new_state[:-2], game_over)

            agent.remember(old_state[:-2], final_move, reward, new_state[:-2], game_over)

            episode_reward += reward

            if game_over:
                agent.n_games += 1
                agent.train_long_memory()
                episode_rewards.append(episode_reward)
                agent.epsilon *= agent.EPS_DECAY
                agent.model.save()
                break

    moving_avg = np.convolve(episode_rewards, np.ones((SHOW_EVERY,)) / SHOW_EVERY, mode='valid')

    plt.plot([i for i in range(len(moving_avg))], moving_avg, color='indigo')
    plt.ylabel("reward on %d" % SHOW_EVERY)
    plt.xlabel("episode #")
    plt.show()


if __name__ == '__main__':
    train()
