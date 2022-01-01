 
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

## Tests:

In order to run the tests, run the `run_tests.py` file. I will run all the tests, that are located in folder Python Simulator/tests.

### Test structure:

Each file in this folder is a `.txt` file, that represents a test.

In every file, each line represents a move, and consists of 4 parts - move type, move, result, and status code, seperated by `||` seperator.

**Move type:** a regular move (1) or a "give up" move (0)

**Move:** for a regular move described by 5 parameters - player, piece number, permutation, x-coordinate and y-coordinate, which are seperated by `,`. For a give up move describes the player number. 

**Result** describes the lines that are added to the board after the move is performed. Different lines are seperated by `//`, 2 points on the same line are seperated by `-`, and x-coordinate is seperated from y-coordinate by `,` for each point. *Note:* for a give up move this should always be empty

**Status code** describes the meaning of result of the move:
1. -1 is for an error
2. 0 is for a successful pass_turn move
3. 1 represents that the player has another move
4. 2 represents that the turn moves to the next player
5. 3 represents that we are in the final round
6. 5 represents that game is over
*Note:* for a give up move this can never be 1

**Example:**

In test1, we have the line:

`1||1,9,1,16,16||16,14-16,15//15,15-16,15//15,15-15,16||1`

Let's Explain this line:

As described above, we seperate the line to 4 parts, seperated by `||`:

1. Move type is 1 - which means we execute a regular move.

2. The move is `1,9,1,16,16` - this means that the move was performed by player 1, using piece number 4, and laying it in location `16,16` without rotating or flipping it.

3. result is `16,14-16,15//15,15-16,15//15,15-15,16`, which are the lines that we expect that will be added to the board. In this case, we expect a line between `16,14` and `16,15`, a line between `15,15` and `16,15`, and a line between `15,15` and `15,16` to be added to the board.

4. Status code is 1, so we expect player 1 to have another turn.
### Add a new test:

Just create a new txt file, for example test5.txt, and write the moves as described above.
*Note*: the first move is constructed a bit differently, it has only 2 arguments - the piece and the permutation, since the player is known (the last player) and also the location (15,15).
*Another Note*: you must add the file to tests folder in order for it to run.