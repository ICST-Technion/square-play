using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;


public class BoardClass : MonoBehaviour
{
	public List<CellClass> cells;
	[HideInInspector]
	public float cellSize=1;
	public List<CellClass> occupiedCells=new List<CellClass>();
	private float sacleFactor = 33;

	public void generate(int colsAmount,int rowsAmount)
	{
		GameObject refCell = (GameObject)Instantiate(Resources.Load("simplified"));
		int idx = 0;
		for (int col = 0; col < colsAmount; col++)
		{
			for (int row = 0;  row< rowsAmount; row++)
			{
				CellClass cell = Instantiate(refCell, transform).GetComponent<CellClass>();
				/*
				Yes, altough positionCell requiers the following argument order: (size,col,row,idx),
				we pass row first. This is due to the board placement were doing below, which was 
				strictly calculated for a matrix from size of NxN and not of size NxM like this.
				*/
				cell.positionCell(cellSize, row, col,idx);
				cell.name = "Cell [" + row + "," + col + "]";
				cells.Add(cell);
				idx++;
			}
		}

		Destroy(refCell);

		float boardW = colsAmount * cellSize;
		float boardH = rowsAmount * cellSize;
		float newX = -boardW / 2 + cellSize / 2;
        float newY = boardH / 2 - cellSize / 2;
		transform.localPosition= new Vector3(newX* sacleFactor, newY* sacleFactor, 0);
		transform.localScale = new Vector3( sacleFactor,  sacleFactor, 1); ;
	}

	public void populate()
	{
		//I dont think that this function is needed.
		foreach (CellClass cellInstance in cells)
		{
			//cellInstance.isOccupied = true;

			//GameObject newPawn = Instantiate(PawnPrefab) as GameObject;

			//newPawn.transform.localPosition = new Vector3(cellInstance.transform.position.x, cellInstance.transform.position.y, -2);

		}
	}
}

