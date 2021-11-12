
"""
the board is just a list of a lines (one unit long), where a line is a set of two coordinates.

Additionally we save the color of the line for possible future gam implementations

"""

from pieces import Piece


class Board:
    def __init__(self):
        self.line_list = []
        self.new_board = True
        self.last_piece = None

    def piece_permutation(self, p: Piece, index=1):
        if not 1 <= index <= 8:
            print("Illegal permutation")
        if index == 1:  # original
            pass
        if index == 2:
            p.rotate_90_right()
        if index == 3:
            p.rotate_90_right()
            p.rotate_90_right()
        if index == 4:
            p.rotate_90_right()
            p.rotate_90_right()
            p.rotate_90_right()
        if index == 5:
            p.x_axis_flip()
        if index == 6:
            p.x_axis_flip()
            p.rotate_90_right()
        if index == 7:
            p.x_axis_flip()
            p.rotate_90_right()
            p.rotate_90_right()
        if index == 8:
            p.x_axis_flip()
            p.rotate_90_right()
            p.rotate_90_right()
            p.rotate_90_right()

        return p

    def check_piece_placement(self, p: Piece):
        """
         IMPORTANT NOTE: I defined it as legal to place the piece on the board without touching any other piece
         if this isn't legal, another check needs to be made
        """
        # first we check that all coordinates in the piece are inside the board
        for line in p.shape:
            for (x, y) in line:
                if not (0 <= x <= 32 and 0 <= y <= 32):
                    print("Illegal coordinates")
                    return False
        # now we check that such a line doesn't already exist on the board

        for line in p.shape:
            for board_line in self.line_list:
                if board_line[0] == line:
                    print("line already taken")
                    return False
        return True

    def add_piece(self, player_num: int, piece_num=1, permutation_index=1, coordinates=(15, 15)):
        if 1 <= piece_num <= 16 and 1 <= permutation_index <= 8:
            new_piece = Piece(piece_num)
            new_piece = self.piece_permutation(new_piece, permutation_index)
            new_piece.add_coordinates(coordinates[0], coordinates[1])
            if self.check_piece_placement(new_piece):
                for line in new_piece.shape:
                    self.line_list.append((line, player_num))
                self.new_board = False
                self.last_piece = new_piece
                print("Piece added to board")
                # print(self.line_list)
                return True
            else:
                print("Piece NOT added to board")
        else:
            print("Illegal piece number or permutation")
        return False

    def new_squares(self):
        new_sq = 0
        if type(self.last_piece) == Piece and self.last_piece is not None:
            possible_squares = self.last_piece.get_possible_squares()
        else:
            return new_sq

        for sq in possible_squares:
            # looping all possible squares
            edge_counter = 0
            old_line = False
            perm_new_line = False
            for sq_line in sq:
                # looping over every line in a square
                for board_line in self.line_list:
                    # for every line in square we check if it exists on the board
                    if board_line[0] == sq_line:
                        edge_counter += 1
                        new_line = False
                        # if an edge was found we need to see if it was there before the newest piece was placed
                        for piece_line in self.last_piece.shape:
                            if sq_line == piece_line:
                                new_line = True
                                perm_new_line = True
                        if not new_line:
                            old_line = True
                        break
            if old_line and perm_new_line and edge_counter == 4:
                # a valid square consists of a new line from last piece and old lines from before
                new_sq += 1

        return new_sq
