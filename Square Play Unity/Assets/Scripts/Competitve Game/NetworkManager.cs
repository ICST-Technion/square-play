using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Http;
using System.Threading.Tasks;
//using System.Net.Http.WebRequest;

public class NetworkManager : MonoBehaviour
{
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
    private int passTurnCode = 4;

    private struct namesRespBackStr { public string game_id; }
    private struct firstRespBackStr { public string some; }
    private struct realRespBackStr { public int number_that_indicates_whether_the_move_was_legal; public int number_of_squares_closed; };
    private struct aiRespBackStr { public int shape_num; public int permutation; public int x_position; public int y_position; public int number_of_squares_closed; };
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
    
    private async Task<int[]> sendMessageByCode(int code)
    {
        var response = await _client.GetAsync(_server_http_addr + constructDataOut(code));
        string result = await response.Content.ReadAsStringAsync();
        parseReply(result, code);
        return _dataIn;
    }

        private void parseReply(string result, int code = 0)//
    {
        string[] addition = { };
        string str = "Data";
        _dataIn = new int[5];
        switch (code)
        {
            case 0:
                addition = namesRespBack;
                str = "Names";
                namesRespBackStr nameParesd = JsonUtility.FromJson<namesRespBackStr>(result);
                _dataIn[0] = int.Parse(nameParesd.game_id.Substring(0, 3));
                this._currentGameId = nameParesd.game_id;
                break;
            case 1:
                addition = firstRespBack;
                str = "First Move";
                firstRespBackStr firstParsed = JsonUtility.FromJson<firstRespBackStr>(result);
                _dataIn[0] = 1;
                break;
            case 2:
                addition = realRespBack;
                str = "Move";
                realRespBackStr moveParsed = JsonUtility.FromJson<realRespBackStr>(result);
                _dataIn[0] = moveParsed.number_that_indicates_whether_the_move_was_legal;
                _dataIn[1] = moveParsed.number_of_squares_closed;
                break;
            case 3:
                addition = aiRespBack;
                str = "Ai move request";
                aiRespBackStr aiMove = JsonUtility.FromJson<aiRespBackStr>(result);
                _dataIn[0] = aiMove.shape_num;
                _dataIn[1] = aiMove.permutation;
                _dataIn[2] = aiMove.x_position;
                _dataIn[3] = aiMove.y_position;
                _dataIn[4] = aiMove.number_of_squares_closed;
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
            case 0:
                addition = namesFromUnity;
                strOut = "start_new_game?";
                str = "Names";
                break;
            case 1:
                addition = firstFromUnity;
                strOut = "first_move?gid=" + _currentGameId + "&";
                str = "First move";
                break;
            case 2:
                addition = realFromUnity;
                strOut = "reg_move?gid=" + _currentGameId + "&";
                str = "Move";
                break;
            case 3:
                addition = aiFromUnity;
                str = "Ai move request sent to server.";
                Debug.Log(str);
                return "ai_move?gid=" + _currentGameId;
            case 4:
                addition = empty;
                str = "Skip turn";
                Debug.Log(str);
                return "pass_turn?gid="+ _currentGameId+"&p_num"+this._dataOut[0];
            default:
                addition = empty;
                break;
        }
        str = str + " sent to server: " + "\n";
        for (int i = 0; i < this._dataOut.Length; i++)
        {
            str += (addition[i] + ": " + this._dataOut[i] + "\n");
            strOut += addition[i] + "=" + this._dataOut[i] + "&";
        }
        Debug.Log(str);
        strOut = strOut.Remove(strOut.Length - 1);
        return strOut;
    }

    
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
            Debug.Log("An exception occourd in sending names to server!\n The exception is:");
            Debug.Log(e);
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
            this._dataOut = new string[5] { (playerNum + 1).ToString(), pieceNum.ToString(), permutation.ToString(), new_position_x.ToString(), new_position_y.ToString() };
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
            this._dataOut = new string[1] { (playerNum + 1).ToString() };
            return await sendMessageByCode(aiMoveCode);
        }
        catch (Exception e)
        {
            Debug.Log("An exception occourd in sending ai move request to server!\n The exception is:");
            Debug.Log(e);
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
            Debug.Log("An exception occourd in sending ai move request to server!\n The exception is:");
            Debug.Log(e);
            return new int[1] { -1 };
        }
    }

    public async Task byeBye()
    {
        await _client.GetAsync(_server_http_addr + "end_game?gid=" + _currentGameId);
    }
}
