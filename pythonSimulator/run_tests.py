import unittest
from eventgame import EventGame
from player import Player
import os
import json


def initialise_game():
    player_list = list()
    for i in range(0, 4):
        player_list.append(Player(f"Player_{i + 1}"))
    game = EventGame(player_list)
    return game
class TestSquarePlay(unittest.TestCase):
    def __init__(self, testName, test_path):
        super(TestSquarePlay, self).__init__(testName)
        self.test_path = test_path

    def test(self):
        print("\n---------testing file", self.test_path, "---------")
        game = initialise_game()
        game_path = os.path.join("./pythonBackend/tests", self.test_path)
        results = game.load_game(game_path)
        with open(game_path, "r") as game_json:
            data = json.load(game_json)
        self.assertEqual(data["expect"], results)


if __name__ == '__main__':
    suite = unittest.TestSuite()
    for filename in os.listdir("./pythonBackend/tests"):
        if filename.endswith(".json"):
            suite.addTest(TestSquarePlay('test', filename))
    unittest.TextTestRunner(verbosity=2).run(suite)
