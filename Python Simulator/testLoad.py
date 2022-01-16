from eventgame import EventGame
from player import Player

if __name__ == '__main__':
  print("Welcome to Square Game")
  player_list = list()
  for i in range(0, 4):
    player_list.append(Player(f"Player_{i + 1}"))
  test_Game = EventGame(player_list)
  results = test_Game.load_game('./Python Simulator/tests/test1.json')
  assert(results==[2, 0, -1])
  result = test_Game.move(3, 5, 8, 13, 15)
  assert(result==1)
  player_list2 = list()
  for i in range(0, 4):
    player_list2.append(Player(f"Player_{i + 1}"))
  test_Game2 = EventGame(player_list2)
  results = test_Game2.load_game('./Python Simulator/tests/test2.json')
  assert(results==[2,0,0,0,0,1,1,2,2,1,1,1,2,0,0,0,0,1,1,1,1,2,1,1,2,1,1,1,1,1,1,2,1,1,1,1,1,1,1,2,2,2,2,1,1,1,3,5])
  print("---passed----")