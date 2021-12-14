using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CompetitiveGameManager : MonoBehaviour
{
	public BoardClass board;

	private int cols = 39;
	private int rows= 27;

	// Use this for initialization
	void Start()
	{
		board.generate(cols,rows);
	}

	// Update is called once per frame
	void Update()
	{
		//detectInput();
	}

	void detectInput()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Debug.Log(ray.ToString());
			RaycastHit hit;
			//bool res = Physics.Raycast(ray, out hit, 100f);
			if (Physics.Raycast(ray, out hit))
			{
				Debug.Log("Hit! "+hit.collider.ToString());
				CellClass hitCell = hit.collider.GetComponent<CellClass>();
				Debug.Log("Hit: " + hitCell.ToString());
				activateCell(hitCell.Row, hitCell.Col);

                //updateDebugView();

            }
            else
            {
				Debug.Log("no hit!");
			}
		}
	}

	void activateCell(int row, int col)
	{
		Debug.Log("hey");
		//board.cells[row * boardWidth + col].isOccupied=true;
	}

	/*string debugGameState()
	{
		string map = "";

		for (int x = 0; x < 8; x++)
		{

			map += x + ": ";

			for (int y = 0; y < 8; y++)
			{
				map += "[" + boardGrid[x, y] + "]";
			}
			map += "\n";
		}

		return map;
	}

	void updateDebugView()
	{
		DebugText.text = debugGameState();

	}*/
}
