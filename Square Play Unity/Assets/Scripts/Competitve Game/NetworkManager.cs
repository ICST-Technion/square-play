using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
//using System.Net.Http.WebRequest;

public class NetworkManager : MonoBehaviour
{
        #region Network helpers
    private string[] empty = { "", "", "", "", "", "", "" };
    private const int namesCode = 0;
    private string[] namesFromUnity = { "p1", "p2", "p3", "p4" };
    private string[] namesRespBack = { "game_id" };
    private const int firstMoveCode = 1;
    private string[] firstFromUnity = { "piece", "perm" };
    private string[] firstRespBack = { "d" };
    private const int realMoveCode = 2;
    private string[] realFromUnity = { "p_num", "piece", "perm", "x_coor", "y_coor" };
    private string[] realRespBack = { "number that indicates whether the move was legal", "number of squares closed" };
    private const int aiMoveCode = 3;
    private string[] aiFromUnity = { "p_num" };
    private string[] aiRespBack = { "shape num", "permutation", "x position", "y position", "number of squares closed" };
    private const int passTurnCode = 4;
    private const int newGameCode = 5;
    private string[] newGameFromUnity = { "p_name" };
    private string[] newGameRespBack = { "game_id", "p_num" };
    private const int joinGameCode = 6;
    private string[] joinGameFromUnity = { "game_id","p_name" };
    private string[] joinGameRespBack = { "p_num" };

    private struct namesRespBackStr { public string game_id; }
    private struct firstRespBackStr { public string some; }
    private struct realRespBackStr { public int number_that_indicates_whether_the_move_was_legal; public int number_of_squares_closed; };
    private struct aiRespBackStr { public int shape_num; public int permutation; public int x_position; public int y_position; public int number_of_squares_closed; };
    private struct newGameRespBackStr { public string game_id; public string player_num; }
    private struct joinGameRespBackStr { public string player_num; }

    [SerializeField]
    private string[] _dataOut;
    private int[] _dataIn;
    private PlayerClass[] players;

    #endregion
   
    private string _url = "http://132.69.8.19";
    private string _port = "80";
    private string _server_http_addr = "";
    private string _currentGameId = "";
    private HttpClient _client;
    private HttpListener _listener;
    //private WebRequestHandler _clientHandler;

    /*
        Examples of valid GET messages:
        http://url:port/start_new_game?p1=p_1&p2=p_2&p3=AI_Player1&p4=AI_Player2
        http://url:port/first_move?gid=1298297976736304479&piece=2&perm=1
        http://url:port/reg_move?gid=1298297976736304479&p_num=1&piece=15&perm=1&x_coor=15&y_coor=13
        http://url:port/pass_turn?gid=1298297976736304479&p_num=2
        http://url:port/ai_move?gid=1298297976736304479
        http://url:port/end_game?gid=1298297976736304479
    */
    // Start is called before the first frame update
    void Start()
    {   
        /*X509Store store;
        try{
            store =new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
        }
        finally*/
        //System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        _server_http_addr = this._url + ":" + this._port + "/";

        

        /*_clientHandler = new WebRequestHandler();
        _clientHandler.ClientCertificates.Add(cert);

        _client = new HttpClient(_clientHandler);*/
        _client = new HttpClient();
        _listener = new HttpListener();
        _listener.Prefixes.Add(_server_http_addr);
        _listener.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
for a real player move: 
From Unity: [player num,shape num, permutation,x position, y position]
Response from Backend: [ number that indicates whether the move was legal , number of squares closed]

for ai player move: 
From Unity: [player num]
Response from Backend: [ shape num,permutation,x position, y position, number of squares closed]
*/
    #region Generic network functions
    private async Task<int[]> sendMessageByCode(int code)
    {
        try{
        var dataout = constructDataOut(code);
        var response = await _client.GetAsync(_server_http_addr + dataout);
        string result = await response.Content.ReadAsStringAsync();
        parseReply(result, code);
        return _dataIn;
    }catch (Exception e)
        {
            print("An exception occourd in sending names to server!\n The exception is:");
            print(e);
            //this.OnApplicationQuit();
            return new int[1] { -1 };
        }
    }

        private void parseReply(string result, int code = 0)//
    {
        string[] addition = { };
        string str = "Data";
        _dataIn = new int[5];
        switch (code)
        {
            case namesCode:
                addition = namesRespBack;
                str = "Names";
                namesRespBackStr nameParesd = JsonUtility.FromJson<namesRespBackStr>(result);
                _dataIn[0] = int.Parse(nameParesd.game_id.Substring(0, 3));
                this._currentGameId = nameParesd.game_id;
                break;
            case firstMoveCode:
                addition = firstRespBack;
                str = "First Move";
                firstRespBackStr firstParsed = JsonUtility.FromJson<firstRespBackStr>(result);
                _dataIn[0] = 1;
                break;
            case realMoveCode:
                addition = realRespBack;
                str = "Move";
                realRespBackStr moveParsed = JsonUtility.FromJson<realRespBackStr>(result);
                _dataIn[0] = moveParsed.number_that_indicates_whether_the_move_was_legal;
                _dataIn[1] = moveParsed.number_of_squares_closed;
                break;
            case aiMoveCode:
                addition = aiRespBack;
                str = "Ai move request";
                aiRespBackStr aiMove = JsonUtility.FromJson<aiRespBackStr>(result);
                _dataIn[0] = aiMove.shape_num;
                _dataIn[1] = aiMove.permutation;
                _dataIn[2] = aiMove.x_position;
                _dataIn[3] = aiMove.y_position;
                _dataIn[4] = aiMove.number_of_squares_closed;
                break;
            case passTurnCode:
                addition = empty;
                str = "Skip turn";
                break;
            case newGameCode:
                addition = newGameRespBack;
                str = "New game created";
                newGameRespBackStr newGameParesd = JsonUtility.FromJson<newGameRespBackStr>(result);
                _dataIn[0] = int.Parse(newGameParesd.game_id.Substring(0, 3));
                _dataIn[1] = int.Parse(newGameParesd.player_num.Substring(0, 3));
                this._currentGameId = newGameParesd.game_id;
                break;
            case joinGameCode:
                addition = joinGameRespBack;
                str = "Joined game!";
                joinGameRespBackStr joinGameParsed = JsonUtility.FromJson<joinGameRespBackStr>(result);
                _dataIn[0] = int.Parse(joinGameParsed.player_num.Substring(0, 3));
                break;

            default:
                addition = empty;
                break;
        }
        str = ("For " + str + ", got from server: " + "\n");
        for (int i = 0; i < addition.Length; i++)
        {
            str += (addition[i] + ": " + _dataIn[i] + "\n");
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
            case namesCode:
                addition = namesFromUnity;
                strOut = "start_new_game?";
                str = "Names";
                break;
            case firstMoveCode:
                addition = firstFromUnity;
                strOut = "first_move?gid=" + _currentGameId + "&";
                str = "First move";
                break;
            case realMoveCode:
                addition = realFromUnity;
                strOut = "reg_move?gid=" + _currentGameId + "&";
                str = "Move";
                break;
            case aiMoveCode:
                addition = aiFromUnity;
                str = "Ai move request sent to server.";
                Debug.Log(str);
                return "ai_move?gid=" + _currentGameId;
            case passTurnCode:
                addition = empty;
                str = "Skip turn";
                Debug.Log(str);
                return "pass_turn?gid="+ _currentGameId+"&p_num="+this._dataOut[0];
            case newGameCode:
                addition = newGameFromUnity;
                str = "New Game!";
                Debug.Log(str);
                return "new_game?p_num=" + this._dataOut[0];
            case joinGameCode:
                addition = joinGameFromUnity;
                str = "New Game!";
                Debug.Log(str);
                return "join_game?gid=" + this._dataOut[0] +"&p_num =" + this._dataOut[1];
            default:
                addition = empty;
                break;
        }
        str = str + " sent to server: " + "\n";
        for (int i = 0; i < this._dataOut.Length; i++)
        {
            str += addition[i] + ": " + this._dataOut[i] + "\n";
            strOut += addition[i] + "=" + this._dataOut[i] + "&";
        }
        Debug.Log(str);
        strOut = strOut.Remove(strOut.Length - 1);
        return strOut;
    }
    #endregion

    #region Network interface
    
    #region Multiplayer functions
    public async Task<int[]> msgNewMultiplayerGameToServer(string player_name)
    {
        try
        {
            this._dataOut = new string[1] { player_name };
            int[] result = await sendMessageByCode(newGameCode);
            this._currentGameId = result[0].ToString();
            return result;
        }
        catch (Exception e)
        {
            print("An exception occourd in sending creat new multiplayer game to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgJoinMultiplayerGameToServer(string game_id, string player_name)
    {
        try
        {
            this._dataOut = new string[2] { game_id,player_name };
            int[] result = await sendMessageByCode(newGameCode);
            this._currentGameId = game_id;
            return result;
        }
        catch (Exception e)
        {
            print("An exception occourd in sending join multiplayer game to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }
    #endregion

    #region Competitve game functions (for both singleplayer and multiplayer)
    public async Task<int[]> msgNamesToServer(PlayerClass[] playersClassesList)
    {
        this.players = playersClassesList;
        try
        {
            this._dataOut = new string[4];
            foreach (var player in this.players)
            {
                string stringToInsert = player.playerName;
                if (player.isAi)
                {
                    stringToInsert += "_AI_Player";
                }
                this._dataOut[player.playerNum - 1] = stringToInsert;
            }
            return await sendMessageByCode(namesCode);
        }
        catch (Exception e)
        {
            print("An exception occourd in sending names to server!\n The exception is:");
            print(e);
            //this.OnApplicationQuit();
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgGameStartToServer(int shapeNum, int permutation)
    {
        try
        {
            this._dataOut = new string[2] { shapeNum.ToString(), permutation.ToString() };
            return await sendMessageByCode(firstMoveCode);
        }
        catch (Exception e)
        {
            print("An exception occourd in sending start game to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgMoveToServer(int playerNum, int pieceNum, int permutation, int new_position_x, int new_position_y)
    {
        try
        {
            //Adding 1 to player num since here player nums are: 0-3, while in the backend they are 1-4
            this._dataOut = new string[5] { (playerNum + 1).ToString(), pieceNum.ToString(), permutation.ToString(), new_position_x.ToString(), new_position_y.ToString() };
            return await sendMessageByCode(realMoveCode);
        }
        catch (Exception e)
        {
            print("An exception occourd in sending move to server!\n The exception is:");
            print(e);

            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgAiMoveRequestToServer(int playerNum, bool cont)
    {
        try
        {
            //Adding 1 to player num since here player nums are: 0-3, while in the backend they are 1-4
            this._dataOut = new string[1] { (playerNum + 1).ToString() };
            return await sendMessageByCode(aiMoveCode);
        }
        catch (Exception e)
        {
           print("An exception occourd in sending ai move request to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgPassTurnToServer(int playerNum)
    {
        try
        {
            //Adding 1 to player num since here player nums are: 0-3, while in the backend they are 1-4
            this._dataOut = new string[1] { (playerNum + 1).ToString() };
            return await sendMessageByCode(passTurnCode);
        }
        catch (Exception e)
        {
            print("An exception occourd in sending ai move request to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }
    #endregion
    public async Task byeBye()
    {
        await _client.GetAsync(_server_http_addr + "end_game?gid=" + _currentGameId);
    }

    #endregion 
}