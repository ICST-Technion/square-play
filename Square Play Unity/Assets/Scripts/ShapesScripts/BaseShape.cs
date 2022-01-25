using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
//using UnityEngine.;

public abstract class BaseShape : MonoBehaviour, IPointerClickHandler,
 IPointerExitHandler, IPointerEnterHandler
{


    private Vector3 mOffset;
    private float mZCoord;

    public Vector3 startingPosition;

    public ShapesManager shapeManager;

    //This is very important! It is used in order to correctly position the shape on the new cells.
    public List<GameObject> assemblingLines = new List<GameObject>();

    protected CellClass nearestCell = null; //tbd: need to not only highlight the nearest cell, but also the nearest cell to each line

    public int piece_num;

    protected int permutation = 1;

    public bool isFinalPos = false;

    public int playerNum;

    //public GameObject classPrefab;


    //protected List<List<CellClass>> hintCells = new List<List<CellClass>>();//This is for the helper.

    public virtual void Setup(Color newTeamColor, ShapesManager newshapeManager)
    {
        this.shapeManager = newshapeManager;
        this.transform.SetParent(this.shapeManager.canvasTrans);
        this.transform.localScale = new Vector3(this.shapeManager.gameScale, this.shapeManager.gameScale, 0);
        this.transform.localRotation = Quaternion.identity;
        /*foreach (GameObject line in this.assemblingLines)
        {
            line.GetComponent<LineRenderer>().startColor = newTeamColor;
            line.GetComponent<LineRenderer>().endColor = Color.white;
        }*/
        this.name.Split('_');
        int.TryParse(this.name.Split('_')[1], out this.playerNum);
        this.playerNum -= 1;
        //this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Name = " + hit.collider.name);
                Debug.Log("Tag = " + hit.collider.tag);
                Debug.Log("Hit Point = " + hit.point);
                Debug.Log("Object position = " + hit.collider.gameObject.transform.position);
                Debug.Log("--------------");
            }
        }
    }

    void Awake()
    {
        print("hello! i am " + name);
    }

    public void setupStartPos(float x, float y)
    {
        var pos = new Vector3(x, y);
        this.transform.localPosition = pos;
        this.startingPosition = pos;
        //this.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
    }

    #region Movement

    public int Move()
    {
        if (!this.checkPositionInBoard())
        {
            return -1;
        }
        this.nearestCell = this.getNearesetCell();
        int new_position_x = this.nearestCell.x;
        int new_position_y = this.nearestCell.y;

        //Check that move is valid with logic function
        print("sending move for shape : " + this.name);
        int[] response = this.shapeManager.sendMove(piece_num, permutation, new_position_x, new_position_y);
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
        this.transform.localRotation.Set(rot[0], rot[1], rot[2], rot[3]);
    }

    private int[] getRotationByPermutation(int permutation)
    {
        //By default, there is a maximum of 8 permutations.
        switch (permutation)
        {
            case 1:
                return new int[] { 0, 0, 270, 0 };
            case 2:
                return new int[] { 0, 0, -270, 0 };
            case 3:
                return new int[] { 0, 0, 180, 0 };
            case 4:
                return new int[] { 0, 180, 0, 0 };
            case 5:
                return new int[] { 0, 180, 270, 0 };
            case 6:
                return new int[] { 0, 180, -270, 0 };
            case 7:
                return new int[] { 180, 0, 0, 0 };
            case 8:
                return new int[] { 180, 0, 90, 0 };
            case 9:
                return new int[] { 180, 0, -90, 0 };
            default:
                return new int[] { 0, 0, 0, 0 }; ;
        }
    }

    public void showPossibleRotations()
    {
        this.shapeManager.showRotationsForShape.SetActive(true);
        var rect = this.shapeManager.showRotationsForShape.gameObject.GetComponent<RectTransform>().rect;
        var showPos = this.shapeManager.showRotationsForShape.transform.localPosition;
        float width = rect.width;
        float height = rect.height;
        print("w: " + width.ToString() + " h: " + height.ToString());
        float offesetFromEdges = 20;
        var position = this.shapeManager.showRotationsForShape.transform;
        float x_add = 0, y_add = 0;
        float absoluteX = showPos.x - width / 2 + offesetFromEdges;
        float absoluteY = showPos.y - height / 2 + offesetFromEdges;
        for (int i = 0; i < 10; i++) // replace 10 with this.shapeManager.numOfPossiblePermutations
        {
            x_add = i % (this.shapeManager.numOfPossiblePermutations / 2);
            y_add = x_add == 0 ? y_add + 1 : y_add;
            var pos = new Vector3(absoluteX + x_add * this.shapeManager.spacingFactor, absoluteY - y_add * this.shapeManager.spacingFactor);
            print("pos before change: ");
            print(pos);
            pos = new Vector3(i * 50, i * 50);
            var qut = Quaternion.identity;
            var rot = getRotationByPermutation(i);
            qut.Set(rot[0], rot[1], rot[2], rot[3]);
            /*print("w: " + width.ToString() + " h: " + height.ToString());
            print("ax: " + absoluteX.ToString() + " ay:  " + absoluteY.ToString());
            print("pos: " + pos.ToString());*/
            BaseShape rotated = Instantiate(this, this.shapeManager.showRotationsForShape.transform, false);
            //rotated.transform.SetPositionAndRotation(pos, qut);
            rotated.transform.localPosition = pos;
            print("rot: ");
            print(rot);
            rotated.transform.localEulerAngles = new Vector3(rot[0], rot[1], rot[2]);
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
    }

    private bool checkPositionInBoard()
    {
        this.transform.SetParent(this.shapeManager.boardtrans);
        bool res = this.shapeManager.isPositionedInBoard(transform.localPosition);
        this.transform.SetParent(this.shapeManager.canvasTrans);
        return res;
    }

    public void moveForAi(int permutation, int new_position_x, int new_position_y)
    {
        this.transform.SetParent(this.shapeManager.canvasTrans);
        transform.localPosition = new Vector3(new_position_x, new_position_y);
        this.nearestCell = this.getNearesetCell();
        this.rotateByPermutation(permutation);
        Place();
        this.shapeManager.switchTurn();
    }


    private CellClass getNearesetCell()
    {
        this.transform.SetParent(this.shapeManager.boardtrans);
        print("for position: " + this.transform.localPosition.ToString() + " got cell: ");
        var cell = this.shapeManager.getNearestCell(transform.localPosition);
        this.transform.SetParent(this.shapeManager.canvasTrans);
        return cell;
    }

    private void getInitialCell()
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

    /*public void OnMouseDown()
    {
        print("touched: " + this.name);
        //This function is called once the player has started to drag the object.
        if (!isFinalPos && canBeMoved() && shapeOfHuman())
        {
            if (!this.shapeManager.isFirstTurn)
            {
                //The localposition is the objects position inside the canvas.
                this.startingPosition = this.transform.localPosition;

                mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

                //Store offset = gameobject world pos - mouse world pos
                mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
            }
            else
            {
                //In the first turn in the game, the player (4) chooses which shape to position in the middle of the board
                this.getInitialCell();
                this.Place();
                this.shapeManager.sendStartGame(piece_num, permutation);
            }
        }
    }


    public void OnMouseUp()
    {
        //This function is called once the player has finished dragging the object, and put it down.
        if (canBeMoved() && !isFinalPos && !this.shapeManager.isFirstTurn && shapeOfHuman())
        {
            if (Move() == -1)
            {
                //In case of an illegal move - reset the move.
                this.shapeManager.shoutAtPlayer();
                transform.localPosition = startingPosition;
                return;
            }
            else
            {
                Place();

                shapeManager.switchTurn();
            }
        }
    }*/

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
        print("hey from " + name);
        //This function is called while the player drags the piece.
        if (canBeMoved() && !isFinalPos && !this.shapeManager.isFirstTurn && shapeOfHuman())
        {
            print("draggind: " + this.name);
            transform.position = GetMouseAsWorldPoint() + mOffset;
            if (this.checkPositionInBoard())
            {
                this.nearestCell = this.getNearesetCell();

                //this.nearestCell.mOutlineImage.enabled = true;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print("click!");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        print("enter");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        print("bye:(");
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


