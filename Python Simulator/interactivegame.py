from player import Player
from board import Board


class InteractiveGame:
    def __init__(self, players=None):
        if players is None:
            self.players = []
            for i in range(0, 2):  # default to 2 players
                self.players.append(Player(f"Player_{i + 1}"))
        else:
            self.players = players
        self.game_board = Board()
        self.winners = None

    def gameplay(self):

        if not self.players:
            print("Game with no players, finished")
            return

        while True:
            player = self.players[-1]
            # so that he will always have an extra turn at the end if someone else wins, since
            # the for loop for the turns begins at the first player
            if self.game_board.new_board:
                print(f"{player.name}'s Turn, choose piece and permutation, placement is predefined")
                pn, pr = input("Syntax is: <piece num: 1-16> <permutation: 1-8>:\n").split()
                pn = int(pn)
                pr = int(pr)
                if player.check_piece(pn) and self.game_board.add_piece(len(self.players), pn, pr, (15, 15), True):
                    player.remove_piece(pn)
                    break
                else:
                    print("Try again")

        while True:
            for count, player in enumerate(self.players):
                # for loop for player turns
                # each player has one move at the start of each turn
                player.add_moves()
                while True:
                    print(f"{player.name}'s Move, choose piece, permutation, placement")
                    pn, pr, x, y = input("Syntax is: <piece num: 1-16> <permutation: 1-8> <X coordinate : 1-32> <Y "
                                         "coordinate 1-32)>:\n").split()
                    pn = int(pn)
                    pr = int(pr)
                    coordinates = (int(x), int(y))

                    if player.check_piece(pn) and self.game_board.add_piece(count + 1, pn, pr, coordinates):
                        player.remove_piece(pn)
                        new_squares = self.game_board.new_squares()

                        if new_squares != 0:
                            player.add_moves(new_squares - 1)
                            print(
                                f"{player.name} got {new_squares - 1} more moves with a total of {player.moves_left()}"
                                f" left")

                        if player.is_player_finished():
                            print("Final Round for players with less turns")
                            self.winners.append(player)
                            break

                        if player.moves_left() < 1:  # end of turn for player
                            break
            if len(self.winners) != 0:
                break

        print("Congratulations, the winner/s are: ")
        num_turns = 0
        for player in self.winners:
            num_turns = player.turns_played
            print(player.name)
        print(f"Which finished the game in {num_turns} Turns")
        return
