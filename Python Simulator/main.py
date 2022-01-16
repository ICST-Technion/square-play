
from player import Player
from board import Board

if __name__ == '__main__':
    print("Welcome to Square Game")
    number_of_players = int(input("Enter number of players, between 2 and 4:\n"))
    while not 2 <= number_of_players <= 4:
        number_of_players = int(input("Invalid Number of players, try again"))
    player_list = list()
    for i in range(0, number_of_players):
        player_list.append(Player("Player" + str(i + 1)))

    game_board = Board()

    winners = []

    while True:
        player = player_list[-1]  # so that he will always have an extra turn at the end if someone else wins, since
        # the for loop for the turns begins at the first player
        if game_board.new_board:
            print(player.name + "'s Turn, choose piece and permutation, placement is predefined")
            pn, pr = input("Syntax is: <piece num: 1-16> <permutation: 1-8>:\n").split()
            pn = int(pn)
            pr = int(pr)
            if player.check_piece(pn) and game_board.add_piece(len(player_list), pn, pr):
                player.remove_piece(pn)
                break
            else:
                print("Try again")

    while True:
        for count, player in enumerate(player_list):
            # for loop for player turns
            # each player has one move at the start of each turn
            player.add_moves()

            while True:
                print(player.name + "'s Move, choose piece, permutation, placement")
                pn, pr, x, y = input("Syntax is: <piece num: 1-16> <permutation: 1-8> <X coordinate : 1-32> <Y "
                                     "coordinate 1-32)>:\n").split()
                pn = int(pn)
                pr = int(pr)
                coordinates = (int(x), int(y))

                if player.check_piece(pn) and game_board.add_piece(count + 1, pn, pr, coordinates):
                    player.remove_piece(pn)
                    new_squares = game_board.new_squares()

                    if new_squares != 0:
                        player.add_moves(new_squares - 1)
                        # print(new_squares)
                        print(player.name + " got " + str(new_squares - 1) + " more moves with a total of " + str(
                            player.moves_left()) + " left")

                    if player.is_player_finished():
                        print("Final Round for players with less turns")
                        winners.append(player)
                        break

                    if player.moves_left() < 1:  # end of turn for player
                        break
        if len(winners) != 0:
            break

    print("Congratulations, the winner/s are: ")
    num_turns = 0
    for player in winners:
        num_turns = player.turns_played
        print(player.name)
    print("Which finished the game in " + str(num_turns) + " Turns")
