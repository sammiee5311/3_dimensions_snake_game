import socket
import random
from time import sleep


IP, PORT = '', 12345
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.connect((IP, PORT))

while True:
    sleep(0.5)
    start_pos = [0, 0, 0]
    start_pos[random.randint(0,2)] = random.randint(-1,1)
    pos_string = ','.join(map(str, start_pos))
    print(pos_string)

    s.sendall(pos_string.encode('UTF-8'))
    received_data = s.recv(1024).decode('UTF-8')
    print(received_data)
