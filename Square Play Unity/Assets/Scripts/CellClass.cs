using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellClass : MonoBehaviour
{
    private int row;
    private int col;
    public bool isOccupied;
    public int cellIndex;
    public Vector3[] cellEdges=new Vector3[4];
    public int Row { get => row; set => row = value; }
    public int Col { get => col; set => col = value; }

    public void positionCell(float cellSize,int col,int row,int index)
    { 
        this.row = row;
        this.col = col;
        transform.localPosition = new Vector2(this.row*cellSize, this.col*-cellSize);
        cellIndex = index;
        cellEdges[0] = transform.localPosition;//Left Upper
        cellEdges[1] = new Vector3(transform.localPosition.x+cellSize, transform.localPosition.y);//Right Upper
        cellEdges[2] = new Vector3(transform.localPosition.x, transform.localPosition.y - cellSize);//Left Lower
        cellEdges[3] = new Vector3(transform.localPosition.x + cellSize, transform.localPosition.y-cellSize);//Right Lower
    }


    public bool isThisCellOccupied(Vector3 objPos)
    {
        //X's of cells are always greater than 0, Y's are always negative.
        var leftUp = cellEdges[0];
        var rightUp = cellEdges[1];
        var leftLow = cellEdges[2];
        var rightLow = cellEdges[3];
        float cellSize = GetComponentInParent<BoardClass>().cellSize;
        return isOccupied && leftUp.x <= objPos.x && objPos.x <= rightLow.x &&
           objPos.y <= leftUp.y && rightLow.y <= leftUp.y;
    }
}
