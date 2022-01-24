using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CClass : BaseShape
{

    public override void Setup(Color newTeamColor, ShapesManager newPieceManager)
    {
        base.Setup(newTeamColor, newPieceManager);

        base.piece_num = newPieceManager.getPieceNumByType(this.name);

    }
    public void tempSet(ShapesManager newPieceManager)
    {
        this.shapeManager = newPieceManager;
        base.piece_num = newPieceManager.getPieceNumByType(this.name);
    }
}
