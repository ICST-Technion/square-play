using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EClass : BaseShape
{
    public override void Setup(Color newTeamColor, Color32 newSpriteColor, int teamN,ShapesManager newPieceManager, Vector3 startingPos)
    {
        base.Setup(newTeamColor, newSpriteColor, teamN,newPieceManager,startingPos);

        Debug.Log(this.name);

        base.piece_num = newPieceManager.getPieceNumByType(this.name);
    }
}
