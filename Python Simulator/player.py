
"""
Each Player has a set of 16 pieces.

Each player also has a counter of how many moves he has left in the turn.
The players turn ends when the number of moves left is zero

"""


class Player:
    def __init__(self, name="Default Name"):
        self.name = name
        self.pieces = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16}
        self.moves = 0
        self.turns_played = 0

    def check_piece(self, num):
        if num in self.pieces:
            return True

    def remove_piece(self, num):  # this is essentially a move meaning that we subtract one from moves counter
        try:
            self.pieces.remove(num)
            self.moves -= 1
            if self.moves == 0:
                self.turns_played += 1
        except KeyError as e:
            print("Piece does not exist for player")

    def add_moves(self, new_moves=1):  # if we created more squares, we get more moves
        self.moves += new_moves

    def moves_left(self):
        return self.moves

    def get_turns_played(self):
        return self.turns_played

    def add_turn(self):
        self.turns_played += 1

    def is_player_finished(self):
        return len(self.pieces) == 0
