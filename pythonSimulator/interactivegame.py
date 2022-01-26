from player import Player
from board import Board
from eventgame import EventGame


class InteractiveGame:
    def __init__(self, players=None):
        if players is None:
            self.players = []
            for i in range(0, 2):  # default to 2 players
                self.players.append(Player(f"Player_{i + 1}"))
        else:
            self.players = players
        self.event_g = EventGame(players)

    def gameplay(self):

        if not self.players:
            print("Game with no players, finished")
            return
        ret = -1
        file_name = input("Choose output file name\n").split()
        while ret == -1:
            print(
                f"Player_{self.event_g.curr_player_num}'s Turn, choose piece and permutation, placement is "
                f"predefined")
            try:
                pn, pr = input("Syntax is: <piece num: 1-16> <permutation: 1-8>:\n").split()
                pn = int(pn)
                pr = int(pr)
                ret = self.event_g.start_game(pn, pr)
            finally:
                print("")
                # pass

        while ret != 5:
            print(f"Player_{self.event_g.curr_player_num}'s Move, choose piece, permutation, placement")
            pm = input("Pass/Turn/Store: 0/1/2 : ")
            pm = int(pm)
            if pm == 0:
                ret = self.event_g.pass_turn(self.event_g.curr_player_num)
            elif pm == 2:
                self.event_g.store_game(f"{file_name[0]}.json")
                ret = 5
            else:
                num, pn, pr, x, y = input(
                    "Syntax is: <Player Num> <piece num: 1-16> <permutation: 1-8> <X coordinate : 1-32> <Y "
                    "coordinate 1-32)>:\n").split()
                num = int(num)
                pn = int(pn)
                pr = int(pr)
                coordinates = (int(x), int(y))
                ret = self.event_g.move(num, pn, pr, coordinates[0], coordinates[1])
        return
