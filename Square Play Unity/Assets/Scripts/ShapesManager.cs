using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class ShapesManager : MonoBehaviour
{
    public GameObject mShapePrefab;

    //[HideInInspector]
    public int currentPlayer = 2;

    //[HideInInspector]
    public int numOfPossiblePermutations = 8;
    [HideInInspector]
    public float spacingFactor;

    //[HideInInspector]
    public int numOfMovesForCurrentPlayer = 0;

    public int gameScale = 33;

    private bool currentPlayerContinues = false;

    public GameObject turnStatisticsPlayerName;

    public GameObject turnStatisticsNumOfMoves;

    public CompetitiveGameManager gameManager;
    [HideInInspector]
    public Transform boardtrans;
    [HideInInspector]
    public Transform canvasTrans;

    public GameObject showRotationsForShape;

    public bool isFirstTurn;

    private string[] shapeOrder = new string[16]
    {
        "Six","H","shape3","Gimel","F","E","P","A","notnot","ground","Five","chair","G","C","Lambda","Seven"
    };

    /*
    TODO:
     Later on, when ill have time, make the whole prefabs thing smarter:
     Instead of doing initShapesPrefabs one by one, do a loop that will go foreach item in shape order,
     and in the loop, given the name of the current shape in the loop, go to its value in shapeLibrary, and get its type.
     The problem is that it wont allow to use it in the conversion - where i do (HClass)Ass.... 
     I need to find a way to properly convert it..
     maybe... do a dict from shapes names to the lines assembling it. take the lines from the assetload and put them on a new BaseShape object.
    */

    #region Setup

    public void Setup(BoardClass board, CompetitiveGameManager gM)
    {

        this.gameManager = gM;
        this.boardtrans = this.gameManager.board.transform;
        this.canvasTrans = this.gameManager.gameCanvas.transform;
        this.gameScale = this.gameManager.scaleFactor;
        this.spacingFactor = 2.35f * (this.gameManager.scaleFactor + 10);

        setupShapes(Color.blue, gameManager.players[0], true);

        setupShapes(Color.yellow, gameManager.players[1]);

        setupShapes(Color.red, gameManager.players[2], isDown: true);
        Color green = new Color(0.45f, 1f, 0.70f);
        setupShapes(green, gameManager.players[3]);

    }

    private void setupShapes(Color teamColor, PlayerClass bank, bool isUp = false, bool isDown = false)
    {
        int playerNum = bank.playerNum;
        Vector3 textPos = bank.transform.GetChild(0).localPosition;
        var absoluteX = bank.transform.localPosition.x + textPos.x;
        var absoluteY = bank.transform.localPosition.y + textPos.y;
        if (isUp)
        {
            absoluteX += 70;
            absoluteY += 50;
        }
        else if (isDown)
        {
            absoluteX += 70;
            absoluteY += 40;
        }
        else
        {
            absoluteX += -35;
            absoluteY += -40;
        }

        int x_add = 0, y_add = 0;

        bank.playerShapes.ForEach(delegate (BaseShape shape)
        {
            shape.Setup(teamColor, this);
            float new_x = absoluteX + x_add * spacingFactor;
            float new_y = absoluteY - y_add * spacingFactor;
            shape.setupStartPos(new_x, new_y, bank.transform);
            if (isUp || isDown)
            {
                x_add = (x_add + 1) % 8;
            }
            else
            {
                x_add = (x_add + 1) % 3;
            }
            if (x_add == 0)
            {
                y_add++;
            }


        });
    }

    #endregion

    #region Game process

    public void shoutAtPlayer()
    {
        this.gameManager.showNotification("Youv'e made an illegal move!");
    }

    public void currentPlayerClosedSquares(int numberClosed)
    {
        if (numberClosed > 1)
        {
            this.numOfMovesForCurrentPlayer += numberClosed - 1;
        }
        else
        {
            this.numOfMovesForCurrentPlayer = 0;
        }
        print(this.currentPlayer + " closed: " + numberClosed + " squares");
    }



    private async Task setInteractive(List<BaseShape> allShapes, bool value)
    {
        if (!this.isHeHuman() && value)
        {
            int[] aiMove = await this.requestAiMove();
            if (aiMove.Length > 2)
            {
                await MoveShapeForAi(aiMove, allShapes);
                if (this.isFirstTurn)
                {
                    await this.endFirstMove();
                }
            }
        }
    }

    private async Task MoveShapeForAi(int[] aiMove, List<BaseShape> allShapes)
    {
        int shapeNum = aiMove[0];
        int permutation = aiMove[1];
        int newX = aiMove[2];
        int newY = aiMove[3];
        int closed_squares = aiMove[4];
        allShapes.ForEach(delegate (BaseShape shape)
        {
            if (shape.piece_num == shapeNum)
            {
                shape.moveForAi(permutation, newX, newY);
                return;
            }
        });
        if (!this.isFirstTurn)
        {
            this.currentPlayerClosedSquares(closed_squares);
            await this.switchTurn();
        }
    }

    public async Task switchTurn()
    {
        print("For player number: " + this.currentPlayer + " there are, " +
         this.numOfMovesForCurrentPlayer + " moves left");
        if (this.numOfMovesForCurrentPlayer == 0)
        {
            this.currentPlayer = (currentPlayer + 1) % 4;
            print("Switched to: " + this.currentPlayer);
            this.turnStatisticsPlayerName.GetComponent<TextMeshProUGUI>().text = this.gameManager.players[this.currentPlayer].playerName + "'s Turn:";

            await setInteractive(gameManager.players[0].playerShapes, currentPlayer == 0);

            await setInteractive(gameManager.players[1].playerShapes, currentPlayer == 1);

            await setInteractive(gameManager.players[2].playerShapes, currentPlayer == 2);

            await setInteractive(gameManager.players[3].playerShapes, currentPlayer == 3);

            this.numOfMovesForCurrentPlayer = 1;

            this.currentPlayerContinues = false;

            this.turnStatisticsNumOfMoves.GetComponent<TextMeshProUGUI>().text = "Moves Left: " + this.numOfMovesForCurrentPlayer.ToString();

        }
        else
        {
            this.numOfMovesForCurrentPlayer--;
            this.currentPlayerContinues = true;
            if (!this.isHeHuman())
            {
                await setInteractive(gameManager.players[0].playerShapes, currentPlayer == 0);

                await setInteractive(gameManager.players[1].playerShapes, currentPlayer == 1);

                await setInteractive(gameManager.players[2].playerShapes, currentPlayer == 2);

                await setInteractive(gameManager.players[3].playerShapes, currentPlayer == 3);

                this.turnStatisticsNumOfMoves.GetComponent<TextMeshProUGUI>().text = "Moves Left: " + this.numOfMovesForCurrentPlayer.ToString();

            }
        }

    }
    public async Task startGame()
    {
        this.currentPlayer = 2;
        this.numOfMovesForCurrentPlayer = 0;
        this.isFirstTurn = true;
        await this.switchTurn();

    }

    private async Task endFirstMove()
    {
        this.numOfMovesForCurrentPlayer = 0;
        this.isFirstTurn = false;
        await this.switchTurn();
    }

    public int getPieceNumByType(string shapeClassName)
    {
        int idx = 0;
        foreach (string item in this.shapeOrder)
        {
            var onlyname = shapeClassName.Split('_')[0];
            if (onlyname == item)
            {
                return idx + 1;
            }
            idx++;
        }
        return idx;
    }
    #endregion



    #region Backend communication
    public async Task<int[]> sendStartGame(int shapeNum, int permutation)
    {
        var res = await this.gameManager.msgGameStartToServer(shapeNum, permutation);
        await this.endFirstMove();
        return res;
    }

    public async Task<int[]> sendMove(int shapeNum, int permutation, int new_position_x, int new_position_y)
       => await this.gameManager.msgMoveToServer(this.currentPlayer, shapeNum, permutation, new_position_x, new_position_y);

    public bool isPositionedInBoard(Vector3 pos) => this.gameManager.board.isInBoard(pos);

    public CellClass getNearestCell(Vector3 pos) => this.gameManager.board.getCellByCoordinates(pos);

    public async Task<int[]> requestAiMove() => await this.gameManager.msgAiMoveRequestToServer(this.currentPlayer, this.currentPlayerContinues);

    public bool isHeHuman() => !this.gameManager.players[this.currentPlayer].isAi; //true if the current player isnt Ai player

    #endregion

    #region shapesPrefabs
    /*public static AClass AShape;
    public static PClass PShape;
    public static GClass GShape;
    public static chairClass chairShape;
    public static SevenClass SevenShape;
    public static notnotClass notnotShape;
    public static FiveClass FiveShape;
    public static LambdaClass LambdaShape;
    public static GimelClass GimelShape;
    public static groundClass groundShape;
    public static SixClass SixShape;
    public static shape3Class shape3Shape;
    public static CClass CShape;
    public static EClass EShape;
    public static HClass HShape;
    public static FClass FShape;

    private Dictionary<string, (BaseShape, Type)> shapeLibrary = new Dictionary<string, (BaseShape, Type)>()
    {
        {"P",(PShape,typeof(PClass))},
        {"E",(EShape,typeof(EClass))},
        {"Gimel",(GimelShape,typeof(GimelClass))},
        {"G",(GShape,typeof(GClass))},
        {"shape3",(shape3Shape,typeof(shape3Class))},
        {"C",(CShape,typeof(CClass))},
        {"A",(AShape,typeof(AClass))},
        {"Lambda",(LambdaShape,typeof(LambdaClass))},
        {"H",(HShape,typeof(HClass))},
        {"Six",(SixShape,typeof(SixClass))},
        {"F",(FShape,typeof(FClass))},
        {"notnot",(notnotShape,typeof(notnotClass))},
        {"chair",(chairShape,typeof(chairClass))},
        {"Five",(FiveShape,typeof(FiveClass))},
        {"ground",(groundShape,typeof(groundClass))},
        {"Seven",(SevenShape,typeof(SevenClass))}
    };*/
    #endregion

    #region shape creation
    //For tests only:
    /*
    this goes into the loop in setupShapes:
    orig.shapeManager = this;
            BaseShape shape = Instantiate(orig);
            orig.transform.localPosition = new Vector3(-4000, -4000);
            //var rem = new char[] { '(', ')', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ' };
            //shape.name = orig.name.Trim(rem);
            //Destroy(orig);
            /*if (shape.name.Contains("shape"))
            {
                shape.name += '3';
            }
            shape.name += "_" + playerNum;
            shape.GetComponent<BoxCollider2D>().isTrigger = true;
            int i = 0;

            for (int j = 0; j < shape.transform.childCount; j++)
            {
                shape.transform.GetChild(j).name = "p" + playerNum + " " + shape.name + "line" + i;
                //shape.transform.GetChild(j).gameObject.AddComponent<BoxCollider2D>();
                //shape.transform.GetChild(j).GetComponent<BoxCollider2D>().isTrigger = false;
                i++;
            }
    */
    /*private void initShapesPrefabs()
    {
        shapeLibrary["E"] = (((EClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/E.prefab", typeof(EClass))), shapeLibrary["E"].Item2);
        shapeLibrary["shape3"] = ((shape3Class)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/shape3.prefab", typeof(shape3Class)), shapeLibrary["shape3"].Item2);
        shapeLibrary["G"] = ((GClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/G.prefab", typeof(GClass)), shapeLibrary["G"].Item2);
        shapeLibrary["Lambda"] = ((LambdaClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/Lambda.prefab", typeof(LambdaClass)), shapeLibrary["Lambda"].Item2);
        shapeLibrary["P"] = ((PClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/P.prefab", typeof(PClass)), shapeLibrary["P"].Item2);
        shapeLibrary["notnot"] = ((notnotClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/notnot.prefab", typeof(notnotClass)), shapeLibrary["notnot"].Item2);
        shapeLibrary["H"] = ((HClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/H.prefab", typeof(HClass)), shapeLibrary["H"].Item2);
        shapeLibrary["Gimel"] = ((GimelClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/Gimel.prefab", typeof(GimelClass)), shapeLibrary["Gimel"].Item2);
        shapeLibrary["Six"] = ((SixClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/Six.prefab", typeof(SixClass)), shapeLibrary["Six"].Item2);
        shapeLibrary["C"] = ((CClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/C.prefab", typeof(CClass)), shapeLibrary["C"].Item2);
        shapeLibrary["Seven"] = ((SevenClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/Seven.prefab", typeof(SevenClass)), shapeLibrary["Seven"].Item2);
        shapeLibrary["F"] = ((FClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/F.prefab", typeof(FClass)), shapeLibrary["F"].Item2);
        shapeLibrary["Five"] = ((FiveClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/Five.prefab", typeof(FiveClass)), shapeLibrary["Five"].Item2);
        shapeLibrary["ground"] = ((groundClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/ground.prefab", typeof(groundClass)), shapeLibrary["ground"].Item2);
        shapeLibrary["chair"] = ((chairClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/chair.prefab", typeof(chairClass)), shapeLibrary["chair"].Item2);
        shapeLibrary["A"] = ((AClass)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Shapes/A.prefab", typeof(AClass)), shapeLibrary["A"].Item2);

        foreach (var shapeName in shapeOrder)
        {
            var shapesEntery = shapeLibrary[shapeName];
            var shapeClass = shapesEntery.Item1;
            var shapesType = shapesEntery.Item2;
            shapeClass.assemblingLines = new List<GameObject>();
            for (int i = 0; i < shapeClass.transform.childCount; i++)
            {
                GameObject line = shapeClass.transform.GetChild(i).gameObject;
                shapeClass.assemblingLines.Add(line);
            }
        }
    }

    //The following is a code for generating the shapes by code.
    public void Setup(BoardClass board, CompetitiveGameManager gM)
    {
        initShapesPrefabs();

        gameManager = gM;
        boardtrans = gM.board.transform;
        canvasTrans = gM.canvas.transform;

        player1.playerShapes = CreateShapes(Color.cyan, 1, player1);

        player2.playerShapes = CreateShapes(Color.black, 2, player2);

        player3.playerShapes = CreateShapes(Color.red, 3, player3);

        player4.playerShapes = CreateShapes(Color.green, 4, player4);
    }

    private List<BaseShape> CreateShapes(Color teamColor, int playerNum, PlayerClass bank)
    {
        initShapesPrefabs();
        List<BaseShape> newShapes = new List<BaseShape>();

        for (int i = 0; i < shapeOrder.Length; i++)
        {
            string shapeName = shapeOrder[i];
            (BaseShape, Type) shapeClassType = shapeLibrary[shapeName];
            BaseShape newShape = createShape(shapeClassType.Item1, shapeClassType.Item2, shapeName, playerNum);
            newShape.Setup(teamColor, this);
            newShapes.Add(newShape);
        }
        this.placeShapes(newShapes, bank, playerNum % 2 == 1);
        return newShapes;
    }

    private BaseShape createShape(BaseShape shapeClass, Type shapeType, string shapeName, int pNum)
    {
        //Maybe move it to baseShape class later.
        GameObject shapeObj = Instantiate(mShapePrefab);
        BaseShape newShape = Instantiate((BaseShape)shapeObj.AddComponent(shapeType));
        newShape.name = shapeName + pNum;
        int i = 0;

        foreach (var line in shapeClass.assemblingLines)
        {
            GameObject newline = Instantiate(line);
            newline.name = "p" + pNum + " " + shapeName + "line" + i;
            newline.transform.SetParent(newShape.transform, false);
            newShape.assemblingLines.Add(newline);
            i++;
        }
        return newShape;
    }

    private void placeShapes(List<BaseShape> shapes, PlayerClass bank, bool isVertical = false)
    {
        Vector3 textPos = bank.transform.GetChild(0).localPosition;
        var absoluteX = bank.transform.localPosition.x + textPos.x - 25;
        var absoluteY = bank.transform.localPosition.y + textPos.y - 20;
        float spacingFactor = 2.15f * (this.gameManager.scaleFactor + 10);
        int x_add = 0, y_add = 0;
        shapes.ForEach(delegate (BaseShape shape)
        {
            float new_x = absoluteX + x_add * spacingFactor;
            float new_y = absoluteY - y_add * spacingFactor;
            shape.setupStartPos(new_x, new_y);
            if (isVertical)
            {
                x_add = (x_add + 1) % 8;
            }
            else
            {
                x_add = (x_add + 1) % 3;
            }
            if (x_add == 0)
            {
                y_add++;
            }
        });
    }*/
    #endregion

}