using UnityEngine;

public class LambdaClass : BaseShape
{
    public override void Setup(Color newTeamColor, ShapesManager newPieceManager)
    {
        base.Setup(newTeamColor, newPieceManager);

        base.piece_num = newPieceManager.getPieceNumByType(this.name);

    }

}
