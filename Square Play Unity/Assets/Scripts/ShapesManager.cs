using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShapesManager : MonoBehaviour
{
    [HideInInspector]
    public GameObject mPiecePrefab;

    public AClass a_test;

    public CClass c_test;

    [HideInInspector]
    public int currentPlayer=2;

    public GameObject turnStatisticsPlayerName;

    private List<BaseShape> player1Pieces = new List<BaseShape>();
    private List<BaseShape> player2Pieces = new List<BaseShape>();
    private List<BaseShape> player3Pieces = new List<BaseShape>();
    private List<BaseShape> player4Pieces = new List<BaseShape>();

    public GameObject bank1;
    public GameObject bank2;
    public GameObject bank3;
    public GameObject bank4;

    private CompetitiveGameManager gameManager;
     [HideInInspector]
    public Transform boardtrans;
     [HideInInspector]
    public Transform canvasTrans;
    private string[] pieceOrder = new string[16]
    {
        "Six","H","shape3","Gimel","F","E","P","A","notnot","ground","Five","chair","G","C","Seven","Lambda"
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
    {gM.randomizePlayerNames();
        a_test.tempSet(this);
        c_test.tempSet(this);
        a_test.isFirstTurn=true;
        c_test.isFirstTurn=true;
        a_test.isPlayable=true;
        c_test.isPlayable=true;
        gameManager = gM;
        boardtrans=gM.board.transform;
        canvasTrans=gM.canvas.transform;
        this.turnStatisticsPlayerName.GetComponent<TextMeshProUGUI>().text=this.gameManager.playernames[this.currentPlayer+1]+"'s Turn:";
    }


    public void Setup(BoardClass board, CompetitiveGameManager gM)
    {
        gameManager = gM;
        boardtrans=gM.board.transform;
        canvasTrans=gM.canvas.transform;
        player1Pieces = CreatePieces(Color.white, new Color32(80, 124, 159, 255),1,bank1);
        
        player2Pieces = CreatePieces(Color.black, new Color32(210, 95, 64, 255),2,bank2);

        player3Pieces = CreatePieces(Color.red, new Color32(210, 95, 64, 255),3,bank3); //other team color.

        player4Pieces = CreatePieces(Color.green, new Color32(210, 95, 64, 255),4,bank4); //another different team color.
    }

    private List<BaseShape> CreatePieces(Color teamColor, Color32 spriteColor,int playerNum,GameObject bank)
    {
        List<BaseShape> newPieces = new List<BaseShape>();

        for (int i = 0; i < pieceOrder.Length; i++)
        {
            string key = pieceOrder[i];
            Type pieceType = pieceLibrary[key];
            BaseShape newPiece = createPiece(pieceType);
            newPiece.Setup(teamColor, spriteColor, playerNum, this,newPiece.transform.position);
            newPieces.Add(newPiece);
        }
        this.placePieces(newPieces,bank);
        return newPieces;
    }

    private BaseShape createPiece(Type pieceType)
    {
        GameObject newPieceObject = Instantiate(mPiecePrefab);
        newPieceObject.transform.localScale = new Vector3(this.gameManager.scaleFactor, this.gameManager.scaleFactor, 0);
        newPieceObject.transform.localRotation = Quaternion.identity;
        BaseShape newPiece = (BaseShape)newPieceObject.AddComponent(pieceType);
        return newPiece;
    }

    private void placePieces(List<BaseShape> pieces, GameObject bank)
    {
        int x_add=0,y_add=0;
        pieces.ForEach(delegate (BaseShape piece){
            float new_x = bank.transform.localPosition.x + x_add*(this.gameManager.scaleFactor+10) + 4;
            float new_y = bank.transform.localPosition.y + y_add*(this.gameManager.scaleFactor+10) + 4;
           piece.transform.localPosition=new Vector3(new_x,new_y);
           x_add=(x_add+1)%2;
           if(x_add==0){
               y_add++;
           }
        });
    }

    private void setInteractive(List<BaseShape> allPieces, bool value)
    {
        allPieces.ForEach(delegate (BaseShape piece){
             piece.isPlayable = value;
        });
    }


    public void switchTurn()
    {
        this.currentPlayer = (currentPlayer + 1) % 4;
        this.turnStatisticsPlayerName.GetComponent<TextMeshProUGUI>().text=this.gameManager.playernames[this.currentPlayer]+"'s Turn:";

        setInteractive(player1Pieces, currentPlayer == 0);

        setInteractive(player2Pieces, currentPlayer == 1);

        setInteractive(player3Pieces, currentPlayer == 2);

        setInteractive(player4Pieces, currentPlayer == 3);
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
    public int sendStartGame(int pieceNum,int permutation) {
        a_test.isFirstTurn=false;//tbd:delete
        c_test.isFirstTurn=false;//tbd:delete
        this.endFirstMove();
        return this.gameManager.msgGameStartToServer(pieceNum,permutation);
    }
           

    public int sendMove(int pieceNum,int permutation, int new_position_x, int new_position_y)=>this.gameManager.msgMoveToServer( this.currentPlayer + 1,  pieceNum, permutation,  new_position_x,  new_position_y);
    

    public bool isPositionedInBoard(Vector3 pos)=>this.gameManager.board.isInBoard(pos);

    public CellClass getNearestCell(Vector3 pos)=> this.gameManager.board.getCellByCoordinates(pos);

    public void startGame(){
        this.switchTurn();
        this.player4Pieces.ForEach(delegate (BaseShape shape){
            shape.isFirstTurn=true;
        });
    }

    private void endFirstMove(){
        this.switchTurn();
        this.player4Pieces.ForEach(delegate (BaseShape shape){
            shape.isFirstTurn=false;
        });
    }
    public void shoutAtPlayer(){
        this.gameManager.showNotification("Youv'e made an illegal move!");
    }
}