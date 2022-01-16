from eventgame import EventGame
from interactivegame import InteractiveGame
from player import Player
import socket
import numpy as np
import struct

if __name__ == '__main__':
    # ---------------------- INTERACTIVE GAME ----------------------
    print("Welcome to Square Game")
    number_of_players = int(input("Enter number of players, between 2 and 4:\n"))
    while not 2 <= number_of_players <= 4:
        number_of_players = int(input("Invalid Number of players, try again"))
    player_list = list()
    for i in range(0, number_of_players):
        player_list.append(Player(f"Player_{i + 1}"))
    test_Game = InteractiveGame(player_list)
    test_Game.gameplay()

    # ---------------------- EVENT GAME ----------------------
    """
    print("Welcome to Square Game")
    number_of_players = int(input("Enter number of players, between 2 and 4:\n"))
    while not 2 <= number_of_players <= 4:
        number_of_players = int(input("Invalid Number of players, try again"))
    player_list = list()
    for i in range(0, number_of_players):
        player_list.append(Player(f"Player_{i + 1}"))
    test_Game = EventGame(player_list)
    test_Game.start_game(13, 4)
    test_Game.move(1, 9, 1, 16, 16)
    """
    # ----------------------- REMOTE GAME -----------------------

    s = socket.socket()
    socket.setdefaulttimeout(None)
    print('socket created ')
    port = 60000
    s.bind(('127.0.0.1', port))
    s.listen(1)
    game_started = False
    remote_game = None
    to_send = float(-999)
    while True:
        try:
            c, addr = s.accept()
            bytes_received = c.recv(4000)
            action_code = np.frombuffer(bytes_received, dtype=np.float32)

            if action_code[0] == 0 and remote_game is None:
                bytes_received = c.recv(4000)
                array_received = np.frombuffer(bytes_received, dtype=str)
                players_names = array_received[0].split(',')
                players = []
                for name in array_received:
                    players.append(Player(name))
                remote_game = EventGame(players)
                to_send = 1

            if action_code[0] == 1 and remote_game:
                bytes_received = c.recv(4000)
                array_received = np.frombuffer(bytes_received, dtype=np.float32)
                to_send = float(remote_game.start_game(array_received[0], array_received[1]))
                if to_send != -1:
                    game_started = True

            if action_code[0] == 2 and game_started:
                bytes_received = c.recv(4000)
                array_received = np.frombuffer(bytes_received, dtype=np.float32)
                array_received = np.frombuffer(bytes_received, dtype=np.string_)
                to_send = float(remote_game.move(array_received[0], array_received[1], array_received[2],
                                                 array_received[3], array_received[4]))

            bytes_to_send = struct.pack("%f", to_send)
            c.sendall(bytes_to_send)
            c.close()

        except Exception as e:
            print("error")
            c.sendall(bytearray([]))
            c.close()
            break
