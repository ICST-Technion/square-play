using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using TMPro;
using UnityEngine;

public class CompetitiveGameManager : MonoBehaviour
{
    public BoardClass board;
    public ShapesManager shapesManager;
    public GameObject notification;
    public gameCanvasScript gameCanvas;
    public NetworkManager netManager;
    private string _currentGameId = "";
    static bool wantsToCreateGame;
    static bool wantsOnlineGame;

    public startGameCanvasScript startNewGameCanvas;

    public GameObject turnStatisticsPlayerName;

    public GameObject turnStatisticsNumOfMoves;

    public GameObject turnStatisticsTimeLeft;

    [HideInInspector]
    public float scaleFactor = 33;
    private int cols = 33;
    private int rows = 33;

    public bool rotationMode = false; //Turns true when "Rotation" Button is clicked

    public bool choosingRotationMode = false; //Turns true when rotation choice is made

    public int chosenRotation = -1;
    [HideInInspector]
    public int timeForMove = 30000;//The default for now is 30 sceonds
    private System.Timers.Timer turnTimer;

    public PlayerClass[] players;

    // Use this for initialization
    void Start()
    {
        print(wantsOnlineGame);
        print(wantsToCreateGame);
        this.scaleFactor = 33 * GetScale(2560, 1440); ;
        var scaleVec = new Vector3(this.scaleFactor, this.scaleFactor);
        this.gameCanvas.transform.localScale = scaleVec;
        this.startNewGameCanvas.transform.localScale = scaleVec;
        board.generate(cols, rows, this); //give that board instance access to the python comm functions, via the socket interface

        shapesManager.Setup(this.board, this);

        setupTimer(false);
    }

    void Update()
    {
        this.turnStatisticsTimeLeft.GetComponent<TextMeshProUGUI>().text = "Time left: " + this.turnTimer.Interval;
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
        await this.shapesManager.switchTurn(); //may cause to problems! remove the await if it does
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

    #region python communcation

    void OnApplicationQuit()
    {
        print("bye!");

        this.netManager.byeBye();
    }

    public async Task byeBye()
    {
        this.netManager.byeBye();
    }

    public async Task<int[]> msgNamesToServer()
    {
        return await this.netManager.msgNamesToServer(this.players);
    }

    public async Task<int[]> msgGameStartToServer(int shapeNum, int permutation)
    {
        return await this.netManager.msgGameStartToServer(shapeNum, permutation);
    }

    public async Task<int[]> msgMoveToServer(int playerNum, int pieceNum, int permutation, int new_position_x, int new_position_y)
    {
        return await this.netManager.msgMoveToServer(playerNum, pieceNum, permutation, new_position_x, new_position_y);
    }

    public async Task<int[]> msgAiMoveRequestToServer(int playerNum, bool cont)
    {
        return await this.netManager.msgAiMoveRequestToServer(playerNum, cont);
    }

    public async Task<int[]> msgPassTurnToServer(int playerNum)
    {
        return await this.netManager.msgPassTurnToServer(playerNum);
    }

    public async Task<int[]> msgNewMultiplayerGameToServer(string player_name)
    {
        this.players[0].thisIsMe(player_name);
        int[] result = await this.netManager.msgNewMultiplayerGameToServer(player_name);
        this._currentGameId = result[0].ToString();
        activatePlayersFromNum(0);
        return result;
    }

    public async Task<int[]> msgJoinMultiplayerGameToServer(string game_id, string player_name)
    {
        int[] result = await this.netManager.msgJoinMultiplayerGameToServer(game_id, player_name);
        this._currentGameId = game_id;
        int player_num = result[1];
        this.players[player_num - 1].thisIsMe(player_name);
        activatePlayersFromNum(player_num - 1);
        return result;
    }

    private void activatePlayersFromNum(int active_player_num)
    {
        foreach (var player in this.players)
        {
            if (player.playerNum <= active_player_num)
            {
                player.turnMeOn();
            }
            else
            {
                player.turnMeOff();
            }
        }
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
