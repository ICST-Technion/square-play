using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class BaseShape : MonoBehaviour
{
    protected Vector3 mOffset;
    protected float mZCoord;

    public Vector3 startingPosition;

    public ShapesManager shapeManager;

    //This is very important! It is used in order to correctly position the shape on the new cells.
    public List<GameObject> assemblingLines = new List<GameObject>();

    protected CellClass nearestCell = null; //tbd: need to not only highlight the nearest cell, but also the nearest cell to each line

    public int piece_num;

    public int permutation = 1;

    public bool isFinalPos = false;

    public bool chooseCellForMe = false;

    public int playerNum;


    //protected List<List<CellClass>> hintCells = new List<List<CellClass>>();//This is for the helper.

    public virtual void Setup(Color newTeamColor, ShapesManager newshapeManager)
    {
        this.shapeManager = newshapeManager;
        this.transform.SetParent(this.shapeManager.canvasTrans);
        this.transform.localScale = new Vector3(this.shapeManager.gameScale, this.shapeManager.gameScale, 0);
        this.transform.localRotation = Quaternion.identity;
        int.TryParse(this.name.Split('_')[1], out this.playerNum);
        this.playerNum -= 1;
        this.permutation = 1;
        foreach (var line in this.assemblingLines)
        {
            line.GetComponent<LineRenderer>().startColor = newTeamColor;
            line.GetComponent<LineRenderer>().endColor = newTeamColor;
            line.GetComponent<LineRenderer>().widthMultiplier = 0.6f;
        }
    }

    private Transform startTransformation;
    public void setupStartPos(float x, float y, Transform startTrans)
    {
        var pos = new Vector3(x, y);
        this.transform.localPosition = pos;
        this.startTransformation = startTrans;
        this.startingPosition = pos;
    }

    #region Movement

    public async Task<int> Move()
    {
        if (!this.checkPositionInBoard())
        {
            return -1;
        }
        this.nearestCell = this.getNearesetCell();
        int new_position_x = this.nearestCell.x;
        int new_position_y = this.nearestCell.y;


        var response = await this.shapeManager.sendMove(
            piece_num, permutation, new_position_x, new_position_y);
        if (response[0] != -1)
        {
            var numOfClosed = response[1];
            this.shapeManager.currentPlayerClosedSquares(numOfClosed);
        }
        return response[0];
    }

    private void rotateByPermutation(int permutation)
    {
        this.permutation = permutation;
        int[] rot = this.getRotationByPermutation(permutation);
        this.transform.Rotate(new Vector3(rot[0], rot[1], rot[2]), Space.Self);
    }

    private int[] getRotationByPermutation(int permutation)
    {
        //By default, there is a maximum of 8 permutations.
        switch (permutation)
        {
            case 2:
                return new int[] { 0, 0, 270, 0 };
            case 4:
                return new int[] { 0, 0, -270, 0 };
            case 3:
                return new int[] { 0, 0, 180, 0 };
            case 5:
                return new int[] { 0, 180, 0, 0 };
            case 8:
                return new int[] { 0, 180, 270, 0 };
            case 9:
                return new int[] { 0, 180, -270, 0 };
            case 7:
                return new int[] { 180, 0, 0, 0 };
            case 6:
                return new int[] { 180, 0, 270, 0 };
            case 10:
                return new int[] { 180, 0, -90, 0 };
            default:
                return new int[] { 0, 0, 0, 0 }; ;
        }
    }

    private int[] getPosAdditionByPerm(int permutation)
    {
        //By default, there is a maximum of 8 permutations.
        switch (permutation)
        {
            case 2:
                return new int[] { -200, -300 };
            case 3:
                return new int[] { -10, -170 };
            case 4:
                return new int[] { -10, -270 };
            case 5:
                return new int[] { 200, -100 };
            case 6:
                return new int[] { 200, -300 };
            case 7:
                return new int[] { 450, -170 };
            case 8:
                return new int[] { 450, -270 };
            default:
                return new int[] { -200, -100 }; ;
        }
    }


    void Update()
    {
        /* if (rotateMe && this.shapeManager.gameManager.chosenRotation != -1)
         {
             print("rotating the shape: " + this.name + " in perm: " + this.shapeManager.gameManager.chosenRotation);
             this.rotateByPermutation(this.shapeManager.gameManager.chosenRotation);
             this.shapeManager.gameManager.chosenRotation = -1;
             rotateMe = false;
         } */
    }

    private bool rotateMe = false;

    //private bool amITheChosenPermutation = false;
    private List<BaseShape> rotationsShown = new List<BaseShape>();

    private void changeLinesLyaer(List<GameObject> lines)
    {
        foreach (GameObject line in lines)
        {
            line.GetComponent<LineRenderer>().sortingOrder += 2;
        }
    }
    public void showPossibleRotations()
    {
        rotateMe = true;
        this.shapeManager.showRotationsForShape.SetActive(true);
        var reference_point = this.shapeManager.showRotationsForShape.transform.GetChild(0).transform.localPosition;
        print("ref point: " + reference_point[0] + "," + reference_point[1]);
        print(this.shapeManager.numOfPossiblePermutations);
        for (int i = 1; i <= this.shapeManager.numOfPossiblePermutations; i++)
        {
            var rot = getRotationByPermutation(i);
            var pos = shapeManager.showRotationsForShape.transform.localPosition;
            var additon = getPosAdditionByPerm(i);
            pos.x = reference_point[0] + additon[0];
            pos.y = reference_point[1] + additon[1];
            BaseShape rotated = Instantiate(this, this.shapeManager.showRotationsForShape.transform, false);
            rotated.name = "permutation_" + (i).ToString();
            rotated.transform.localPosition = pos;
            rotated.transform.Rotate(new Vector3(rot[0], rot[1], rot[2]), Space.Self);
            rotated.permutation = i;
            changeLinesLyaer(rotated.assemblingLines);
            rotationsShown.Add(rotated);
        }
    }

    public void Place()
    {
        //Placing the piece in its final positon on board, and making it unplayable.
        this.isFinalPos = true;
        this.nearestCell.isOccupied = true;
        this.transform.SetParent(this.nearestCell.transform.parent);
        this.transform.position = this.nearestCell.transform.position;
        this.transform.localPosition = this.nearestCell.upperRightEdge();
        print("placed!");
    }

    protected bool checkPositionInBoard()
    {
        this.transform.SetParent(this.shapeManager.boardtrans);
        bool res = this.shapeManager.isPositionedInBoard(transform.localPosition);
        this.transform.SetParent(this.shapeManager.canvasTrans);
        return res;
    }

    public void moveForAi(int permutation, int new_position_x, int new_position_y)
    {
        this.transform.SetParent(this.shapeManager.boardtrans);
        transform.localPosition = new Vector3(new_position_x, new_position_y);
        this.nearestCell = this.getNearesetCell();
        this.rotateByPermutation(permutation);
        this.Place();

    }


    protected CellClass getNearesetCell()
    {
        this.transform.SetParent(this.shapeManager.boardtrans);
        var cell = this.shapeManager.getNearestCell(transform.localPosition);
        this.transform.SetParent(this.shapeManager.canvasTrans);
        return cell;
    }

    protected void getInitialCell()
    {
        if (this.shapeManager.isFirstTurn)
        {
            this.transform.SetParent(this.shapeManager.boardtrans);
            var startingPos = new Vector3(15, 15);
            this.nearestCell = this.shapeManager.getNearestCell(startingPos);
            this.transform.SetParent(this.shapeManager.canvasTrans);
        }
    }

    #endregion

    #region Events

    public bool canBeMoved() => playerNum == this.shapeManager.currentPlayer;

    public bool shapeOfHuman() => this.shapeManager.isHeHuman();//one can assume that this is always called after can be moved.

    public void setMyRotation(int rot)
    {
        if (rotateMe && this.shapeManager.gameManager.chosenRotation != -1)
        {
            print("rotating the shape: " + this.name + " in perm: " + rot);
            this.rotateByPermutation(rot);
            this.shapeManager.gameManager.chosenRotation = -1;
            rotateMe = false;
        }
    }

    public async Task OnMouseDown()
    {
        //This function is called once the player has started to drag the object.
        if (!isFinalPos && canBeMoved() && shapeOfHuman())
        {
            if (this.shapeManager.gameManager.rotationMode)
            {
                if (this.shapeManager.gameManager.choosingRotationMode)
                {
                    //this.amITheChosenPermutation = true;
                    this.shapeManager.gameManager.choosingRotationMode = false;
                    this.shapeManager.showRotationsForShape.SetActive(false);
                    this.shapeManager.gameManager.rotationMode = false;
                    this.shapeManager.gameManager.chosenRotation = permutation;
                    this.shapeManager.gameManager.gameCanvas.chooseRotation(permutation);
                    for (int i = 1; i <= this.shapeManager.numOfPossiblePermutations; i++)
                    {
                        //var childMatch = this.shapeManager.showRotationsForShape.transform.GetChild(i);
                        //childMatch.gameObject.SetActive(true);
                        Destroy(this.shapeManager.showRotationsForShape.transform.GetChild(i).gameObject);

                    }
                }
                else
                {
                    this.showPossibleRotations();
                    this.shapeManager.gameManager.choosingRotationMode = true;
                }
            }
            else
            {
                if (!this.shapeManager.isFirstTurn)
                {
                    //The localposition is the objects position inside the canvas.
                    mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

                    //Store offset = gameobject world pos - mouse world pos
                    mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
                }
                else
                {
                    //In the first turn in the game, the player (4) chooses which shape to position in the middle of the board
                    this.getInitialCell();
                    this.Place();
                    await this.shapeManager.sendStartGame(piece_num, permutation);
                }
            }
        }
    }


    public async Task OnMouseUp()
    {
        //This function is called once the player has finished dragging the object, and put it down.
        if ((!this.shapeManager.gameManager.rotationMode) && canBeMoved() && !isFinalPos && !this.shapeManager.isFirstTurn && shapeOfHuman())
        {
            if (await Move() == -1)
            {
                //In case of an illegal move - reset the move.
                this.shapeManager.shoutAtPlayer();
                transform.localPosition = startingPosition;
                this.transform.SetParent(this.startTransformation);
                return;
            }
            else
            {
                Place();

                await shapeManager.switchTurn();
            }
        }
    }

    private Vector3 GetMouseAsWorldPoint()
    {
        //Pixel coordinates of mouse (x,y)
        Vector3 mousePoint = Input.mousePosition;

        //z coordinate of game object on screen
        mousePoint.z = mZCoord;

        //Convert it to world points
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }


    public void OnMouseDrag()
    {
        //This function is called while the player drags the piece.
        if ((!this.shapeManager.gameManager.rotationMode) && canBeMoved() && !isFinalPos && !this.shapeManager.isFirstTurn && shapeOfHuman())
        {
            transform.position = GetMouseAsWorldPoint() + mOffset;

            if (this.checkPositionInBoard())
            {
                this.nearestCell = this.getNearesetCell();

                //this.nearestCell.mOutlineImage.enabled = true;
            }
        }
    }
    #endregion

}





/*
 * The following code isnt needed. Its prupose is to check for valid moves
 * 
 * private void CreateCellPath(int xDirection, int yDirection, int movement)
    {
        
         * this function would have to be completly changed...
        // Target position
        int currentX = mCurrentCell.mBoardPosition.x;
        int currentY = mCurrentCell.mBoardPosition.y;

        // Check each cell
        for (int i = 1; i <= movement; i++)
        {
            currentX += xDirection;
            currentY += yDirection;

            // Get the state of the target cell
            CellState cellState = CellState.None;
            cellState = mCurrentCell.mBoard.ValidateCell(currentX, currentY, this);

            // If enemy, add to list, break
            if (cellState == CellState.Enemy)
            {
                hintCells.Add(mCurrentCell.mBoard.mAllCells[currentX, currentY]);
                break;
            }

            // If the cell is not free, break
            if (cellState != CellState.Free)
                break;

            // Add to list
            hintCells.Add(mCurrentCell.mBoard.mAllCells[currentX, currentY]);
        }

    }

    protected virtual void CheckPathing()
{
    // Horizontal
    CreateCellPath(1, 0, mMovement.x);
    CreateCellPath(-1, 0, mMovement.x);

    // Vertical 
    CreateCellPath(0, 1, mMovement.y);
    CreateCellPath(0, -1, mMovement.y);

    // Upper diagonal
    CreateCellPath(1, 1, mMovement.z);
    CreateCellPath(-1, 1, mMovement.z);

    // Lower diagonal
    CreateCellPath(-1, -1, mMovement.z);
    CreateCellPath(1, -1, mMovement.z);
}

this is for hint cells
     protected void ShowCells()
    {
        foreach (Cell cell in hintCells)
            cell.mOutlineImage.enabled = true;
    }

    protected void ClearCells()
    {
          foreach (Cell cell in hintCells)
            cell.mOutlineImage.enabled = false;

        hintCells.Clear();
    }
*/


