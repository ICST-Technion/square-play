using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellClass : MonoBehaviour
{
    public int x;
    public int y;
    public float upper_right_x;
    public float upper_right_y;

    public float lower_left_x;
    public float lower_left_y;
    public bool isOccupied;

    private CompetitiveGameManager manager;

    public Image mOutlineImage;

    public void setupPos(int posx, int posy, CompetitiveGameManager manager)
    {
        this.manager = manager;
        this.x = posx;
        this.y = posy;
        this.upper_right_x = (float)(posx + 0.5);
        this.upper_right_y = (float)(posy + 0.5);
        this.lower_left_x = (float)(posx - 0.5);
        this.lower_left_y = (float)(posy - 0.5);

    }
    public void LightItUp()
    {
    }

    public void TurnLightOff()
    {

    }

    public Vector3 upperRightEdge()
    {
        var vec = this.transform.localPosition;
        vec.x += (float)0.5;
        vec.y += (float)0.5;
        return vec;
    }


}
