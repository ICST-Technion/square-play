using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
public abstract class BaseShape : EventTrigger
{
    [HideInInspector]
    public Color color = Color.clear;
    [HideInInspector]
    public int teamNum=-1;

    protected Vector3 startingPosition;
    protected List<CellClass> currentCells = new List<CellClass>();
    protected RectTransform rectTransform = null;
    protected ShapeManager shapesManager; //I dont know if it is even needed.
    protected bool targetCells = true;
    protected List<GameObject> assemblingLines;
    protected List<List<CellClass>> hintCells = new List<List<CellClass>>();//This is for the helper.

    public virtual void Setup(Color newTeamColor, Color32 newSpriteColor, int teamNum, ShapeManager newPieceManager,Vector3 startingPos)
    {
        shapesManager = newPieceManager;
        this.teamNum = teamNum;
        color = newTeamColor;
        GetComponent<Image>().color = newSpriteColor;
        rectTransform = GetComponent<RectTransform>();
        startingPosition = startingPos;
    }

    public virtual void Place(List<CellClass> newCells)
    {
        //Fix
        currentCells = newCells;

        foreach(CellClass cell in currentCells){
            //Add a reference to the piece in the cell class (?)
            cell.isOccupied = true;
        }

        //Need to position it on all of the new cells (? - maybe this is automatically done via the drag function?)
        gameObject.SetActive(true);
    }

    public void Reset()
    { //Tbd: just regenerate the competitve game object?
        Kill();

        //isFirstMove = true;

        //Place(mOriginalCell);
    }

    public virtual void Kill()
    {
        // Clear current cell
        //mCurrentCell.mCurrentPiece = null;

        // Remove piece
        //gameObject.SetActive(false);
    }


    public void ComputerMove()
    {
        //tbd: move according to the AI algo... 

        // Move to new cell
        //Place();

        // End turn
        shapesManager.SwitchTurn(teamNum);
    }

    #region Movement
    

    /*this is for hint cells
     * protected void ShowCells()
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
    #endregion

    #region Events
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        //ShowCells(); for hint
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        // Follow pointer
        transform.position += (Vector3)eventData.delta;

        //MAYBE look at it in a different way: search over board.cells for cells which are
        //In the locations where our shape is. If we find one of these which is occupied we return false and none targerCells
        foreach (CellClass cell in shapesManager.occupiedCells)
        {
            if (cell.isThisCellOccupied(transform.position))
            {
                // If the mouse is within an occupied cell , break.
                targetCells = false;
                break;
            }
            
        }


    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        //ClearCells();for hint cells

        if (targetCells==false)
        {
            transform.position = startingPosition;
            return;
        }

        //Place();

        shapesManager.SwitchTurn(teamNum);
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
*/