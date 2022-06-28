using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using TMPro;
using UnityEngine;

public class CompetitiveGameManager : MonoBehaviour
{
    #region Game management elements
    public ShapesManager shapesManager;
    public NetworkManager netManager;
    private string _currentGameId = "";
    private bool wantsToCreateGame = GameValues.wantsToCreateGame;
    private bool wantsOnlineGame = GameValues.wantsOnlineGame;
    #endregion

    #region UI elements
    public PlayerClass[] players; //list of player objects
    public BoardClass board;
    public GameObject notification;
    public gameCanvasScript gameCanvas;
    public singlePlayerCanvasScript singlePlayerCanvas;
    public multiPlayerCanvasScript multiPlayerCanvas;
    public GameObject turnStatisticsPlayerName;
    public GameObject turnStatisticsNumOfMoves;
    public GameObject turnStatisticsTimeLeft;

    [HideInInspector]
    public float scaleFactor = 33;
    [HideInInspector]
    public int timeForMove = 30000;//The default for now is 30 sceonds
    private int cols = 32;
    private int rows = 32;

    public bool rotationMode = false; //Turns true when "Rotation" Button is clicked
    public bool choosingRotationMode = false; //Turns true when rotation choice is made
    public int chosenRotation = -1;
    private System.Timers.Timer turnTimer;
    #endregion

    // Use this for initialization
    void Start()
    {
        this.gameCanvas.gameObject.SetActive(false);
        if (wantsOnlineGame == false)
        {
            this.multiPlayerCanvas.gameObject.SetActive(false);
            this.singlePlayerCanvas.gameObject.SetActive(true);
        }
        else
        {
            this.multiPlayerCanvas.gameObject.SetActive(true);
            this.singlePlayerCanvas.gameObject.SetActive(false);
        }
        /*this.scaleFactor = 33 * GetScale(2560, 1440); 
        var scaleVec = new Vector3(this.scaleFactor, this.scaleFactor);
        print("scale vec: " + scaleVec.ToString());
        this.gameCanvas.transform.localScale = scaleVec;
        this.singlePlayerCanvas.transform.localScale = scaleVec;*/
        board.generate(cols, rows, this); //give that board instance access to the python comm functions, via the socket interface

        shapesManager.Setup(this.board, this);

        setupTimer();
    }

    void Update()
    {
        this.turnStatisticsTimeLeft.GetComponent<TextMeshProUGUI>().text = "Time left: " + this.turnTimer.ToString();
    }

    private void setupTimer(bool activate = true)
    {
        this.turnTimer = new System.Timers.Timer(timeForMove);

        this.turnTimer.Elapsed += OnTimedEvent;

        this.turnTimer.AutoReset = false;

        this.turnTimer.Enabled = activate;
    }

    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                          e.SignalTime);
        //this.passTurn();
        this.setupTimer();
    }

    public async Task passTurn()
    {
        await this.msgPassTurnToServer(this.shapesManager.currentPlayer);
        await this.shapesManager.switchTurn(); //may be a cause of problems! remove the await if it does
    }

    public void randomizePlayerNames()
    {
        string[] possibleNames = { "Jonatan", "Mai", "Hadi", "Daniel", "Ron", "Elazar", "Moshe", "Roey", "Amit" };
        List<int> usedIndexes = new List<int>();
        System.Random rand = new System.Random();
        for (int i = 0; i < 4; i++)
        {
            var idx = rand.Next(possibleNames.Length);
            while (usedIndexes.Contains(idx))
            {
                idx = rand.Next(possibleNames.Length);
            }
            usedIndexes.Add(idx);
            this.players[i].playerName = possibleNames[idx];
        }
    }

    private float GetScale(float width, float height)
    {
        var pt1 = Mathf.Pow(width / 1920, 1f - 0.5f);
        var pt2 = Mathf.Pow(height / 1080, 0.5f);
        print(pt1);
        print(pt2);
        return pt1 * pt2;
    }
    private IEnumerator announceNotification(string notificationMsg)
    {
        print("the notification to be shown:" + notificationMsg);
        this.notification.gameObject.SetActive(true);
        this.notification.GetComponent<TextMeshProUGUI>().text = notificationMsg;
        yield return new WaitForSeconds(3);
        this.notification.gameObject.SetActive(false);
        this.notification.GetComponent<TextMeshProUGUI>().text = "";
    }

    public void updatePlayerNames()
    {
        foreach (var player in players)
        {
            player.updateName();
            if (player.name == this.netManager.playerName)
            {
                player.isItMe = true;
            }
        }
    }

    public void updateCurrentPlayerName(int currentPlayer)
    {
        this.turnStatisticsPlayerName.GetComponent<TextMeshProUGUI>().text = this.players[currentPlayer].playerName + "'s Turn!";
    }

    public void updateNumOfMovesLeft(int numOfMovesForCurrentPlayer)
    {
        this.turnStatisticsNumOfMoves.GetComponent<TextMeshProUGUI>().text = "Moves Left: " + numOfMovesForCurrentPlayer.ToString();
    }

    public void resetTimeLeftForCurrentPlayer()
    {
        this.turnTimer.Stop();
        this.turnTimer.Dispose();
        this.setupTimer();
    }

    public void updatePlayersAtGameStart(string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            players[i].name = names[i];
            players[i].playerNum = i;
            if (players[i].name.Contains("AI"))
            {
                players[i].isAi = true;
            }
            else
            {
                players[i].isAi = false;
            }
        }
        updatePlayerNames();
    }

    public bool isAdmin()
    {
        return this.netManager.isAdmin;
    }

    public void showNotification(string msg)
    {
        StartCoroutine(this.announceNotification(msg));
    }
    public async Task startGame()
    {
        this.showNotification(players[3].playerName + ", choose the piece you'd like to put in the middle of the board");
        await this.shapesManager.startGame();
        //this.setupTimer();
    }

    public async Task activateGame(bool activator)
    {
        this.updatePlayerNames();
        this.multiPlayerCanvas.gameObject.SetActive(false);
        gameCanvas.gameObject.SetActive(true);
        if (activator)
        {
            await this.msgActivateGameToServer(this.netManager.roomName);
        }
        else { await this.startGame(); }
    }

    #region python communcation

    void OnApplicationQuit()
    {
        print("bye!");

        this.netManager.byeBye();
    }

    public async Task byeBye()
    {
        await this.netManager.byeBye();
    }

    public async Task<int[]> msgNamesToServer()
    {
        int[] result = await this.netManager.msgNamesToServer(this.players);
        if (result[0] == Int16.MinValue)
        {
            showNotification("Error from server: " + this.netManager.errorFromServer);
            return new int[1] { -1 };
        }
        return result;
    }

    public async Task<int[]> msgGameStartToServer(int shapeNum, int permutation)
    {
        int[] result = await this.netManager.msgGameStartToServer(shapeNum, permutation);
        if (result[0] == Int16.MinValue)
        {
            showNotification("Error from server: " + this.netManager.errorFromServer);
            return new int[1] { -1 };
        }
        return result;
    }

    public async Task<int[]> msgMoveToServer(int playerNum, int pieceNum, int permutation, int new_position_x, int new_position_y)
    {
        int[] result = await this.netManager.msgMoveToServer(playerNum, pieceNum, permutation, new_position_x, new_position_y);
        if (result[0] == Int16.MinValue)
        {
            showNotification("Error from server: " + this.netManager.errorFromServer);
            return new int[1] { -1 };
        }
        return result;
    }

    public async Task<int[]> msgAiMoveRequestToServer(int playerNum, bool cont)
    {
        int[] result = await this.netManager.msgAiMoveRequestToServer(playerNum, cont);
        if (result[0] == Int16.MinValue)
        {
            showNotification("Error from server: " + this.netManager.errorFromServer);
            return new int[1] { -1 };
        }
        return result;
    }

    public async Task<int[]> msgPassTurnToServer(int playerNum)
    {
        int[] result = await this.netManager.msgPassTurnToServer(playerNum);
        if (result[0] == Int16.MinValue)
        {
            showNotification("Error from server: " + this.netManager.errorFromServer);
            return new int[1] { -1 };
        }
        return result;
    }

    public async Task<int[]> msgNewMultiplayerGameToServer(string room_name, string player_name)
    {
        this.players[0].thisIsMe(player_name);
        int[] result = await this.netManager.msgNewMultiplayerGameToServer(room_name, player_name);
        if (result[0] == Int16.MinValue)
        {
            showNotification("Error from server: " + this.netManager.errorFromServer);
            return new int[1] { -1 };
        }
        this._currentGameId = result[0].ToString();
        return result;
    }

    public async Task<int[]> msgJoinMultiplayerGameToServer(string room_name, string player_name)
    {
        int[] result = await this.netManager.msgJoinMultiplayerGameToServer(room_name, player_name);
        if (result[0] == Int16.MinValue)
        {
            showNotification("Error from server: " + this.netManager.errorFromServer);
            return new int[1] { -1 };
        }
        int player_num = result[1];
        this.players[player_num - 1].thisIsMe(player_name);
        return result;
    }

    public async Task<int[]> msgActivateGameToServer(string room_name)
    {
        int[] result = await this.netManager.msgActivateRoomToServer(room_name);
        if (result[0] == Int16.MinValue)
        {
            showNotification("Error from server: " + this.netManager.errorFromServer);
            return new int[1] { -1 };
        }
        return result;
    }


    #endregion

    /*string debugGameState()
	{
		string map = "";

		for (int x = 0; x < 8; x++)
		{

			map += x + ": ";

			for (int y = 0; y < 8; y++)
			{
				map += "[" + boardGrid[x, y] + "]";
			}
			map += "\n";
		}

		return map;
	}

	void updateDebugView()
	{
		DebugText.text = debugGameState();

	}*/
}
