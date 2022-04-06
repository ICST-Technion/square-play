from pieces import Piece

"""
A challenge is a collection of pieces, such that when put on a board in a certain configuration, 
create a structure.
The challenge is to recreate the structure with the available pieces

Structure is a list where each value in it is a tuple (Piece_num, permutation_index, x, y).
TODO: maybe structure should be a list of actual Pieces instead....

The Structure will use the already existing 32X32 board, meaning that the player will see the "faded"
structure on the board itself, and try to put pieces on the board to cover the faded structure on the board.

Bank is the available pieces for the player to use. Sometimes there are more than one solution meaning that 
the Bank doesn't have to match the structure pieces

Shadow-struct is just the collection of lines of the struct, to keep the individual piece a mystery
"""


class Challenge:
    def __init__(self, structure=None, bank=None):
        if structure is not None:
            self.structure = structure
        else:
            self.structure = [(2, 1, 15, 15)]

        if bank is None:
            self.bank = [pt[0] for pt in self.structure]
        else:
            self.bank = bank
        self.shadow = []
        for p in self.structure:
            temp_p = Piece(p[0])
            temp_p.permutate((p[1] - 1) % 4, (p[1] - 1) // 4 == 1)
            temp_p.add_coordinates(p[2], p[3])
            for line in temp_p.shape:
                self.shadow.append(line)

