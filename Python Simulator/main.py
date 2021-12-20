from eventgame import EventGame
from interactivegame import InteractiveGame
from player import Player

if __name__ == '__main__':
    # ---------------------- INTERACTIVE GAME ----------------------
    print("Welcome to Square Game")
    number_of_players = int(input("Enter number of players, between 2 and 4:\n"))
    while not 2 <= number_of_players <= 4:
        number_of_players = int(input("Invalid Number of players, try again"))
    player_list = list()
    for i in range(0, number_of_players):
        player_list.append(Player(f"Player_{i + 1}"))
    test_Game = InteractiveGame(player_list)
    test_Game.gameplay()
    # ---------------------- EVENT GAME ----------------------
    """
    print("Welcome to Square Game")
    number_of_players = int(input("Enter number of players, between 2 and 4:\n"))
    while not 2 <= number_of_players <= 4:
        number_of_players = int(input("Invalid Number of players, try again"))
    player_list = list()
    for i in range(0, number_of_players):
        player_list.append(Player(f"Player_{i + 1}"))
    test_Game = EventGame(player_list)
    test_Game.start_game(15, 1)
    test_Game.move(1, 10, 1, 15, 13)
    test_Game.move(2, 10, 1, 15, 13)
    """