 
 Square Game python simulator written by Ron Rubinstein

 This is a proof of concept for the square game project.


 The board is defined as the following:

 The playing board is effectively an x,y grid.

 each shape can take upto 2 units in length/height.
 each player has 16 such shapes, and there are upto 4 players.
 The real max possible length/height is 16*4*2 = 128, but such a setup makes no sense according to each
 player's motivations.
 On the other end, the optimal setup would be a square grid of size: 16 * 16 since there are upto 256 squares
 on the board.

Since good gameplay should be closer to an optimal setup,
I decided to define the board as a 32 * 32 grid (indexes 0-32), where the first piece is always placed
at its base position (see piece definitions below), with its "Zero point" at coordinates: (15,15)

The pieces are defined as the following:

each piece is essentially a collection of lines that can fit inside a digital number counter.
This means that we can define each piece as digital shape object where each of the 7 possible lines either
appears or don't.

We define a "Zero point" for each piece (in its base permutation), which will be the point of reference for placement
on the board. Additionally, each piece has upto 8 permutations.
90 degree rotations flip the x/y-axis of the shape as well as the sign of one axis.
Flips change the sign of either the x or the y-axis.

For example, in the piece 7, we define the zero point as the upper left point, meaning that the shape is essentially:

{ [(0,0), (1,0)], [(1,0),(1,-1)], [(1,-1), (1,-2)] }

if we rotate 90 degrees right we flip x/y-axis, and then flip the "NEW" y-axis sign and get:

 { [(0,0), (0,-1)], [(0,-1),(-1,-1)], [(-1,-1), (-2,-1)] }

if we flip using the x-axis as a mirror we get a sign change on the x-axis:

{ [(0,0), (-1,0)], [(-1,0),(-1,-1)], [(-1,-1), (-1,-2)] }

We can reach any permutation with these two operators relative to the base piece, meaning we can
reach any legal placement on the board with a number 1-3 of 90 degree right rotations and x-axis flips

Board placement is defined as the following:

After defining our board and our pieces, our piece placement works as following:

Each piece has its initial setup of points and lines as explained above. Additionally, the user inserts the
number of 90 degree rotations he wants, if he wants to flip the piece, and finally the "zero point" placement
of the piece on the board.

For example:
Lets take the piece J (a flipped and rotated 180 degrees 7), where we want the "zero point" (which in this case
would turn out to be the bottom left point of the shape) to be at point (14,15).

The user input should be <shape code 1-16> <Rotate Right number 1-3> <X axis flip 0/1> <Base point: (x,y)>

Then we calculate the shape placement using our 2 permutation operators, and finally add the base point to
ALL coordinates in the piece.

The stages are:
Base shape:

{ [(0,0), (1,0)], [(1,0),(1,-1)], [(1,-1), (1,-2)] }

2 rotation operators in a row:

{ [(0,0), (-1,0)], [(-1,0),(-1,1)], [(-1,1), (-1,2)] }

X-axis flip:

{ [(0,0), (1,0)], [(1,0),(1,1)], [(1,1), (1,2)] }

Base Point addition:

{ [(14,15),(15,15)], [(15,15),(15,16)], [(15,16),(15,17)] }

Meaning this will be the shape placement on the board.

To manage legal placements, we can check that all points in the result are in the range of the board
Additionally the board itself will have a collection of taken lines, and their color.
Before adding a shape we check that all the piece's lines don't appear on the board already.

To check if a new square has appeared on the board we can search for the 8 possible new squares that could
be created as a result of the piece placement, as well as checking that the square has at least one
line from the new piece and one from beforehand.
