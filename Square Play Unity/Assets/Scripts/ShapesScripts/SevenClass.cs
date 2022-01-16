using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SevenClass : BaseShape
{
    public List<GameObject> lines;

    public override void Setup(Color newTeamColor, Color32 newSpriteColor, int teamN,ShapesManager newPieceManager, Vector3 startingPos)
    {
        base.Setup(newTeamColor, newSpriteColor, teamN,newPieceManager,startingPos);

        base.assemblingLines=lines;

        //GetComponent<Image>().sprite = Resources.Load<Sprite>("T_Bishop");
    }
}
