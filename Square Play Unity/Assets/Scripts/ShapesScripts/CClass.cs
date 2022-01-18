using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CClass : BaseShape
{

    public override void Setup(Color newTeamColor, Color32 newSpriteColor, int teamN,ShapesManager newPieceManager, Vector3 startingPos)
    {
        base.Setup(newTeamColor, newSpriteColor, teamN,newPieceManager,startingPos);


        base.piece_num = newPieceManager.getPieceNumByType(this.name);
    }
    public void tempSet(ShapesManager newPieceManager){
         base.playerNum=1;
this.shapeManager=newPieceManager;
        base.piece_num = newPieceManager.getPieceNumByType(this.name);
    }
}
