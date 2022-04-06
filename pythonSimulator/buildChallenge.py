from board import Board
from player import Player
from pieces import Piece
from challenge import Challenge


class BuildChallenge:
    def __init__(self, chal: Challenge = None, player=None):
        if player is None:
            self.player = Player(f"Guest_Player")
        else:
            self.player = player
        if chal is None:
            self.challenge = Challenge()  # default challenge
        else:
            self.challenge = chal
        self.pieces_left = list(self.challenge.bank)
        self.attempt = []
        self.challenge_finished = False
        self.move_count = 0

    def check_attempt(self):
        attempt_shadow = []
        for p in self.attempt:
            temp_p = Piece(p[0])
            temp_p.permutate((p[1] - 1) % 4, (p[1] - 1) // 4 == 1)
            temp_p.add_coordinates(p[2], p[3])
            for line in temp_p.shape:
                attempt_shadow.append(line)
        if len(attempt_shadow) != len(self.challenge.shadow):
            return False
        # assuming the challenge doesn't have overlapping lines, it forces the attempt not to as well
        for al in attempt_shadow:
            if al not in self.challenge.shadow:
                return False
        self.challenge_finished = True
        return True

    def add_to_attempt(self, piece_num, perm_index, x, y):
        if piece_num in self.pieces_left:
            self.pieces_left.remove(piece_num)
            self.attempt.append((piece_num, perm_index, x, y))
            self.move_count += 1
            return True
        else:
            print("Error, piece doesn't exist in bank")
            return False

    def remove_from_attempt(self, piece_num, perm_index, x, y):
        if (piece_num, perm_index, x, y) in self.attempt:
            self.pieces_left.append(piece_num)
            self.attempt.remove((piece_num, perm_index, x, y))
            self.move_count += 1
            return True
        else:
            print("Error, piece doesn't exist in attempt")
            return False

    def print_state(self):
        if not self.challenge_finished:
            print(f"Player {self.player.name}.name has made {self.move_count} moves,"
                  f" with the current attempt being: {self.attempt}")
        else:
            print(f"Success! Player {self.player.name} finished in {self.move_count} moves!")
