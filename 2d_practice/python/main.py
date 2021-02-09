import socket
import random
import torch
from time import sleep
from model import Model


class Test:
    def __init__(self):
        self.model = Model(11, 256, 3)
        self.model.load_state_dict(torch.load('./model/model.pth'))

    def get_action(self, model, state):
        final_choice = [0, 0, 0]
        cur_state = torch.tensor(state, dtype=torch.float)
        prediction = model(cur_state)
        choice = torch.argmax(prediction).item()
        final_choice[choice] = 1

        return final_choice


IP, PORT = '', 12345
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((IP, PORT))
test = Test()
received_data = None

while True:
    sleep(0.5)
    if received_data:
        state = list(map(int, received_data.split(',')))
        choice = test.get_action(test.model, state)
        pos_string = ','.join(map(str, choice))
        print(state)
    else:
        start_pos = [1, 0, 0]
        pos_string = ','.join(map(str, start_pos))

    print(pos_string)

    s.sendall(pos_string.encode('UTF-8'))
    received_data = s.recv(1024).decode('UTF-8')
    print(received_data)
