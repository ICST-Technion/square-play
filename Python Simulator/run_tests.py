import unittest
from eventgame import EventGame
from player import Player
import os

def initialise_game(path):
  player_list = list()
  for i in range(0, 4):
    player_list.append(Player(f"Player_{i + 1}"))
  game = EventGame(player_list)
  moves = parse_file(path)
  game.start_game(moves[0][1][0], moves[0][1][1])
  board = moves[0][2]
  return (game, board, moves[1:])

def parse_move(move):
  #split line to move, result and status code:
  move_list = move.split("||")
  #get move type
  move_list[0] = int(move_list[0])
  # create list for give up move
  if(move_list[0]==0):
    move_list[1] = int(move_list[1])
    move_list[2] = []
  else:
    # create list for regular move
    # create move parameters list
    move_list[1] = list(map(lambda param: int(param), move_list[1].split(",")))
    # split result to lines
    if(move_list[2]!=" "):
      move_list[2] = move_list[2].split("//")
      for i in range(len(move_list[2])):
        #split each line to 2 points
        move_list[2][i] = move_list[2][i].split("-")
        #set first point to be a tuple of 2 coordinates
        first_point = move_list[2][i][0].split(",")
        move_list[2][i][0] = (int(first_point[0]), int(first_point[1]))
        #set second point to be a tuple of 2 coordinates
        second_point = move_list[2][i][1].split(",")
        move_list[2][i][1] = (int(second_point[0]), int(second_point[1]))
    else:
      move_list[2]=[]
  #set expected status code
  move_list[3] = int(move_list[3])
  #so now a move is described by a list of:
  # 0. the move type
  # 1. the move itself, which is a list of the arguments for the move function of the game
  # 2. the result, which is a list of lines, where each lines is a list of points, where each point is a tuple of 2 coordinates.
  # 3. the status code
  return move_list

def parse_file(path):
  f = open(os.path.join("./Python Simulator/tests", path), "r")
  moves_text = f.read()
  return list(map(parse_move, moves_text.splitlines()))
  

def get_game_board(game):
  board = game.get_board()
  line_list = board.line_list
  lines = map(lambda line: frozenset(line[0]), line_list)
  return lines

class TestSquarePlay(unittest.TestCase):
  def __init__(self, testName, test_path):
        super(TestSquarePlay, self).__init__(testName)
        self.test_path = test_path

  def test(self):
    (game, board, moves) = initialise_game(self.test_path)
    for move in moves:
      if(move[0]==0):
        code = game.pass_turn(move[1])
      else:
        code = game.move(move[1][0], move[1][1], move[1][2], move[1][3], move[1][4])
        board.extend(move[2])
      self.assertEqual(code, move[3])
      self.assertEqual(frozenset(get_game_board(game)), frozenset(map(lambda line: frozenset(line), board)))

if __name__ == '__main__':
  suite = unittest.TestSuite()
  print(os.getcwd())
  for filename in os.listdir("./Python Simulator/tests"):
    suite.addTest(TestSquarePlay('test', filename))
  unittest.TextTestRunner(verbosity=2).run(suite)
