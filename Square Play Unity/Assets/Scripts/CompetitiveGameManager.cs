using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Threading.Tasks;
using System.Net.Http;

public class CompetitiveGameManager : MonoBehaviour
{
    public BoardClass board;
    public ShapesManager shapesManager;
    public GameObject notification;
    public gameCanvasScript gameCanvas;

    public competitveGameCanvasScript preCanvas;

    [HideInInspector]
    public int scaleFactor = 33;
    private int cols = 33;
    private int rows = 33;

    public bool rotationMode = false; //Turns true when "Rotation" Button is clicked

    public bool choosingRotationMode = false; //Turns true when rotation choice is made

    public int chosenRotation = -1;

    public PlayerClass[] players;

    // Use this for initialization
    void Start()
    {
        server_http_addr = this.url + ":" + this.port + "/";

        client = new HttpClient();

        board.generate(cols, rows, this); //give that board instance access to the python comm functions, via the socket interface

        shapesManager.Setup(this.board, this);
    }

    /*TODO:
        When a player connects to the game, if he is:
        - player 1 - than activate player1 prefab and set it to be his prefab ( spawn Player1 prefab for him... )
        - player 2 - than activate both player1 and player2 prefabs and set player2 to be this player prefab
        - etc... 
    */

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

    private IEnumerator announceNotification(string notificationMsg)
    {
        print("the notification to be shown:" + notificationMsg);
        this.notification.gameObject.SetActive(true);
        this.notification.GetComponent<TextMeshProUGUI>().text = notificationMsg;
        yield return new WaitForSeconds(3);
        this.notification.gameObject.SetActive(false);
        this.notification.GetComponent<TextMeshProUGUI>().text = "";
    }

    public void showNotification(string msg)
    {
        StartCoroutine(this.announceNotification(msg));
    }
    public async Task startGame()
    {
        this.showNotification(players[3].playerName + ", choose the piece you'd like to put in the middle of the board");
        await this.shapesManager.startGame();
    }


    #region python communcation

    /*
for a real player move: 
From Unity: [player num,shape num, permutation,x position, y position]
Response from Backend: [ number that indicates whether the move was legal , number of squares closed]

for ai player move: 
From Unity: [player num]
Response from Backend: [ shape num,permutation,x position, y position, number of squares closed]
*/
    #region Network helpers
    private string[] empty = { "", "", "", "", "", "", "" };
    private int namesCode = 0;
    private string[] namesFromUnity = { "p1", "p2", "p3", "p4" };
    private string[] namesRespBack = { "game_id" };
    private int firstMoveCode = 1;
    private string[] firstFromUnity = { "piece", "perm" };
    private string[] firstRespBack = { "d" };
    private int realMoveCode = 2;
    private string[] realFromUnity = { "p_num", "piece", "perm", "x_coor", "y_coor" };
    private string[] realRespBack = { "number that indicates whether the move was legal", "number of squares closed" };
    private int aiMoveCode = 3;
    private string[] aiFromUnity = { "p_num" };
    private string[] aiRespBack = { "shape num", "permutation", "x position", "y position", "number of squares closed" };
    //private int passTurnCode = 4;

    public struct namesRespBackStr { public string game_id; }
    public struct firstRespBackStr { public string some; }
    public struct realRespBackStr { public int number_that_indicates_whether_the_move_was_legal; public int number_of_squares_closed; };
    public struct aiRespBackStr { public int shape_num; public int permutation; public int x_position; public int y_position; public int number_of_squares_closed; };
    [SerializeField]
    private string[] dataOut;
    private int[] dataIn;

    #endregion
    private string url = "http://132.69.8.19";
    private string port = "80";
    public string server_http_addr = "";
    public string currentGameId = "";
    private HttpClient client;
    /*
        Examples of valid GET messages:
        http://url:port/start_new_game?p1=p_1&p2=p_2&p3=AI_Player1&p4=AI_Player2
        http://url:port/first_move?gid=1298297976736304479&piece=2&perm=1
        http://url:port/reg_move?gid=1298297976736304479&p_num=1&piece=15&perm=1&x_coor=15&y_coor=13
        http://url:port/pass_turn?gid=1298297976736304479&p_num=2
        http://url:port/ai_move?gid=1298297976736304479
        http://url:port/end_game?gid=1298297976736304479
    */

    private async Task<int[]> sendMessageByCode(int code)
    {
        var response = await client.GetAsync(server_http_addr + constructDataOut(code));
        string result = await response.Content.ReadAsStringAsync();
        parseReply(result, code);
        return dataIn;
    }

    void OnApplicationQuit()
    {
        print("bye!");

        client.GetAsync(server_http_addr + "end_game?gid=" + currentGameId);

    }

    public async Task byeBye()
    {
        print("bye bye!");
        await client.GetAsync(server_http_addr + "end_game?gid=" + currentGameId);
    }

    private void parseReply(string result, int code = 0)//
    {
        string[] addition = { };
        string str = "Data";
        dataIn = new int[5];
        switch (code)
        {
            case 0:
                addition = namesRespBack;
                str = "Names";
                namesRespBackStr nameParesd = JsonUtility.FromJson<namesRespBackStr>(result);
                dataIn[0] = int.Parse(nameParesd.game_id.Substring(0, 3));
                this.currentGameId = nameParesd.game_id;
                break;
            case 1:
                addition = firstRespBack;
                str = "First Move";
                firstRespBackStr firstParsed = JsonUtility.FromJson<firstRespBackStr>(result);
                dataIn[0] = 1;
                break;
            case 2:
                addition = realRespBack;
                str = "Move";
                realRespBackStr moveParsed = JsonUtility.FromJson<realRespBackStr>(result);
                dataIn[0] = moveParsed.number_that_indicates_whether_the_move_was_legal;
                dataIn[1] = moveParsed.number_of_squares_closed;
                break;
            case 3:
                addition = aiRespBack;
                str = "Ai move request";
                aiRespBackStr aiMove = JsonUtility.FromJson<aiRespBackStr>(result);
                dataIn[0] = aiMove.shape_num;
                dataIn[1] = aiMove.permutation;
                dataIn[2] = aiMove.x_position;
                dataIn[3] = aiMove.y_position;
                dataIn[4] = aiMove.number_of_squares_closed;
                break;
            case 4:
                addition = empty;
                str = "Skip turn";
                break;
            default:
                addition = empty;
                break;
        }
        str = ("For " + str + ", got from server: " + "\n");
        for (int i = 0; i < addition.Length; i++)
        {
            str += (addition[i] + ": " + dataIn[i] + "\n");
        }
        Debug.Log(str);
    }

    private string constructDataOut(int code = 0)
    {
        string[] addition = { };
        string str = "Data";
        string strOut = "";
        switch (code)
        {
            case 0:
                addition = namesFromUnity;
                strOut = "start_new_game?";
                str = "Names";
                break;
            case 1:
                addition = firstFromUnity;
                strOut = "first_move?gid=" + currentGameId + "&";
                str = "First move";
                break;
            case 2:
                addition = realFromUnity;
                strOut = "reg_move?gid=" + currentGameId + "&";
                str = "Move";
                break;
            case 3:
                addition = aiFromUnity;
                str = "Ai move request sent to server.";
                Debug.Log(str);
                return "ai_move?gid=" + currentGameId;
            case 4:
                addition = empty;
                str = "Skip turn";
                break;
            default:
                addition = empty;
                break;
        }
        str = str + " sent to server: " + "\n";
        for (int i = 0; i < this.dataOut.Length; i++)
        {
            str += (addition[i] + ": " + this.dataOut[i] + "\n");
            strOut += addition[i] + "=" + this.dataOut[i] + "&";
        }
        Debug.Log(str);
        strOut = strOut.Remove(strOut.Length - 1);
        return strOut;
    }

    public async Task<int[]> msgNamesToServer()
    {
        try
        {
            this.dataOut = new string[4];
            foreach (var player in this.players)
            {
                string stringToInsert = player.playerName;
                if (player.isAi)
                {
                    stringToInsert += "_AI_Player";
                }
                this.dataOut[player.playerNum - 1] = stringToInsert;
            }
            return await sendMessageByCode(namesCode);
        }
        catch (Exception e)
        {
            Debug.Log("An exception occourd in sending names to server!\n The exception is:");
            Debug.Log(e);
            this.OnApplicationQuit();
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgGameStartToServer(int shapeNum, int permutation)
    {
        try
        {
            this.dataOut = new string[2] { shapeNum.ToString(), permutation.ToString() };
            return await sendMessageByCode(firstMoveCode);
        }
        catch (Exception e)
        {
            Debug.Log("An exception occourd in sending start game to server!\n The exception is:");
            Debug.Log(e);
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgMoveToServer(int playerNum, int pieceNum, int permutation, int new_position_x, int new_position_y)
    {
        try
        {
            //Adding 1 to player num since here player nums are: 0-3, while in the backend they are 1-4
            this.dataOut = new string[5] { (playerNum + 1).ToString(), pieceNum.ToString(), permutation.ToString(), new_position_x.ToString(), new_position_y.ToString() };
            return await sendMessageByCode(realMoveCode);
        }
        catch (Exception e)
        {
            Debug.Log("An exception occourd in sending move to server!\n The exception is:");
            Debug.Log(e);

            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgAiMoveRequestToServer(int playerNum, bool cont)
    {
        try
        {
            //Adding 1 to player num since here player nums are: 0-3, while in the backend they are 1-4
            this.dataOut = new string[1] { (playerNum + 1).ToString() };
            return await sendMessageByCode(aiMoveCode);
        }
        catch (Exception e)
        {
            Debug.Log("An exception occourd in sending ai move request to server!\n The exception is:");
            Debug.Log(e);
            return new int[1] { -1 };
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
