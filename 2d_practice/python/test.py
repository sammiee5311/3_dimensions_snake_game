import socket
import random
import torch
from time import sleep
from model import Model


class Test:
    def __init__(self):
        self.model = Model(11, 256, 3)
        self.model.load_state_dict(torch.load('./model/model.pth'))

    def get_action(self, state):
        final_choice = [0, 0, 0]
        cur_state = torch.tensor(state, dtype=torch.float)
        prediction = self.model(cur_state)
        choice = torch.argmax(prediction).item()
        final_choice[choice] = 1

        return final_choice


IP, PORT = "", 12345
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((IP, PORT))
test = Test()
is_game = False

while not is_game:
    sleep(0.3)

    received_data = s.recv(1024).decode('UTF-8')
    old_state = list(map(int, received_data.split(',')))
    final_move = test.get_action(old_state[:-2])
    pos_string = ','.join(map(str, final_move))
    s.sendall(pos_string.encode('UTF-8'))

    if old_state[-2] == 1:
        is_game = True
