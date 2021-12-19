from pieces import Piece

"""
The Board is just a list of a lines (one unit long), where a line is a set of two coordinates.

Additionally we save the color of the line for possible future gam implementations

"""


class Board:
    def __init__(self):
        self.line_list = []  # list of tuples: [ ( {(a,b), (c,d)}, player_num, piece_id ), ...]
        self.new_board = True
        self.last_piece = None
        self.piece_count = 0

    def line_exists(self, list_of_lines, test_line):
        for line in list_of_lines:
            if test_line == line:
                return True
        return False

    def gen_piece_id(self):
        self.piece_count += 1
        return self.piece_count

    def find_extended_lines(self, ided_lines):
        """
        Function receives a list of lines (which are sets of coordinates)
        an ided_line will be a (coordinate set, piece_id) tuple
        function returns a list of the lines that can be created by extending two lines (from the same piece)
        in a list
        """
        extended_lines = []
        unchecked_lines = list(ided_lines)
        for line1 in ided_lines:
            unchecked_lines.remove(line1)
            line1_list = list(line1[0])  # list of 2 coordinates
            line1_id = line1[1]
            for line2 in unchecked_lines:
                line2_list = list(line2[0])  # list of 2 coordinates
                line2_id = line2[1]
                new_extended_line = None
                if line2_id == line1_id:  # only check if the lines are from the same piece
                    if line1_list[0] == line2_list[0] and (
                            line1_list[1][0] == line2_list[1][0] or line1_list[1][1] == line2_list[1][1]):
                        new_extended_line = {line1_list[1], line2_list[1]}
                    if line1_list[0] == line2_list[1] and (
                            line1_list[1][0] == line2_list[0][0] or line1_list[1][1] == line2_list[0][1]):
                        new_extended_line = {line1_list[1], line2_list[0]}
                    if line1_list[1] == line2_list[0] and (
                            line1_list[0][0] == line2_list[1][0] or line1_list[0][1] == line2_list[1][1]):
                        new_extended_line = {line1_list[0], line2_list[1]}
                    if line1_list[1] == line2_list[1] and (
                            line1_list[0][0] == line2_list[0][0] or line1_list[0][1] == line2_list[0][1]):
                        new_extended_line = {line1_list[0], line2_list[0]}
                if new_extended_line and not self.line_exists(extended_lines, new_extended_line):
                    extended_lines.append(new_extended_line)
        return extended_lines

    def piece_permutation(self, p: Piece, index=1):
        if not 1 <= index <= 8:
            print("Illegal permutation")
        p.permutate((index - 1) % 4, (index - 1) / 4 == 1)
        return p

    def check_piece_placement(self, p: Piece, first_move: bool):

        # first we check that all coordinates in the piece are inside the board
        for line in p.shape:
            for (x, y) in line:
                if not (0 <= x <= 32 and 0 <= y <= 32):
                    print("Illegal coordinates")
                    return False

        # now we check that such a line doesn't already exist on the board
        # and that the new piece touches the some other piece on the board.
        new_piece_touching = False
        for line in p.shape:
            new_coor_list = list(line)
            for board_line in [bl[0] for bl in self.line_list]:
                board_line_coor = list(board_line)
                if board_line == line:
                    print("line already taken")
                    return False
                if board_line_coor[0] == new_coor_list[0] or board_line_coor[1] == new_coor_list[0] or \
                        board_line_coor[0] == new_coor_list[1] or board_line_coor[1] == new_coor_list[1]:
                    new_piece_touching = True

        if first_move:
            return True

        if not new_piece_touching:
            print("New Piece Doesn't touch any previous pieces")
            return False

        """
            check that we don't cross another line already on the board.
            We do this by creating "extended lines" which are pairs of original lines that extend each other on the same
            axis. Then we check if these extended lines cross each other
        """
        for line in self.find_extended_lines([(line, -1) for line in p.shape]):  # -1 since from same piece
            new_coor_list = list(line)
            for board_line in self.find_extended_lines([(bl[0], bl[2]) for bl in self.line_list]):
                board_line_coor = list(board_line)

                if (  # check if old line is between the new x values
                        (new_coor_list[0][0] < board_line_coor[0][0] < new_coor_list[1][0] or
                         new_coor_list[1][0] < board_line_coor[0][0] < new_coor_list[0][0]) and (
                                # check if new line is between the old y values
                                board_line_coor[0][1] < new_coor_list[0][1] < board_line_coor[1][1] or
                                board_line_coor[1][1] < new_coor_list[0][1] < new_coor_list[0][1])
                ) or (  # check if new line is between the old x values
                        (board_line_coor[0][0] < new_coor_list[0][0] < board_line_coor[1][0] or
                         board_line_coor[1][0] < new_coor_list[0][0] < board_line_coor[0][0]) and (
                                # check if old line is between the new y values
                                new_coor_list[0][1] < board_line_coor[0][1] < new_coor_list[1][1] or
                                new_coor_list[1][1] < board_line_coor[0][1] < new_coor_list[0][1])):
                    print("Piece Crosses a previous piece")
                    return False
        return True

    def check_piece_placement_wrapper(self, piece_num, permutation_index, coordinates, first):
        new_piece = Piece(piece_num)
        new_piece = self.piece_permutation(new_piece, permutation_index)
        new_piece.add_coordinates(coordinates[0], coordinates[1])
        return self.check_piece_placement(new_piece, first)

    def add_piece(self, player_num: int, piece_num=1, permutation_index=1, coordinates=(15, 15), first=False):
        if 1 <= piece_num <= 16 and 1 <= permutation_index <= 8:

            if self.check_piece_placement_wrapper(piece_num, permutation_index, coordinates, first):
                new_piece = Piece(piece_num)
                new_piece = self.piece_permutation(new_piece, permutation_index)
                new_piece.add_coordinates(coordinates[0], coordinates[1])
                new_id = self.gen_piece_id()
                for line in new_piece.shape:
                    self.line_list.append((line, player_num, new_id))
                self.new_board = False
                self.last_piece = new_piece
                print("Piece added to board")
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
