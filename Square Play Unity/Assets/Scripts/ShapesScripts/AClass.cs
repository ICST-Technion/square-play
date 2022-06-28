using UnityEngine;

public class AClass : BaseShape
{
    //public GameObject aclassPrefab;
    public override void Setup(Color newTeamColor, ShapesManager newPieceManager)
    {

        base.Setup(newTeamColor, newPieceManager);

        //base.classPrefab = aclassPrefab;

        base.piece_num = newPieceManager.getPieceNumByType(this.name);

    }
    public void tempSet(ShapesManager newPieceManager)
    {
        this.shapeManager = newPieceManager;
        base.piece_num = newPieceManager.getPieceNumByType(this.name);
    }
    
}
