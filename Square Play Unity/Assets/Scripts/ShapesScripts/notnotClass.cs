using UnityEngine;

public class notnotClass : BaseShape
{
    public override void Setup(Color newTeamColor, ShapesManager newPieceManager)
    {
        base.Setup(newTeamColor, newPieceManager);

        base.piece_num = newPieceManager.getPieceNumByType(this.name);

    }
}
