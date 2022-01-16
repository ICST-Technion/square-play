using System;
using System.Collections.Generic;
using UnityEngine;

public class ShapesManager : MonoBehaviour
{
    [HideInInspector]
    public GameObject mPiecePrefab;

    public AClass a_test;

    public CClass c_test;
    private List<BaseShape> player1Pieces = new List<BaseShape>();
    private List<BaseShape> player2Pieces = new List<BaseShape>();
    private List<BaseShape> player3Pieces = new List<BaseShape>();
    private List<BaseShape> player4Pieces = new List<BaseShape>();
    private List<BaseShape> playablePieces = new List<BaseShape>();

    private CompetitiveGameManager gameManager;
    public Transform boardtrans;

    public Transform canvasTrans;
    private string[] pieceOrder = new string[16]
    {
        "A","C","chair","E","F","Five","G","Gimel","ground","H","Lambda","notnot","P","Seven","shape3","Six" 
    };

    private Dictionary<string, Type> pieceLibrary = new Dictionary<string, Type>()
    {
        {"A",  typeof(AClass)},
        {"C",  typeof(CClass)},
        {"chair",  typeof(chairClass)},
        {"E",  typeof(EClass)},
        {"F",  typeof(FClass)},
        {"Five",  typeof(FiveClass)},
        {"G",  typeof(GClass)},
        {"Gimel",  typeof(GimelClass)},
        {"ground",  typeof(groundClass)},
        {"H",  typeof(HClass)},
        {"Lambda",  typeof(LambdaClass)},
        {"notnot",  typeof(notnotClass)},
        {"P",  typeof(PClass)},
        {"Seven",  typeof(SevenClass)},
        {"shape3",  typeof(shape3Class)},
        {"Six",  typeof(SixClass)},
    };


    
    public void temporarySetup(BoardClass board, CompetitiveGameManager gM)
    {
        a_test.shapeManager = this;
        c_test.shapeManager=this;
        gameManager = gM;
        boardtrans=gM.board.transform;
        canvasTrans=gM.canvas.transform;
    }


    public void Setup(BoardClass board, CompetitiveGameManager gM)
    {
        gameManager = gM;
        boardtrans=gM.board.transform;
        canvasTrans=gM.canvas.transform;
        player1Pieces = CreatePieces(Color.white, new Color32(80, 124, 159, 255),1);

        player2Pieces = CreatePieces(Color.black, new Color32(210, 95, 64, 255),2);

        player3Pieces = CreatePieces(Color.red, new Color32(210, 95, 64, 255),3); //other team color.

        player4Pieces = CreatePieces(Color.green, new Color32(210, 95, 64, 255),4); //another different team color.

        SwitchTurn(1);
    }

    private List<BaseShape> CreatePieces(Color teamColor, Color32 spriteColor,int playerNum)
    {
        //Fix according to changes in createPiece
        List<BaseShape> newPieces = new List<BaseShape>();

        for (int i = 0; i < pieceOrder.Length; i++)
        {
            string key = pieceOrder[i];
            Type pieceType = pieceLibrary[key];
            BaseShape newPiece = CreatePiece(pieceType);
            newPieces.Add(newPiece);
            newPiece.Setup(teamColor, spriteColor, playerNum, this,newPiece.transform.position);
        }

        return newPieces;
    }

    private BaseShape CreatePiece(Type pieceType)
    {
        GameObject newPieceObject = Instantiate(mPiecePrefab);
        newPieceObject.transform.localScale = new Vector3(this.gameManager.scaleFactor, this.gameManager.scaleFactor, 0);
        newPieceObject.transform.localRotation = Quaternion.identity;
        //tbd: position in the appropriate shapes bank.
        BaseShape newPiece = (BaseShape)newPieceObject.AddComponent(pieceType);
        newPiece.shapeManager=this;

        return newPiece;
    }

    private void PlacePieces(List<BaseShape> pieces, BoardClass board)
    {
        //Place each piece in the correct bank
    }

    private void SetInteractive(List<BaseShape> allPieces, bool value)
    {
        if (value) {
            foreach (BaseShape piece in allPieces)
                piece.enabled = value;
        }
    }


    public void SwitchTurn(int playerNum)
    {
        playerNum = (playerNum + 1) % 4;
        SetInteractive(player1Pieces, playerNum == 1);

        SetInteractive(player2Pieces, playerNum == 2);

        SetInteractive(player3Pieces, playerNum == 3);

        SetInteractive(player4Pieces, playerNum == 4);

        foreach (BaseShape piece in playablePieces)
        {
            piece.enabled = playerNum==piece.playerNum;
        }
    }

    public int getPieceNumByType(string shapeClassName){
        int idx =0;
        foreach (string item in this.pieceOrder)
        {
            if(item.CompareTo(shapeClassName)==0){
                return idx+1;
            }
            idx++;
        }  
        return idx;
    }

    public bool sendMove(int playerNum, int piece_num,int permutation, float new_position_x, float new_position_y){
        float[] data_out = new float[5]{(float)playerNum, (float)piece_num, (float)permutation, new_position_x,new_position_y};
        if(this.gameManager.msgMoveToServer(data_out)){
            return false;
        }
        return true;
    }

    public bool isPositionedInBoard(Vector3 pos){
        return this.gameManager.board.isInBoard(pos);
    }

    public CellClass getNearestCell(Vector3 pos){
        return this.gameManager.board.getCellByCoordinates(pos);
    }

}