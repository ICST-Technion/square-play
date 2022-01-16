using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public abstract class BaseShape : MonoBehaviour
{
    [HideInInspector]
    public Color color = Color.clear;
    [HideInInspector]
    public int playerNum = -1;
    private Vector3 mOffset;
    private float mZCoord;

    protected Vector3 startingPosition;
    protected RectTransform rectTransform = null;

    public ShapesManager shapeManager;

    //This is very important! It is used in order to correctly position the shape on the new cells.
    public List<MonoBehaviour> assemblingLines;

    protected CellClass nearestCell=null; //tbd: need to not only highlight the nearest cell, but also the nearest cell to each line

    protected int piece_num;

    private bool isFinalPos=false;

    //protected List<List<CellClass>> hintCells = new List<List<CellClass>>();//This is for the helper.

    public virtual void Setup(Color newTeamColor, Color32 newSpriteColor, int playerNum, ShapesManager newshapeManager, Vector3 startingPos)
    {
        shapeManager = newshapeManager;
        this.playerNum = playerNum;
        color = newTeamColor;
        GetComponent<Image>().color = newSpriteColor;
        rectTransform = GetComponent<RectTransform>();
        startingPosition = startingPos;
    }

    #region Movement

    public bool Move()
    {
        //tbd: if its a computer player - move according to the AI algo... 
        this.nearestCell = this.getNearesetCell();
        float new_position_x = this.nearestCell.x;
        float new_position_y = this.nearestCell.y;
        int permutation = 1; //tbd: add permutations later.

        //Check that move is valid with logic function
        return this.shapeManager.sendMove(playerNum, piece_num, permutation, new_position_x, new_position_y);
    }


    public void Place() //tbd: make it virtual so each shape will override it and place all its lines according to its shape!
    {
        this.isFinalPos=true;
        this.nearestCell.isOccupied=true;
        this.transform.SetParent(this.nearestCell.transform.parent);
        this.transform.position=this.nearestCell.transform.position;
        this.transform.localPosition=this.nearestCell.upperRightEdge();
    }


    #endregion

    #region Events

    void OnMouseDown()
    {if(!isFinalPos){
        //The localposition is the objects position inside the canvas.
        this.startingPosition=this.transform.localPosition;
        //This function is called once the player has started to drag the object.
        mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;

        //Store offset = gameobject world pos - mouse world pos
        mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
    }
    }

    void OnMouseUp()
    {
        if(!isFinalPos){
        //This function is called once the player has finished dragging the object, and put it down.
        if (!Move()) //In case of an illegal move - reset the move.
        {
            transform.localPosition = startingPosition;
            return;
        }
        else //tbd:either play next player, or give the current one more moves.
        {
            Place();

            //shapeManager.SwitchTurn(playerNum);
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

    void OnMouseDrag()
    {
        //This function is called while the player drags the piece.
        if(!isFinalPos){
        transform.position = GetMouseAsWorldPoint() + mOffset;
        if (this.checkPositionInBoard()) //maybe need to check if all the lines are within the board...
        {
            this.nearestCell = this.getNearesetCell();
            print(this.nearestCell.ToString());
            //this.nearestCell.mOutlineImage.enabled = true;
        }
        }
    }

    private bool checkPositionInBoard(){
        
        this.transform.SetParent(this.shapeManager.boardtrans);
        bool res =this.shapeManager.isPositionedInBoard(transform.localPosition);
        this.transform.SetParent(this.shapeManager.canvasTrans);
        return res;
    }

    private CellClass getNearesetCell(){
        this.transform.SetParent(this.shapeManager.boardtrans);
        var cell = this.shapeManager.getNearestCell(transform.localPosition);
        this.transform.SetParent(this.shapeManager.canvasTrans);
        return cell;
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

