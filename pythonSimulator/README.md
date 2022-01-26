 
 # Square Game python simulator
 
 ### Written by Ron Rubinstein

 This is a proof of concept for the square game project.


 ## The board is defined as the following:

 The playing board is effectively an `x,y` grid.

 Each piece can take upto 2 units in length/height.
 Each player has 16 such pieces, and there are upto 4 players.
 The real max possible length/height is `16*4*2` = 128, but such a setup makes no sense according to each
 player's motivations.
 On the other end, the optimal setup would be a square grid of size: `12 * 12` since there are upto 128 squares
 on the board.

Since good gameplay should be closer to an optimal setup,
I decided to define the board as a `32 * 32` grid (indexes 0-32), where the first piece is always placed
at its base position (see piece definitions below), with its "Zero point" at coordinates: `(15,15)`

## The pieces are defined as the following:

Each piece is essentially a collection of lines that can fit inside a digital number counter.
This means that we can define each piece as digital shape object where each of the 7 possible lines either
appears or don't.

We define a **"Zero point"** for each piece (in its base permutation), which will be the point of reference for placement
on the board. Additionally, each piece has upto 8 permutations.
90 degree rotations flip the x/y-axis of the shape as well as the sign of one axis.
Flips change the sign of either the x or the y-axis.

**Example:**

In the piece 7, we define the zero point as the upper left point, meaning that the shape is essentially:

`{[(0,0), (1,0)], [(1,0),(1,-1)], [(1,-1), (1,-2)]}`

if we rotate 90 degrees right we flip x/y-axis, and then flip the "NEW" y-axis sign and get:

`{[(0,0), (0,-1)], [(0,-1),(-1,-1)], [(-1,-1), (-2,-1)]}`

if we flip using the x-axis as a mirror we get a sign change on the x-axis:

`{[(0,0), (-1,0)], [(-1,0),(-1,-1)], [(-1,-1), (-1,-2)]}`

We can reach any permutation with these two operators relative to the base piece, meaning we can
reach any legal placement on the board with a number 1-3 of 90 degree right rotations and x-axis flips

## Board placement is defined as the following:

After defining our board and our pieces, our piece placement works as following:

Each piece has its initial setup of points and lines as explained above. Additionally, the user inserts the
number of 90 degree rotations he wants, if he wants to flip the piece, and finally the "zero point" placement
of the piece on the board.

**Example:**

Lets take the piece J (a flipped and rotated 180 degrees 7), where we want the "zero point" (which in this case
would turn out to be the bottom left point of the shape) to be at point (14,15).

The user input should be 

`<shape code 1-16> <Rotate Right number 1-3> <X axis flip 0/1> <Base point: (x,y)>`

Then we calculate the shape placement using our 2 permutation operators, and finally add the base point to
ALL coordinates in the piece.

The stages are:

1. Base shape:

`{[(0,0), (1,0)], [(1,0),(1,-1)], [(1,-1), (1,-2)]}`

2. 2 rotation operators in a row:

`{[(0,0), (-1,0)], [(-1,0),(-1,1)], [(-1,1), (-1,2)]}`

3. X-axis flip:

`{[(0,0), (1,0)], [(1,0),(1,1)], [(1,1), (1,2)]}`

4. Base Point addition:

`{[(14,15),(15,15)], [(15,15),(15,16)], [(15,16),(15,17)]}`

Meaning this will be the shape placement on the board.

To manage legal placements, we can check that all points in the result are in the range of the board
Additionally the board itself will have a collection of taken lines, and their color.
Before adding a shape we check that all the piece's lines don't appear on the board already.

To check if a new square has appeared on the board we can search for the 8 possible new squares that could
be created as a result of the piece placement, as well as checking that the square has at least one
line from the new piece and one from beforehand.

## Load and Store:

You can save an ongoing game by calling the function store_game() of EventGame. This function will save the current game in a `json` format to the file name you pass as an argument. Please choose a unique file name so that you won't override other saved games. The file will be saved in `saved_games` folder.

A saved game is a json in this format:
```
{
  "board": []
  "moves: [
    {
      "type":
      "player_num": ,
      "piece_num":  ,
      "permutation": ,
      "x": ,
      "y": 
    }
  ]
}
```

Meaning a saved game is an object with a list describing the board and a list of moves, where each move is described by:

1. type - describes weather this is a regular move (1) or a "pass turn" move (0).
2. player_num - the number describing the player who performs the move.
3. piece_num - the number describing the piece that is added to the board
4. permutation - a number describing the rotation of the piece
5. x and y - the coordinates on the board to locate the piece

Note that the board list describes the initial board of the game - **The board list is always empty for saved games, do not change it!**

You can load an old game to continue by using the game.load_game function. Pass the saved game file path as an argument.
## Tests:

In order to run the tests, run the `run_tests.py` file. It will run all the tests, that are located in folder Python Simulator/tests.

### Test structure:

Each file in this folder is a `.json` file, that represents a test.

A test is similar to a saved game, it is a json in this format:
```
{
  "board": [
    {
      "player_num": ,
      "piece_num":  ,
      "permutation": ,
      "x": ,
      "y": 
    }
  ],
  "moves: [
    {
      "type":
      "player_num": ,
      "piece_num":  ,
      "permutation": ,
      "x": ,
      "y": 
    }
  ],
  expect: []
}
```
The list of moves is formatted as described above.

The board is a list of pieces - described by:

1. player_num - the number describing the player who performs the move.
2. piece_num - the number describing the piece that is added to the board
3. permutation - a number describing the rotation of the piece
4. x and y - the coordinates on the board to locate the piece

You can add pieces to the board to start the test from a specific initial board, otherwise leave the board empty to start a test from an empty board.

The expect list is a list of expacted status codes that will be returned by each move call (each move in the moves list)

**Status codes** are described as follows:
1. -1 is for an error
2. 0 is for a successful pass_turn move
3. 1 represents that the player has another move
4. 2 represents that the turn moves to the next player
5. 3 represents that we are in the final round
6. 5 represents that game is over
*Note:* for a give up move this can never be 1

**Example:**

Let's look at the `load_board.json` test:

```
{
  "board": [
    {
      "player_num": 4,
      "piece_num": 13,
      "permutation": 4,
      "x": 15,
      "y": 15
    },
    {
      "player_num": 1,
      "piece_num": 9,
      "permutation": 1,
      "x": 16,
      "y": 16
    },
    {
      "player_num": 1,
      "piece_num": 5,
      "permutation": 2,
      "x": 17,
      "y": 15
    }
  ],
  "moves": [
      {
        "type": 1,
        "player_num": 1,
        "piece_num": 8,
        "permutation": 4,
        "x": 13,
        "y": 15
      },
      {
        "type": 0,
        "player_num": 2
      },
      {
        "type": 1,
        "player_num": 3,
        "piece_num": 5,
        "permutation": 7,
        "x": 15,
        "y": 16
      }
  ],
  "expect": [2, 0, -1]
}
```

In this test we want to have an initial board that contains 3 pieces - piece 13 of player 4, piece 9 of player 1 and piece 5 of player 1.
The pieces locaion and permutatiom is described by the permutation, x, y fields.

We then perform 3 moves - and we expect the first move to return 2, the second move to return 0 and the third to return -1.
### Add a new test:

Just create a new txt file, for example `test5.json`, and write the board, moves and expect as described above.
*Note*: the first move is constructed a bit differently, it has only 2 arguments - the piece and the permutation, since the player is known (the last player) and also the location (15,15).
*Another Note*: you must add the file to tests folder in order for it to run.