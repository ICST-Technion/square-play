import copy
import random
import time
from tkinter import *
from pieces import Piece
from board import Board

class ChallengeGenerator:
    def __init__(self):
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

    def GenerateChallenge(self, c_size,pices):
        '''
        :param c_size: how many pieces to use
        :param pices: what pieces can i use
        :return: challenge
        '''
        chall = []
        board = Board()
        first_piece = True
        for i in range(c_size):
            #choose piece
            random_index = random.randint(0, len(pices) - 1)
            piece_to_use = pices[random_index]
            del pices[random_index]

            if first_piece:
                random_index = random.randint(0, len(self.shape_permutation[piece_to_use]) - 1)
                permutation = self.shape_permutation[piece_to_use][random_index]
                board.add_piece(0,piece_to_use,permutation,(15,15),True)
                chall.append((piece_to_use,permutation,15,15))
                first_piece = False
            else:
                #choose location and premutation
                piece_possible_places = []
                x_min, x_max, y_min, y_max = self.possible_coordinates_range(board)
                for permutation in self.shape_permutation[piece_to_use]:
                    for game_cord_x in range(x_min, x_max):
                        for game_cord_y in range(y_min, y_max):
                            if board.check_piece_placement_wrapper(piece_to_use, permutation, (game_cord_x, game_cord_y),
                                                                   False):
                                new_board = copy.deepcopy(board)
                                new_board.add_piece(0, piece_to_use, permutation,(game_cord_x, game_cord_y))
                                piece_possible_places.append((permutation, game_cord_x, game_cord_y,new_board.new_squares()))
                piece_possible_places.sort(key=lambda x: x[3],reverse=True)
                random_index = random.randint(0, min(len(piece_possible_places) - 1, 5))
                permutation, x, y, _ = piece_possible_places[random_index]
                board.add_piece(0, piece_to_use, permutation,(x,y))
                chall.append((piece_to_use, permutation, x, y))
            board.print_board()
        return chall



    def print_solution(self,chall):
        game_board = Board()
        first = True
        for (piece, permutation, x, y) in chall:
            if first:
                first = False
                game_board.add_piece(0, piece, permutation, (x, y), True)
            else:
                game_board.add_piece(0, piece, permutation, (x, y))

        root = Tk()
        root.attributes('-fullscreen', True)
        root.title('Square Play Board')
        root.geometry("500x500")

        board = Canvas(root, width=1000, height=1000, bg="grey")
        board.pack(pady=20)
        lbl = 1
        for idx,shape in enumerate(game_board.shapes):
            ##########33
            if shape[0] == game_board.last_piece.shape:
                lbl = shape[1]
            ############
            color = 'blue'
            if idx % 16 == 0:
                color = 'black'
            if idx % 16 == 1:
                color = 'red'
            if idx % 16 == 2:
                color = 'green'
            if idx % 16 == 3:
                color = 'blue'
            if idx % 16 == 4:
                color = 'cyan'
            if idx % 16 == 5:
                color = 'yellow'
            if idx % 16 == 6:
                color = 'magenta'
            if idx % 16 == 7:
                color = 'navajo white'
            if idx % 16 == 8:
                color = 'violet red'
            if idx % 16 == 9:
                color = 'SlateBlue1'
            if idx % 16 == 10:
                color = 'green yellow'
            if idx % 16 == 11:
                color = 'gold'
            if idx % 16 == 12:
                color = 'navy'
            if idx % 16 == 13:
                color = 'gray'
            if idx % 16 == 14:
                color = 'SkyBlue4'
            if idx % 16 == 15:
                color = 'coral'
            needed_shape = shape[0]  # get_needed_coords(shape[0])
            for line in needed_shape:
                iterator = iter(line)
                point1 = next(iterator, None)
                point2 = next(iterator, None)
                board.create_line(point1[0] * 30 + 50, board.winfo_reqheight() - (point1[1] * 30) - 150,
                                  point2[0] * 30 + 50, board.winfo_reqheight() - (point2[1] * 30) - 150, fill=color,
                                  width=5)


        # root.mainloop()
        root.update_idletasks()
        root.update()

    def possible_coordinates_range(self,board):
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

        x_min = max(0,x_min-3)
        y_min = max(0,y_min-3)
        x_max = min(32,x_max+3)
        y_max = min(32,y_max+3)
        return x_min,x_max,y_min,y_max


if __name__ == '__main__':
    ch = ChallengeGenerator()
    c = ch.GenerateChallenge(10,[i for i in range(1,17)])
    time.sleep(5)
    ch.print_solution(c)
    time.sleep(10)
