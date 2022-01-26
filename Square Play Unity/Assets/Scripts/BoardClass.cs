using System.Collections.Generic;
using UnityEngine;


public class BoardClass : MonoBehaviour
{

    public CellClass cellPrefab;

    public List<CellClass> cells;

    //For each cell, well save its upper right edge coordinates together with the cells index in list.
    private SortedDictionary<Vector3, int> coordinatesToCellIndex;

    private CoordinatesComparer my_comparer;

    private int scaleFactor = 33;

    //public Material cell_material;//tbd


    private class CoordinatesComparer : IComparer<Vector3>
    {
        //Points are to be sorted by x, than y.
        public int Compare(Vector3 k1, Vector3 k2)
        {
            //One can assume that cells x and y are integers represented in float.
            float x_comp = k1.x - k2.x;
            float y_comp = k1.y - k2.y;
            if (x_comp == 0)
            {
                return (int)y_comp;
            }
            else
            {
                return (int)x_comp;
            }
        }
    }

    private CompetitiveGameManager manager;
    public void generate(int cols, int rows, CompetitiveGameManager m)
    {

        manager = m;
        cells = new List<CellClass>();

        my_comparer = new CoordinatesComparer();

        coordinatesToCellIndex = new SortedDictionary<Vector3, int>(my_comparer);

        for (int x = 0; x < cols; x++)
        {

            for (int y = 0; y < rows; y++)
            {
                var newCell = createCell(x, y);
                cells.Add(newCell);
                coordinatesToCellIndex.Add(new Vector3(newCell.x, newCell.y), cells.Count - 1);
            }

        }


        float newX = -rows / 2 + 1 / 2;
        float newY = -cols / 2 + 1 / 2;
        transform.localPosition = new Vector3(newX * scaleFactor, newY * scaleFactor, 0);
        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
    }

    CellClass createCell(int posx, int posy)
    {
        CellClass newCell = Instantiate(cellPrefab) as CellClass;
        //newCell.GetComponent<MeshRenderer>().material = cell_material;
        newCell.setupPos(posx, posy, manager);

        newCell.transform.SetParent(transform, false);
        newCell.transform.localPosition = new Vector3(posx, posy, 0);

        newCell.name = "Board CellClass [" + posx + "," + posy + "]";

        return newCell;
    }

    public bool isInBoard(Vector3 position)
    {
        //Checks wheter a given point is inside the board.
        float x = position.x;
        float y = position.y;
        return coordinatesToCell(x, y) != -1;
    }

    private int coordinatesToCell(float x, float y)
    {
        //This function will map coordinates on the unity grid to the cell in which those coordinates can be found.
        Vector3 toLowerLeft = new Vector3(Mathf.Floor(x), Mathf.Floor(y));
        int ret = -1;
        this.coordinatesToCellIndex.TryGetValue(toLowerLeft, out ret);
        return ret;
    }

    public CellClass getCellByCoordinates(Vector3 position)
    {
        //Assume isInBoard is called before that function.
        float x = position.x;
        float y = position.y;
        int idx = coordinatesToCell(x, y);
        return this.cells[idx];
    }
}

