# a class representing the ai logic, gets details of the game and calculates best move for a player
import copy
import time
from math import inf
import random


class Ai_player:
    def __init__(self, level, max_turn_time):
        self.shape_permutation = {1: [1, 2, 3, 4, 5, 6, 7, 8],
                                  2: [1, 2],
                                  3: [1, 2, 3, 4, 5, 6, 7, 8],
                                  4: [1, 2, 3, 4, 5, 6, 7, 8],
                                  5: [1, 2, 3, 4, 5, 6, 7, 8],
                                  6: [1, 2, 3, 4],
                                  7: [1, 2, 3, 4, 5, 6, 7, 8],
                                  8: [1, 2, 3, 4],
                                  9: [1, 2, 5, 6],
                                  10: [1, 2, 3, 4],
                                  11: [1, 2, 5, 6],
                                  12: [1, 2, 3, 4, 5, 6, 7, 8],
                                  13: [1, 2, 3, 4, 5, 6, 7, 8],
                                  14: [1, 2, 3, 4],
                                  15: [1, 2, 3, 4, 5, 6, 7, 8],
                                  16: [1, 2, 3, 4, 5, 6, 7, 8]}
        self.moves_in_turn = []
        self.prob = 0.1  # 0.07
        self.run_time = max_turn_time - 5
        if level == "medium":
            self.run_time = (self.run_time * 2) / 3
        if level == "easy":
            self.run_time = self.run_time / 2

    def argmax(self, array):
        idx, max_val = -1, -2
        for i in range(len(array)):
            if array[i] > max_val:
                max_val = array[i]
                idx = i
        return idx

    def argmin(self, array):
        idx, min_val = -1, 2
        for i in range(len(array)):
            if array[i] < min_val:
                min_val = array[i]
                idx = i
        return idx

    def best_move_minmax_aux(self, board, players_pieces, turns_left_for_current_player, current_player):
        # current player -> 0 is the caller, others are the other players.
        if len(players_pieces[current_player]) == 0:
            return (1, None) if current_player == 0 else (-1, None)  # TODO check- should we consider last lap of game?
        turn_options = []
        if turns_left_for_current_player > 0:  # simulate the turn of a player
            for piece in players_pieces[current_player]:
                for permutation in range(1, 9):
                    for game_cord_x in range(0, 32):
                        for game_cord_y in range(0, 32):
                            new_board = copy.deepcopy(board)
                            if new_board.add_piece(current_player, piece, permutation,
                                                   (game_cord_x, game_cord_y)) and new_board.new_squares() > 0:
                                new_player_piece = copy.deepcopy(players_pieces)
                                new_player_piece[current_player].remove(piece)
                                turn_options.append((self.best_move_minmax_aux(new_board, new_player_piece,
                                                                               turns_left_for_current_player - 1 + new_board.new_squares() - 1,
                                                                               current_player)[0],
                                                     (piece, permutation, game_cord_x, game_cord_y)))
            if current_player == 0:
                return turn_options[self.argmax([a[0] for a in turn_options])]
            else:
                return turn_options[self.argmin([a[0] for a in turn_options])]
        else:
            return (
            self.best_move_minmax_aux(board, players_pieces, 1, (current_player + 1) % len(players_pieces))[0], None)

    def best_move_minmax(self, board, players_pieces, current_player_idx):
        for i in range(current_player_idx):
            players_pieces.append(players_pieces.pop(0))
        return self.best_move_minmax_aux(board, players_pieces, 1, 0)[
            1]  # returns (piece,permutation,game_cord_x,game_cord_y)

    def best_move_depth_minmax_aux(self, board, players_pieces, turns_left_for_current_player, current_player, depth):
        # current player -> 0 is the caller, others are the other players.
        if len(players_pieces[current_player]) == 0:
            return (1, None) if current_player == 0 else (-1, None)  # TODO check- should we consider last lap of game?
        if depth == 0:
            return self.score_of_state(board, players_pieces, current_player, turns_left_for_current_player), None
        turn_options = []
        if turns_left_for_current_player > 0:  # simulate the turn of a player
            for piece in players_pieces[current_player]:
                for permutation in range(1, 9):
                    for game_cord_x in range(0, 32):
                        for game_cord_y in range(0, 32):
                            new_board = copy.deepcopy(board)
                            if new_board.add_piece(current_player, piece, permutation,
                                                   (game_cord_x, game_cord_y)) and new_board.new_squares() > 0:
                                new_player_piece = copy.deepcopy(players_pieces)
                                new_player_piece[current_player].remove(piece)
                                turn_options.append((self.best_move_depth_minmax_aux(new_board, new_player_piece,
                                                                                     turns_left_for_current_player - 1 + new_board.new_squares() - 1,
                                                                                     current_player, depth)[0],
                                                     (piece, permutation, game_cord_x, game_cord_y)))
            if current_player == 0:
                return turn_options[self.argmax([a[0] for a in turn_options])]
            else:
                return turn_options[self.argmin([a[0] for a in turn_options])]
        else:
            return (
            self.best_move_depth_minmax_aux(board, players_pieces, 1, (current_player + 1) % len(players_pieces),
                                            depth - 1)[0], None)

    def best_move_depth_minmax(self, board, players_pieces, depth,
                               current_player_idx):  # depth is max number of turnes to predict
        for i in range(current_player_idx):
            players_pieces.append(players_pieces.pop(0))
        return self.best_move_depth_minmax_aux(board, players_pieces, 1, 0, depth)[
            0]  # returns (piece,permutation,game_cord_x,game_cord_y)

    def best_move_depth_alphabeta_aux(self, alpha, beta, board, players_pieces, turns_left_for_current_player,
                                      current_player, depth, prob):
        # current player -> 0 is the caller, others are the other players.
        if len(players_pieces[current_player]) == 0:
            return (1, None) if current_player == 0 else (-1, None)  # TODO check- should we consider last lap of game?
        if depth == 0:
            return self.score_of_state(board, players_pieces, current_player, turns_left_for_current_player), None
        turn_options = []
        if turns_left_for_current_player > 0:  # simulate the turn of a player
            for piece in players_pieces[current_player]:
                for permutation in range(1, 9):
                    for game_cord_x in range(0, 32):
                        for game_cord_y in range(0, 32):
                            if random.random() < prob:
                                new_board = copy.deepcopy(board)
                                if new_board.add_piece(current_player, piece, permutation,
                                                       (game_cord_x, game_cord_y)) and new_board.new_squares() > 0:
                                    new_player_piece = copy.deepcopy(players_pieces)
                                    new_player_piece[current_player].remove(piece)
                                    turn_options.append(
                                        (new_board, new_player_piece, piece, permutation, game_cord_x, game_cord_y))
            if current_player == 0:
                best_val = -inf
                move = None
                for move_option in turn_options:
                    value = self.best_move_depth_alphabeta_aux(alpha, beta, move_option[0], move_option[1],
                                                               turns_left_for_current_player - 1 + move_option[
                                                                   0].new_squares() - 1, current_player, depth, prob)[0]
                    best_val = max(value, best_val)
                    if best_val == value:
                        move = [move_option[i] for i in range(2, 6)]
                    alpha = max(alpha, best_val)
                    if beta <= alpha:
                        break
                return best_val, move
            else:
                best_val = inf
                move = None
                for move_option in turn_options:
                    value = self.best_move_depth_alphabeta_aux(alpha, beta, move_option[0], move_option[1],
                                                               turns_left_for_current_player - 1 + move_option[
                                                                   0].new_squares() - 1, current_player, depth, prob)[0]
                    best_val = min(value, best_val)
                    if best_val == value:
                        move = [move_option[i] for i in range(2, 6)]
                    beta = min(beta, best_val)
                    if beta <= alpha:
                        break
                return best_val, move
        else:
            return (self.best_move_depth_alphabeta_aux(alpha, beta, board, players_pieces, 1,
                                                       (current_player + 1) % len(players_pieces), depth - 1, prob)[0],
                    None)

    def best_move_depth_alphabeta(self, board, players_pieces, depth, current_player_idx,
                                  prob=1):  # depth is max number of turnes to predict, prob is probaillity to choose a child
        for i in range(current_player_idx):
            players_pieces.append(players_pieces.pop(0))
        return self.best_move_depth_alphabeta_aux(-inf, inf, board, players_pieces, 1, 0, depth, prob)[
            1]  # returns (piece,permutation,game_cord_x,game_cord_y)

    def best_move_depth_alphabeta_timeLimit_aux(self, alpha, beta, board, players_pieces, turns_left_for_current_player,
                                                current_player, depth, prob,
                                                start_time, time_limit):
        # current player -> 0 is the caller, others are the other players.
        if time.time() - start_time > time_limit:
            return "time is up", "time is up"
        if len(players_pieces[current_player]) == 0:
            return (1, None) if current_player == 0 else (-1, None)  # TODO check- should we consider last lap of game?
        if depth == 0:
            return self.score_of_state(board, players_pieces, current_player, turns_left_for_current_player), None
        turn_options = []
        if turns_left_for_current_player > 0:  # simulate the turn of a player
            for piece in players_pieces[current_player]:
                for permutation in self.shape_permutation[piece]:
                    for game_cord_x in range(0, 32):
                        for game_cord_y in range(0, 32):
                            if random.random() < prob:
                                new_board = copy.deepcopy(board)
                                if new_board.add_piece(current_player, piece, permutation, (
                                game_cord_x, game_cord_y)) and new_board.count_new_squares() > 0:
                                    new_player_piece = copy.deepcopy(players_pieces)
                                    new_player_piece[current_player].remove(piece)
                                    turn_options.append(
                                        (new_board, new_player_piece, piece, permutation, game_cord_x, game_cord_y))
            if current_player == 0:
                best_val = -inf
                move = None
                for move_option in turn_options:
                    value = self.best_move_depth_alphabeta_timeLimit_aux(alpha, beta, move_option[0], move_option[1],
                                                                         turns_left_for_current_player - 1 +
                                                                         move_option[0].count_new_squares() - 1,
                                                                         current_player, depth, prob, start_time,
                                                                         time_limit)[0]
                    if value == "time is up":
                        return "time is up", "time is up"
                    best_val = max(value, best_val)
                    if best_val == value:
                        move = [move_option[i] for i in range(2, 6)]
                    alpha = max(alpha, best_val)
                    if beta <= alpha:
                        break
                return best_val, move
            else:
                best_val = inf
                move = None
                for move_option in turn_options:
                    value = self.best_move_depth_alphabeta_timeLimit_aux(alpha, beta, move_option[0], move_option[1],
                                                                         turns_left_for_current_player - 1 +
                                                                         move_option[
                                                                             0].count_new_squares() - 1, current_player,
                                                                         depth, prob, start_time, time_limit)[0]
                    if value == "time is up":
                        return "time is up", "time is up"
                    best_val = min(value, best_val)
                    if best_val == value:
                        move = [move_option[i] for i in range(2, 6)]
                    beta = min(beta, best_val)
                    if beta <= alpha:
                        break
                return best_val, move
        else:
            return (self.best_move_depth_alphabeta_timeLimit_aux(alpha, beta, board, players_pieces, 1,
                                                                 (current_player + 1) % len(players_pieces), depth - 1,
                                                                 prob, start_time, time_limit)[0], None)

    def best_move_depth_timeLimit_alphabeta(self, board, players_pieces, current_player_idx,
                                            prob=1,
                                            time_limit=300):  # prob is probaillity to choose a child (0.05 is fine)
        for i in range(current_player_idx):
            players_pieces.append(players_pieces.pop(0))

        start = time.time()
        depth = 1
        move = \
        self.best_move_depth_alphabeta_timeLimit_aux(-inf, inf, board, players_pieces, 1, 0, depth, prob, start, inf)[1]
        while True:
            depth += 1
            res = \
            self.best_move_depth_alphabeta_timeLimit_aux(-inf, inf, board, players_pieces, 1, 0, depth, prob, start,
                                                         time_limit)[1]
            if res == "time is up":
                print("********* DEPTH =" + str(depth) + "move =" + str(move) + " **********")
                return move
            else:
                move = res

    def score_of_state(self, board, players_pieces, player,
                       moves_left):  # if player is 1, then ai(0) played latest, so we calculate his state
        score = 1 / len(players_pieces[0])  # less pieces ->better
        score += moves_left  # more moves -> may help lose pieces
        score += 1 / (2 ** self.num_of_shpizim(board))  # less shpitzim for our enemy
        return score if player == 1 else 0 - score

    def num_of_shpizim(self, board):  # todo: implement this func
        count = 0
        board_line_list = [ll[0] for ll in board.line_list]
        for line in board_line_list:
            line = copy.deepcopy(line)
            line_pos = [line.pop(), line.pop()]
            if line_pos[0][1] == line_pos[1][1]:  # line on x axis
                if {(line_pos[0][0], line_pos[0][1]), (line_pos[0][0], line_pos[0][1] + 1)} in board_line_list and (
                        {(line_pos[1][0], line_pos[1][1]),
                         (line_pos[1][0], line_pos[1][1] + 1)} in board_line_list and not (
                        {(line_pos[0][0], line_pos[0][1] + 1),
                         (line_pos[1][0], line_pos[1][1] + 1)} in board_line_list)):
                    count += 1
                if ({(line_pos[0][0], line_pos[0][1]), (line_pos[0][0], line_pos[0][1] - 1)} in board_line_list and (
                        {(line_pos[1][0], line_pos[1][1]),
                         (line_pos[1][0], line_pos[1][1] - 1)} in board_line_list and not (
                        {(line_pos[0][0], line_pos[0][1] - 1),
                         (line_pos[1][0], line_pos[1][1] - 1)} in board_line_list))):
                    count += 1
            if line_pos[0][0] == line_pos[1][0]:  # line on y axis
                if {(line_pos[0][0], line_pos[0][1]), (line_pos[0][0] + 1, line_pos[0][1])} in board_line_list and (
                        {(line_pos[1][0], line_pos[1][1]),
                         (line_pos[1][0] + 1, line_pos[1][1])} in board_line_list and not (
                        {(line_pos[0][0] + 1, line_pos[0][1]),
                         (line_pos[1][0] + 1, line_pos[1][1])} in board_line_list)):
                    count += 1
                if ({(line_pos[0][0], line_pos[0][1]), (line_pos[0][0] - 1, line_pos[0][1])} in board_line_list and (
                        {(line_pos[1][0], line_pos[1][1]),
                         (line_pos[1][0] - 1, line_pos[1][1])} in board_line_list and not (
                        {(line_pos[0][0] - 1, line_pos[0][1]),
                         (line_pos[1][0] - 1, line_pos[1][1])} in board_line_list))):
                    count += 1
        return count

    # should i return serious of moves? the whole moves in the turn??? may work better.

    def best_move_depth_alphabeta_timeLimit_all_turn_aux(self, alpha, beta, board, players_pieces,
                                                         turns_left_for_current_player,
                                                         current_player, depth, prob,
                                                         start_time, time_limit):
        # current player -> 0 is the caller, others are the other players.
        if time.time() - start_time > time_limit:
            return "time is up", "time is up"
        if len(players_pieces[current_player]) == 0:
            return (10, None) if current_player == 0 else (
            -10, None)  # TODO check- should we consider last lap of game?
        if depth == 0:
            return self.score_of_state(board, players_pieces, current_player, turns_left_for_current_player), None
        turn_options = []
        if turns_left_for_current_player > 0:  # simulate the turn of a player
            x_min, x_max, y_min, y_max = self.possible_coordinates_range(board)
            for piece in players_pieces[current_player]:
                for permutation in self.shape_permutation[piece]:
                    for game_cord_x in range(x_min, x_max):
                        for game_cord_y in range(y_min, y_max):
                            if True:  # andom.random() < prob :#or True:#######################
                                # new_board = copy.deepcopy(board)
                                # if new_board.add_piece(current_player, piece, permutation,
                                #                       (game_cord_x, game_cord_y)) and new_board.new_squares() > 0:
                                if board.check_piece_placement_wrapper(piece, permutation, (game_cord_x, game_cord_y),
                                                                       False):
                                    new_board = copy.deepcopy(board)
                                    if new_board.add_piece(current_player, piece, permutation,
                                                           (game_cord_x,
                                                            game_cord_y)) and new_board.count_new_squares() > 0:
                                        new_player_piece = copy.deepcopy(players_pieces)
                                        new_player_piece[current_player].remove(piece)
                                        turn_options.append(
                                            (new_board, new_player_piece, piece, permutation, game_cord_x, game_cord_y))
            # print("done in:"+str(time.time() - start_time)+ "seconds\ncurrent player:"+str(current_player))###########
            # exit()
            ########################################3
            print("number of possabilitis:" + str(len(turn_options)))
            # lets sort by turn posibilitis and if peace gives us another turn -> lets choose first piece that gives more shpitzim
            turn_options.sort(key=lambda x: self.score_of_state_to_sort_childs(x[0]), reverse=True)
            if len(turn_options) > 4:
                turn_options = turn_options[:max(4, int(self.prob * len(turn_options)))]
            print("number of possabilitis now is::" + str(len(turn_options)))
            ###########################################
            if current_player == 0:
                best_val = -inf
                move = None
                for move_option in turn_options:
                    value, move_ = self.best_move_depth_alphabeta_timeLimit_all_turn_aux(alpha, beta, move_option[0],
                                                                                         move_option[1],
                                                                                         turns_left_for_current_player - 1 +
                                                                                         move_option[
                                                                                             0].count_new_squares() - 1,
                                                                                         current_player, depth, prob,
                                                                                         start_time,
                                                                                         time_limit)
                    if value == "time is up":
                        if move is None:
                            move = [move_option[i] for i in range(2, 6)]
                        return "time is up", move  # "time is up"
                    best_val = max(value, best_val)
                    if best_val == value:
                        if move_ is None:
                            move = [move_option[i] for i in range(2, 6)]
                        else:
                            move = [move_option[i] for i in range(2, 6)] + move_
                    alpha = max(alpha, best_val)
                    if beta <= alpha:
                        break
                return best_val, move
            else:
                best_val = inf
                move = None
                for move_option in turn_options:
                    value, move_ = self.best_move_depth_alphabeta_timeLimit_all_turn_aux(alpha, beta, move_option[0],
                                                                                         move_option[1],
                                                                                         turns_left_for_current_player - 1 +
                                                                                         move_option[
                                                                                             0].count_new_squares() - 1,
                                                                                         current_player,
                                                                                         depth, prob, start_time,
                                                                                         time_limit)
                    if value == "time is up":
                        return "time is up", move  # "time is up"
                    best_val = min(value, best_val)
                    if best_val == value:
                        if move_ is None:
                            move = [move_option[i] for i in range(2, 6)]
                        else:
                            move = [move_option[i] for i in range(2, 6)] + move_
                    beta = min(beta, best_val)
                    if beta <= alpha:
                        break
                return best_val, move
        else:
            return (self.best_move_depth_alphabeta_timeLimit_all_turn_aux(alpha, beta, board, players_pieces, 1,
                                                                          (current_player + 1) % len(players_pieces),
                                                                          depth - 1,
                                                                          prob, start_time, time_limit)[0], None)

    def best_move_depth_alphabeta_timeLimit_all_turn(self, board, players_pieces, current_player_idx,
                                                     prob=1,
                                                     time_limit=300):  # prob is probaillity to choose a child (0.05 is fine)
        for i in range(current_player_idx):
            players_pieces.append(players_pieces.pop(0))
        # print("**** PLAYER PIECES ARE:"+ str(players_pieces[0]))
        start = time.time()
        depth = 1
        ret_val, move = self.best_move_depth_alphabeta_timeLimit_all_turn_aux(-inf, inf, board, players_pieces, 1, 0,
                                                                              depth, prob, start,
                                                                              time_limit)  # not inf in time limit
        if ret_val == "time is up":
            print("**im here, move=" + str(move) + "**")
            if move != None and move != "time is up":
                res = move
            else:
                res = self.get_first_legal_move(players_pieces[0], board)
                res = [a for a in res]
            return res
        while True:
            depth += 1
            res = \
            self.best_move_depth_alphabeta_timeLimit_all_turn_aux(-inf, inf, board, players_pieces, 1, 0, depth, prob,
                                                                  start, time_limit)[1]
            print("res is:" + str(res))
            if res == "time is up" or res is None:
                print("********* DEPTH =" + str(depth) + " move =" + str(move) + " **********")
                return move
            else:
                move = res

    def calc_move(self, board, players_pieces, current_player_idx):
        no_need_2_calc = True
        if board.new_board:  # put first piece for new board
            return 15, 1
        players_pieces_cpy = copy.deepcopy(players_pieces)
        players_pieces_cpy_again = copy.deepcopy(players_pieces)
        if self.moves_in_turn == []:
            no_need_2_calc = False
            self.moves_in_turn = self.best_move_depth_alphabeta_timeLimit_all_turn(board, players_pieces_cpy,
                                                                                   current_player_idx, self.prob,
                                                                                   self.run_time)
        if self.moves_in_turn == [] or self.moves_in_turn is None:  # could not find, prob was too much strict, run on depth 1 only.
            self.moves_in_turn = self.best_move_depth_alphabeta_timeLimit_all_turn(board, players_pieces_cpy_again,
                                                                                   current_player_idx, 1,
                                                                                   1)
        res = (
        self.moves_in_turn.pop(0), self.moves_in_turn.pop(0), self.moves_in_turn.pop(0), self.moves_in_turn.pop(0))
        if no_need_2_calc:
            time.sleep(1)
        return res

    def get_first_legal_move(self, player_pieces, board):
        for piece in player_pieces:
            for permutation in self.shape_permutation[piece]:
                for game_cord_x in range(0, 32):
                    for game_cord_y in range(0, 32):
                        if board.check_piece_placement_wrapper(piece, permutation, (game_cord_x, game_cord_y),
                                                               False):
                            new_board = copy.deepcopy(board)
                            if new_board.add_piece(0, piece, permutation,
                                                   (game_cord_x, game_cord_y)) and new_board.count_new_squares() > 0:
                                return piece, permutation, game_cord_x, game_cord_y

    def possible_coordinates_range(self, board):
        x_min, y_min = 32, 32
        x_max, y_max = 0, 0
        for line in board.line_list:
            line = line[0]
            line = copy.deepcopy(line)
            first_point = line.pop()
            second_point = line.pop()
            if x_min > first_point[0] or x_min > second_point[0]:
                x_min = min(first_point[0], second_point[0])
            if x_max < first_point[0] or x_max < second_point[0]:
                x_max = max(first_point[0], second_point[0])
            if y_min > first_point[1] or y_min > second_point[1]:
                y_min = min(first_point[1], second_point[1])
            if y_max < first_point[1] or y_max < second_point[1]:
                y_max = max(first_point[1], second_point[1])

        x_min = max(0, x_min - 3)
        y_min = max(0, y_min - 3)
        x_max = min(32, x_max + 3)
        y_max = min(32, y_max + 3)
        return x_min, x_max, y_min, y_max

    def score_of_state_to_sort_childs(self, board):
        score = board.count_new_squares()
        if score > 1:
            score += 0.01 * self.num_of_shpizim(board)
        return score
