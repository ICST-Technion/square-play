"""
Here we define pieces and their base coordinates

each piece's shape is a list of sets of tuples which are points on the grid
"""


class Piece:
    def __init__(self, piece_num):
        self.shape = []
        self.num = -1
        if 1 <= piece_num <= 16:
            self.num = piece_num
        else:
            print("Illegal piece entered")
            return
        """
        notice that the only possible new squares after a piece placement are the squares that share an edge with
        the digital 2X1 unit rectangle of the piece we placed.
        We make the base coordinates according to the zero point in all the pieces (top right corner), and
        we change the coordinates together with the piece according to the permutations
        """
        self.possible_squares = [
            [{(0, 0), (0, 1)}, {(0, 1), (-1, 1)}, {(-1, 1), (-1, 0)}, {(-1, 0), (0, 0)}],
            [{(0, 0), (1, 0)}, {(1, 0), (1, -1)}, {(1, -1), (0, -1)}, {(0, -1), (0, 0)}],
            [{(0, 0), (0, -1)}, {(0, -1), (-1, -1)}, {(-1, -1), (-1, 0)}, {(-1, 0), (0, 0)}],
            [{(-1, 0), (-1, -1)}, {(-1, -1), (-2, -1)}, {(-2, -1), (-2, 0)}, {(-2, 0), (-1, 0)}],
            [{(-1, -2), (-1, -1)}, {(-1, -1), (-2, -1)}, {(-2, -1), (-2, -2)}, {(-2, -2), (-1, -2)}],
            [{(-1, -2), (0, -2)}, {(0, -2), (0, -1)}, {(0, -1), (-1, -1)}, {(-1, -1), (-1, -2)}],
            [{(0, -2), (1, -2)}, {(1, -2), (1, -1)}, {(1, -1), (0, -1)}, {(0, -1), (0, -2)}],
            [{(-1, -2), (0, -2)}, {(0, -2), (0, -3)}, {(0, -3), (-1, -3)}, {(-1, -3), (-1, -2)}]
        ]

        if piece_num == 1:  # this piece is shaped like a 6. Zero point is the top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(-1, -1), (0, -1)}, {(0, -1), (0, -2)},
                          {(0, -2), (-1, -2)}, {(-1, -2), (-1, -1)}]
        if piece_num == 2:  # this piece is shaped like an H. Zero point is top right corner
            self.shape = [{(0, 0), (0, -1)}, {(-1, 0), (-1, -1)},
                          {(-1, -1), (0, -1)}, {(0, -1), (0, -2)},
                          {(-1, -2), (-1, -1)}]
        if piece_num == 3:  # this piece is shaped like an 5 without bottom line. Zero point is top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(-1, -1), (0, -1)}, {(0, -1), (0, -2)}]

        if piece_num == 4:  # this piece is shaped like a 6 without bottom line. Zero point is the top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(-1, -1), (0, -1)}, {(0, -1), (0, -2)},
                          {(-1, -2), (-1, -1)}]
        if piece_num == 5:  # this piece is shaped like a F. Zero point is the top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(-1, -1), (0, -1)}, {(-1, -2), (-1, -1)}]

        if piece_num == 6:  # this piece is shaped like a E. Zero point is the top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(-1, -1), (0, -1)}, {(0, -2), (-1, -2)},
                          {(-1, -2), (-1, -1)}]
        if piece_num == 7:  # this piece is shaped like a P. Zero point is the top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(0, 0), (0, -1)}, {(-1, -1), (0, -1)},
                          {(-1, -2), (-1, -1)}]
        if piece_num == 8:  # this piece is shaped like a 8 without bottom line. Zero point is the top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(0, 0), (0, -1)}, {(-1, -1), (0, -1)},
                          {(0, -1), (0, -2)}, {(-1, -2), (-1, -1)}]
        if piece_num == 9:  # this piece is shaped like Piece 3 without top line. Zero point is top right corner
            self.shape = [{(-1, 0), (-1, -1)}, {(-1, -1), (0, -1)},
                          {(0, -1), (0, -2)}]

        if piece_num == 10:  # this piece is shaped like E without top and bottom line. Zero point is top right corner
            self.shape = [{(-1, 0), (-1, -1)}, {(-1, -1), (0, -1)},
                          {(-1, -2), (-1, -1)}]

        if piece_num == 11:  # this piece is shaped like an 5. Zero point is top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(-1, -1), (0, -1)}, {(-1, -2), (0, -2)},
                          {(0, -1), (0, -2)}]
        if piece_num == 12:  # this piece is shaped like Piece 4 without top line. Zero point is the top right corner
            self.shape = [{(-1, 0), (-1, -1)}, {(-1, -1), (0, -1)},
                          {(0, -1), (0, -2)}, {(-1, -2), (-1, -1)}]

        if piece_num == 13:  # this piece is shaped like a 6 without middle line. Zero point is the top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(0, -1), (0, -2)}, {(0, -2), (-1, -2)},
                          {(-1, -2), (-1, -1)}]
        if piece_num == 14:  # this piece is shaped like a E without middle line. Zero point is the top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(0, -2), (-1, -2)}, {(-1, -2), (-1, -1)}]

        if piece_num == 15:  # this piece is shaped like a F without middle line. Zero point is the top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(-1, 0), (-1, -1)},
                          {(-1, -2), (-1, -1)}]

        if piece_num == 16:  # this piece is shaped like a P without middle line. Zero point is the top right corner
            self.shape = [{(0, 0), (-1, 0)}, {(0, 0), (0, -1)},
                          {(-1, -1), (0, -1)}, {(-1, -2), (-1, -1)}]

    def rotate_90_right(self):
        # first we switch the x and y axis, and then flip new y axis sign
        new_shape = list()
        for (a, b), (c, d) in self.shape:
            new_shape.append({(b, -1 * a), (d, -1 * c)})
        self.shape = new_shape

        new_possible_squares = list()
        for sq in self.possible_squares:
            new_sq = list()
            for (a, b), (c, d) in sq:
                new_sq.append({(b, -1 * a), (d, -1 * c)})
            new_possible_squares.append(new_sq)
        self.possible_squares = new_possible_squares

    def x_axis_flip(self):
        # flip the x axis sign
        new_shape = list()
        for (a, b), (c, d) in self.shape:
            new_shape.append({(-1 * a, b), (-1 * c, d)})
        self.shape = new_shape

        new_possible_squares = list()
        for sq in self.possible_squares:
            new_sq = list()
            for (a, b), (c, d) in sq:
                new_sq.append({(-1 * a, b), (-1 * c, d)})
            new_possible_squares.append(new_sq)
        self.possible_squares = new_possible_squares

    def permutate(self, rotates=0, flip=False):
        if flip:
            self.x_axis_flip()

        for i in range(rotates):
            self.rotate_90_right()

    def add_coordinates(self, x_val: int, y_val: int):
        new_shape = list()
        for (a, b), (c, d) in self.shape:
            new_shape.append({(a + x_val, b + y_val), (c + x_val, d + y_val)})
        self.shape = new_shape

        new_possible_squares = list()
        for sq in self.possible_squares:
            new_sq = list()
            for (a, b), (c, d) in sq:
                new_sq.append({(a + x_val, b + y_val), (c + x_val, d + y_val)})
            new_possible_squares.append(new_sq)
        self.possible_squares = new_possible_squares

    def get_possible_squares(self):
        return self.possible_squares
