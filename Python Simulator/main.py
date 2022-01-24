from eventgame import EventGame
# from interactivegame import InteractiveGame
from player import Player
import socket
import numpy as np
import struct

if __name__ == '__main__':
    def remoteMain():
        s = socket.socket()
        socket.setdefaulttimeout(None)
        print('socket created ')
        port = 60000
        s.bind(('127.0.0.1', port))
        s.listen(1)
        game_started = False
        remote_game = None
        to_send = [float(-999)]
        while True:
            try:
                send_move_data = False
                c, addr = s.accept()
                bytes_received = c.recv(4000)
                action_code = np.frombuffer(bytes_received, dtype=np.intc)
                if action_code[0] == 0 and remote_game is None:
                    bytes_received = c.recv(4000)
                    players_names = bytes_received.decode("utf-8").split(',')
                    players = []
                    for name in players_names:
                        if name.find("AI_Player") >= 0:
                            players.append(Player(name=name, ai=True))
                        else:
                            players.append(Player(name=name))
                    print("Names:")
                    print(players_names)
                    remote_game = EventGame(players)
                    to_send = [1]
                if action_code[0] == 1 and remote_game:
                    bytes_received = c.recv(4000)
                    array_received = np.frombuffer(bytes_received, dtype=np.intc)
                    print("First Move:")
                    print(array_received)
                    to_send = [remote_game.start_game(array_received[0], array_received[1])]
                    if to_send != -1:
                        game_started = True

                if action_code[0] == 2 and game_started:
                    bytes_received = c.recv(4000)
                    array_received = np.frombuffer(bytes_received, dtype=np.intc)
                    print("Move:")
                    print(array_received)
                    to_send = [remote_game.move(array_received[0], array_received[1], array_received[2],
                                               array_received[3], array_received[4])]

                if action_code[0] == 3:  # ai player turn
                    if remote_game.players[remote_game.curr_player_num - 1].ai_player:
                        best_move = remote_game.players[remote_game.curr_player_num - 1].oracle.calc_move(
                            remote_game.game_board,
                            [p.pieces for p in remote_game.players],
                            remote_game.curr_player_num - 1
                        )
                        to_send = [float(
                            remote_game.move(remote_game.curr_player_num, best_move[0], best_move[1], best_move[2],
                                             best_move[3]))]
                        if to_send != [-1]:
                            send_arr = np.array([i for i in best_move], dtype=np.intc)
                            to_send = send_arr
                            send_move_data = True
                    else:
                        to_send = [-1]

                if action_code[0] == -1:
                    print("Bye!")
                    exit()

                to_send = to_send.append(remote_game.last_new_squares)

                if send_move_data:
                    bytes_to_send = struct.pack('5i', to_send)
                else:
                    bytes_to_send = struct.pack('2i', to_send)

                c.sendall(bytes_to_send)
                c.close()

            except Exception as e:
                print("error:")
                print(e)
                c.sendall(bytearray([]))
                c.close()
                break


    # ---------------------- INTERACTIVE GAME ----------------------
    '''print("Welcome to Square Game")
    number_of_players = int(input("Enter number of players, between 2 and 4:\n"))
    while not 2 <= number_of_players <= 4:
        number_of_players = int(input("Invalid Number of players, try again"))
    player_list = list()
    for i in range(0, number_of_players):
        player_list.append(Player(f"Player_{i + 1}"))
    test_Game = InteractiveGame(player_list)
    test_Game.gameplay()
'''
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
    remoteMain()
