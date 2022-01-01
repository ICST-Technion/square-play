from player import Player
from board import Board


class EventGame:
    def __init__(self, players=None):
        if players is None:
            self.players = []
            for i in range(0, 2):  # default to 2 players
                self.players.append(Player(f"Player_{i + 1}"))
        else:
            self.players = players
        self.started = False
        self.playing_players = []
        for index, player in enumerate(self.players):
            self.playing_players.append(index + 1)

        self.game_board = Board()
        self.winners = []
        self.curr_player_num = len(players)  # the player number who's turn it is
        self.final_round_cnt = -1
        self.game_finished = False

    def start_game(self, piece_num, permutation):
        if self.game_finished:
            print("Error, Game has already Finished")
            return -1
        elif self.started:
            print("Error, Game has already started")
            return -1

        curr_player = self.players[-1]
        curr_player.add_moves()
        if not self.started:
            if curr_player.check_piece(piece_num) and self.game_board.add_piece(len(self.players), piece_num,
                                                                                permutation, (15, 15), True):
                curr_player.remove_piece(piece_num)
                if self.playing_players.index(self.curr_player_num) == (len(self.playing_players) - 1):
                    self.curr_player_num = self.playing_players[0]
                else:
                    self.curr_player_num = self.playing_players.index(self.curr_player_num) + 1
                self.started = True
                self.players[self.curr_player_num - 1].add_moves()
                return 1
            else:
                return -1

    def check_legal_move(self, player_num, piece_num, permutation, x_coor, y_coor):
        # only check legality without checking player turn/game state
        try:
            curr_player = self.players[player_num - 1]
        except IndexError:
            return -1
        return curr_player.check_piece(piece_num) and self.game_board.check_piece_placement_wrapper(piece_num,
                                                                                                    permutation,
                                                                                                    (x_coor, y_coor),
                                                                                                    not self.started)

    def metadata_checks(self, player_num):

        if self.game_finished:
            print("Error, Game has already Finished")
            return -1

        if not self.started:
            print("Invalid Event, game hasn't started")
            return -1

        if player_num != self.curr_player_num:
            print(f"Wrong player, player {self.curr_player_num} needs to play")
            return -1

        if player_num not in self.playing_players:
            print("Player has already won or is not playing")
            return -1

        return 0

    def pass_turn(self, player_num):
        # if this player chooses to pass his turn
        if self.metadata_checks(player_num) == -1:
            return -1
        try:
            curr_player = self.players[player_num - 1]
        except IndexError:
            return -1
        curr_player.moves = 0

        if self.playing_players.index(self.curr_player_num) == (len(self.playing_players) - 1):
            self.curr_player_num = self.playing_players[0]
        else:
            self.curr_player_num = self.playing_players[self.playing_players.index(self.curr_player_num) + 1]
        self.players[self.curr_player_num - 1].add_moves()
        print("passed turn to next player")
        if self.final_round_cnt != -1:  # relevant for the final round
            self.final_round_cnt -= 1
        if self.__check_game_finished() == 5:
            return 5
        return 0

    def move(self, player_num, piece_num, permutation, x_coor, y_coor):

        if self.metadata_checks(player_num) == -1:
            return -1

        if player_num != self.curr_player_num:
            print(f"Wrong player, player {self.curr_player_num} needs to play")
            return -1

        try:
            curr_player = self.players[player_num - 1]
        except IndexError:
            return -1

        if curr_player.check_piece(piece_num) and self.game_board.add_piece(player_num, piece_num, permutation,
                                                                            (x_coor, y_coor)):
            curr_player.remove_piece(piece_num)
            new_squares = self.game_board.count_new_squares()

            if new_squares != 0:
                curr_player.add_moves(new_squares - 1)
                print(f"{curr_player.name} got {new_squares - 1} more moves with a total of {curr_player.moves_left()} left")

            if curr_player.is_player_finished():
                print("Final Round for players with less turns")
                self.final_round_cnt = len(self.playing_players) - self.playing_players.index(self.curr_player_num) - 1
                self.winners.append(curr_player)
                prev_turn = self.curr_player_num
                if self.playing_players.index(self.curr_player_num) == (len(self.playing_players) - 1):
                    self.curr_player_num = self.playing_players[0]
                else:
                    self.curr_player_num = self.playing_players[self.playing_players.index(self.curr_player_num) + 1]
                self.players[self.curr_player_num - 1].add_moves()

                self.playing_players.remove(prev_turn)
                if self.__check_game_finished() == 5:
                    return 5  # 5 means game is finished
                return 3  # 3 means this is the final round

            if curr_player.moves_left() < 1:  # end of turn for player

                if self.playing_players.index(self.curr_player_num) == (len(self.playing_players) - 1):
                    self.curr_player_num = self.playing_players[0]
                else:
                    self.curr_player_num = self.playing_players[self.playing_players.index(self.curr_player_num) + 1]
                self.players[self.curr_player_num - 1].add_moves()

                if self.final_round_cnt != -1:  # relevant for the final round
                    self.final_round_cnt -= 1

                if self.__check_game_finished() == 5:
                    return 5  # 5 means game is finished
                return 2  # 2 means player turn has ended
            return 1  # 1 means player still has moves left
        return -1

    def __check_game_finished(self):
        if self.final_round_cnt == 0 or len(self.playing_players) == 0:
            print(f"Game finished, winners are: {[p.name for p in self.winners]}")
            self.game_finished = True
            return 5  # 5 means game is finished
        return 0

    def get_board(self):
        return self.game_board

    def get_whos_turn(self):
        return self.curr_player_num

    def get_moves_left(self):
        return self.players[self.curr_player_num - 1]

    def compare_board(self, unity_board):
        return unity_board and self.started

    def get_player_pieces(self, player_num):
        if 0 < player_num < len(self.players):
            return self.players[player_num - 1].pieces
        return -1
