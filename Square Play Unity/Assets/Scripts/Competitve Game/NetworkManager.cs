using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
public class NetworkManager : MonoBehaviour
{
    #region Network helpers
    [SerializeField]
    private CompetitiveGameManager gameManager;
    private string[] empty = { "", "", "", "", "", "", "" };
    private enum Code : int
    {
        namesCode, firstMoveCode, realMoveCode,
        aiMoveCode, passTurnCode, newRoomCode,
        joinRoomCode, activateGameCode, leaveGameCode, queryWaitingRoomCode, closeRoom
    };

    #region Response and Data out literals
    private string[] namesFromUnity = { "p1", "p2", "p3", "p4" };
    private string[] namesRespBack = { "game_id" };
    private string[] firstFromUnity = { "piece", "perm" };
    private string[] firstRespBack = { "d" };
    private string[] realFromUnity = { "p_num", "piece", "perm", "x_coor", "y_coor" };
    private string[] realRespBack = { "number that indicates whether the move was legal", "number of squares closed" };
    private string[] aiFromUnity = { "p_num" };
    private string[] aiRespBack = { "shape num", "permutation", "x position", "y position", "number of squares closed" };
    private string[] newRoomFromUnity = { "rn", "p1" };
    private string[] newRoomRespBack = { "room_id", "room_name" };
    private string[] joinRoomFromUnity = { "rn", "pn" };
    private string[] joinRoomRespBack = { "player_code" };
    private string[] leaveRoomFromUnity = { "rn", "pn", "pc" };
    private string[] leaveRoomRespBack = { "state" };
    private string[] activateRoomFromUnity = { "rn", "r_id" };
    private string[] activateRoomRespBack = { "game_id" };
    #endregion
    private bool wantsToCreateGame = GameValues.wantsToCreateGame;
    private bool wantsOnlineGame = GameValues.wantsOnlineGame;

    public string errorFromServer;

    #region Response structs - translated into, from JSON.
    private struct namesRespBackStr { public int Result; public string Desc; public string game_id; }
    private struct firstRespBackStr { public int Result; public string Desc; public string some; }
    private struct realRespBackStr { public string Move; public int Result; public string Desc; public int number_that_indicates_whether_the_move_was_legal; public int number_of_squares_closed; };
    private struct aiRespBackStr { public int Result; public string Desc; public int shape_num; public int permutation; public int x_position; public int y_position; public int number_of_squares_closed; };
    private struct moveUpdateStr { public string Move; public string Player; public int Result; public string Desc; public int Piece; public int Perm; public int x_coor; public int y_coor; public int number_that_indicates_whether_the_move_was_legal; public int number_of_squares_closed; };
    private struct newRoomRespBackStr { public int Result; public string Desc; public string room_id; public string room_name; public string player_code; }
    private struct joinWaitingStr { public int Result; public string Desc; public string p1; public string p2; public string p3; public string p4; };
    private struct joinRoomRespBackStr { public int Result; public string Desc; public string player_code; }
    private struct activateGameRespBackStr { public string Move; public string p1; public string p2; public string p3; public string p4; public string Result; public string Desc; public string game_id; };
    private struct genericAnswerStr { public int Result; public string Desc; public string Move; };
    private struct socketIoMsgStr { public string rn; public string pn; public string pc; };
    private string[] getPlayerArrayActivate(activateGameRespBackStr resp)
    {
        return new string[] { resp.p1, resp.p2, resp.p3, resp.p4 };
    }
    private string[] getPlayerArrayJoin(joinWaitingStr resp)
    {
        return new string[] { resp.p1, resp.p2, resp.p3, resp.p4 };
    }

    #endregion

    [SerializeField]
    private string[] _dataOut;
    [SerializeField]
    private int[] _dataIn;
    [SerializeField]
    private PlayerClass[] players;
    public bool isAdmin = false;

    #endregion

    #region Server addressing
    private string _url = "http://132.69.8.19";
    private string _port = "80";
    private string _server_http_addr = "";
    private HttpClient _client;
    private SocketIOUnity _listenerSocket;
    #endregion

    #region Room identifiers and variables
    private string _roomId = "";
    public string roomName = "";
    private string _playerCode = "";
    public string playerName = "";
    #endregion

    [SerializeField]
    private string _currentGameId = "";


    // Start is called before the first frame update
    void Start()
    {
        _server_http_addr = this._url + ":" + this._port + "/";

        _client = new HttpClient();
        initializeListenSocket();
        onListenerJoinRequest();
        onListenerMoveUpdate();

        _listenerSocket.Connect();
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Generic network functions
    private async Task sendMessageByCode(Code code)
    {
        var dataout = constructDataOut(code);
        var response = await _client.GetAsync(_server_http_addr + dataout);
        string result = await response.Content.ReadAsStringAsync();
        //Debug.Log(result);
        parseReply(result, code);
    }

    private string constructDataOut(Code code = 0)
    {
        string[] addition = { };
        string str = "Data";
        string strOut = "";

        switch (code)
        {
            case Code.namesCode:
                addition = namesFromUnity;
                strOut = "start_new_game?";
                str = "Names";
                break;
            case Code.firstMoveCode:
                addition = firstFromUnity;
                if (wantsOnlineGame)
                {
                    strOut = "first_move_multi?gid=" + _currentGameId + "&pn=" +
                     this.playerName + "&pc=" + this._playerCode + "&";
                }
                else
                {
                    strOut = "first_move?gid=" + _currentGameId + "&";
                }
                str = "First move";
                break;
            case Code.realMoveCode:
                addition = realFromUnity;
                if (wantsOnlineGame)
                {
                    strOut = "reg_move_multi?gid=" + _currentGameId + "&pn=" +
                     this.playerName + "&pc=" + this._playerCode + "&";
                }
                else
                {
                    strOut = "reg_move?gid=" + _currentGameId + "&";
                }
                str = "Move";
                break;
            case Code.aiMoveCode:
                addition = aiFromUnity;
                str = "Ai move request sent to server.";
                Debug.Log(str);
                return "ai_move?gid=" + _currentGameId;
            case Code.passTurnCode:
                addition = empty;
                str = "Skip turn";
                Debug.Log(str);
                if (wantsOnlineGame)
                {
                    return "pass_turn_multi?gid=" + _currentGameId + "&pn=" +
                     this.playerName + "&pc=" + this._playerCode + "&";
                }
                else
                {
                    return "pass_turn?gid=" + _currentGameId + "&p_num=" + this._dataOut[0];
                }
            case Code.newRoomCode:
                addition = newRoomFromUnity;
                str = "New room!";
                strOut = "create_waiting_room?";
                break;
            case Code.joinRoomCode:
                addition = joinRoomFromUnity;
                str = "Join room";
                strOut = "join_waiting_room?";
                break;
            case Code.queryWaitingRoomCode:
                return "query_waiting_room?rn=" + this.roomName;
            case Code.activateGameCode:
                addition = activateRoomFromUnity;
                str = "Activate room";
                strOut = "activate_game?";
                break;
            case Code.leaveGameCode:
                addition = leaveRoomFromUnity;
                str = "leave room";
                return "leave_room?rn=" + this.roomName + "&pn=" + this.playerName + "&pc=" + this._playerCode;
            case Code.closeRoom:
                return "close_room?rn=" + this.roomName + "&r_id=" + this._roomId;
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

    private void parseReply(string result, Code code = 0)
    {
        string[] addition = { };
        string str = "Data";
        _dataIn = new int[5];

        var checkErrors = JsonUtility.FromJson<genericAnswerStr>(result);

        if (checkErrors.Result != 0)
        {
            _dataIn[0] = Int16.MinValue;
            errorFromServer = checkErrors.Desc;
            return;
        }

        switch (code)
        {
            case Code.namesCode:
                addition = namesRespBack;
                str = "Names";
                namesRespBackStr nameParesd = JsonUtility.FromJson<namesRespBackStr>(result);
                _dataIn[0] = int.Parse(nameParesd.game_id.Substring(0, 3));
                this._currentGameId = nameParesd.game_id;
                break;
            case Code.firstMoveCode:
                addition = firstRespBack;
                str = "First Move";
                firstRespBackStr firstParsed = JsonUtility.FromJson<firstRespBackStr>(result);
                _dataIn[0] = 1;
                break;
            case Code.realMoveCode:
                addition = realRespBack;
                str = "Move";
                realRespBackStr moveParsed = JsonUtility.FromJson<realRespBackStr>(result);
                _dataIn[0] = moveParsed.number_that_indicates_whether_the_move_was_legal;
                _dataIn[1] = moveParsed.number_of_squares_closed;
                break;
            case Code.aiMoveCode:
                addition = aiRespBack;
                str = "Ai move request";
                aiRespBackStr aiMove = JsonUtility.FromJson<aiRespBackStr>(result);
                _dataIn[0] = aiMove.shape_num;
                _dataIn[1] = aiMove.permutation;
                _dataIn[2] = aiMove.x_position;
                _dataIn[3] = aiMove.y_position;
                _dataIn[4] = aiMove.number_of_squares_closed;
                break;
            case Code.passTurnCode:
                addition = empty;
                str = "Skip turn";
                break;
            case Code.newRoomCode:
                addition = newRoomRespBack;
                str = "New room created";
                newRoomRespBackStr newrRoomParesd = JsonUtility.FromJson<newRoomRespBackStr>(result);
                _dataIn[0] = int.Parse(newrRoomParesd.room_id.Substring(0, 3));
                _dataIn[1] = 0;
                this._roomId = newrRoomParesd.room_id;
                this.roomName = newrRoomParesd.room_name;
                this._playerCode = newrRoomParesd.player_code;
                break;
            case Code.joinRoomCode:
                addition = joinRoomRespBack;
                str = "Joined room!";
                joinRoomRespBackStr joinRoomParsed = JsonUtility.FromJson<joinRoomRespBackStr>(result);
                _dataIn[0] = int.Parse(joinRoomParsed.player_code.Substring(0, 3));
                this._playerCode = joinRoomParsed.player_code;
                break;
            case Code.queryWaitingRoomCode:
                addition = joinRoomRespBack;
                str = "Query waiting room room!";
                joinWaitingStr queryRoom = JsonUtility.FromJson<joinWaitingStr>(result);
                _dataIn[0] = queryRoom.Result;
                this.gameManager.multiPlayerCanvas.updateQuery(getPlayerArrayJoin(queryRoom));
                break;
            case Code.activateGameCode:
                addition = activateRoomRespBack;
                str = "Activated game!";
                activateGameRespBackStr activateParsed = JsonUtility.FromJson<activateGameRespBackStr>(result);
                _dataIn[0] = int.Parse(activateParsed.game_id.Substring(0, 3));
                if (activateParsed.Result == "0")
                {
                    isAdmin = true;
                    this._currentGameId = activateParsed.game_id;
                }
                break;
            case Code.leaveGameCode:
                addition = leaveRoomRespBack;
                str = "Left room";
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
    #endregion

    #region Network interface

    #region Multiplayer functions
    public async Task<int[]> msgCreateMultiplayerGameToServer(string room_name, string player_name)
    {
        try
        {
            this._dataOut = new string[2] { room_name, player_name };
            await sendMessageByCode(Code.newRoomCode);
            if (_dataIn[0] != Int16.MinValue)
            {
                this.playerName = player_name;
                await listenerJoinBroadcastGroup();
            }

            return _dataIn;
        }
        catch (Exception e)
        {
            print("An exception occourd in sending creat new multiplayer room to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgJoinMultiplayerGameToServer(string room_name, string player_name)
    {
        try
        {
            this._dataOut = new string[2] { room_name, player_name };
            await sendMessageByCode(Code.joinRoomCode);
            if (_dataIn[0] != Int16.MinValue)
            {
                this.playerName = player_name;
                this.roomName = room_name;
                await listenerJoinBroadcastGroup();
                //await this.msgQueryRoom();
            }

            return _dataIn;
        }
        catch (Exception e)
        {
            print("An exception occourd in sending join multiplayer room to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgActivateRoomToServer(string room_name)
    {
        try
        {
            this._dataOut = new string[2] { room_name, this._roomId };
            await sendMessageByCode(Code.activateGameCode);
            return _dataIn;
        }
        catch (Exception e)
        {
            print("An exception occourd in sending creat new multiplayer game to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgQueryRoom()
    {
        try
        {
            await sendMessageByCode(Code.queryWaitingRoomCode);
            return _dataIn;
        }
        catch (Exception e)
        {
            print("An exception occourd in sending creat new multiplayer game to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> leaveRoom()
    {
        try
        {
            await sendMessageByCode(Code.leaveGameCode);
            return _dataIn;
        }
        catch (Exception e)
        {
            print("An exception occourd in sending creat new multiplayer game to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }
    public async Task<int[]> closeRoom()
    {
        try
        {
            await sendMessageByCode(Code.closeRoom);
            return _dataIn;
        }
        catch (Exception e)
        {
            print("An exception occourd in sending creat new multiplayer game to server!\n The exception is:");
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
            await sendMessageByCode(Code.namesCode);
            return _dataIn;
        }
        catch (Exception e)
        {
            print("An exception occourd in sending names to server!\n The exception is:");
            print(e);
            return new int[1] { -1 };
        }
    }

    public async Task<int[]> msgGameStartToServer(int shapeNum, int permutation)
    {
        try
        {
            this._dataOut = new string[2] { shapeNum.ToString(), permutation.ToString() };
            await sendMessageByCode(Code.firstMoveCode);
            return _dataIn;
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
            await sendMessageByCode(Code.realMoveCode);
            return _dataIn;
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
            await sendMessageByCode(Code.aiMoveCode);
            return _dataIn;
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
            await sendMessageByCode(Code.passTurnCode);
            return _dataIn;
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
        if (this._currentGameId != "")
        {
            if (wantsOnlineGame)
            {
                try
                {
                    //Adding 1 to player num since here player nums are: 0-3, while in the backend they are 1-4
                    this._dataOut = new string[1] { this._currentGameId };
                    await sendMessageByCode(Code.leaveGameCode);
                }
                catch (Exception e)
                {
                    print("An exception occourd in sending end game request to server!\n The exception is:");
                    print(e);
                }
            }
            else
            {
                await _client.GetAsync(_server_http_addr + "end_game?gid=" + _currentGameId);
            }
        }
    }

    #region Server Listener

    private void initializeListenSocket()
    {
        _listenerSocket = new SocketIOUnity(_server_http_addr, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                    {
                        {"token", "UNITY" }
                    }
                ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        _listenerSocket.JsonSerializer = new NewtonsoftJsonSerializer();
        _listenerSocket.OnConnected += (sender, e) =>
        {
            Debug.Log("Listener conntected.");
            //await _listenerSocket.EmitAsync("hi", "socket.io");
        };
        _listenerSocket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };

    }

    private async Task listenerJoinBroadcastGroup()
    {
        try
        {
            var msg = new socketIoMsgStr();
            msg.rn = this.roomName; //Room name.
            msg.pn = this.playerName; //Player name.
            msg.pc = this._playerCode; //Unique player code.
            await _listenerSocket.EmitAsync("join_broadcast_group", JsonUtility.ToJson(msg));
        }
        catch (Exception e)
        {
            print("An exception occourd in joining broadcast!\n The exception is:");
            print(e);
        }

    }
    private bool iJustJoined = false;
    private void onListenerJoinRequest()
    {
        try
        {
            _listenerSocket.On("join_request_reply", response =>
        {
            string text = response.GetValue<string>();
            Debug.Log("Got resp on join req: " + text);
            genericAnswerStr responseStr = JsonConvert.DeserializeObject<genericAnswerStr>(text);
            if (responseStr.Result != 0)
            {
                UnityThread.executeInUpdate(() =>
                {
                    gameManager.multiPlayerCanvas.showNotification("An error has occourd in joining the room:\n" + responseStr.Desc);
                });
            }
            else
            {
                UnityThread.executeInUpdate(async () =>
                {
                    await this.msgQueryRoom();
                    gameManager.multiPlayerCanvas.showWaitingRoom();
                });
            }
        });
        }
        catch (Exception e)
        {
            print("An exception occourd in callback broadcast!\n The exception is:");
            print(e);
        }
    }

    private void onListenerMoveUpdate()
    {

        _listenerSocket.On("move_update", async response =>
        {
            try
            {
                string text = response.GetValue<string>();
                Debug.Log("Got update: " + text);
                genericAnswerStr responseStr = JsonConvert.DeserializeObject<genericAnswerStr>(text);
                if (text.Contains("Move"))
                {
                    if (responseStr.Move == "Player_Joined")
                    {
                        joinWaitingStr joinRoomStr = JsonConvert.DeserializeObject<joinWaitingStr>(text);
                        UnityThread.executeInUpdate(() =>
                        {
                            gameManager.multiPlayerCanvas.addPlayerToRoom(getPlayerArrayJoin(joinRoomStr));
                        });
                    }
                    else if (responseStr.Move == "Player_Kicked")
                    {
                        joinWaitingStr removedFromRoomStr = JsonConvert.DeserializeObject<joinWaitingStr>(text);
                        //DEAL WITH IT LATER IF WE HAVE TIME
                    }
                    else if (responseStr.Move == "Player_Left")
                    {
                        joinWaitingStr leaveRoomStr = JsonConvert.DeserializeObject<joinWaitingStr>(text);
                        UnityThread.executeInUpdate(async () =>
                        {
                            gameManager.multiPlayerCanvas.showNotification("A player has left the waiting room!");
                            await this.msgQueryRoom();
                        });
                    }
                    else if (responseStr.Move == "Room_Closed")
                    {
                        joinWaitingStr roomClosedStr = JsonConvert.DeserializeObject<joinWaitingStr>(text);
                        UnityThread.executeInUpdate(async () =>
                        {
                            gameManager.multiPlayerCanvas.showNotification("This waiting room was closed by the admin!");
                            await gameManager.multiPlayerCanvas.goBack();
                        });
                    }
                    else if (responseStr.Move == "Game_started")
                    {
                        activateGameRespBackStr activateGameStr = JsonConvert.DeserializeObject<activateGameRespBackStr>(text);
                        UnityThread.executeInUpdate(async () =>
                        {
                            this._currentGameId = activateGameStr.game_id;
                            this.gameManager.updatePlayersAtGameStart(getPlayerArrayActivate(activateGameStr));
                            await gameManager.activateGame(false);
                        });
                    }
                    else if (responseStr.Move == "Pass")
                    {
                        UnityThread.executeInUpdate(async () =>
                        {
                            await this.gameManager.shapesManager.switchTurn();
                        });

                    }
                    else if (responseStr.Move == "Ai_Move")
                    {
                        moveUpdateStr activateGameStr = JsonConvert.DeserializeObject<moveUpdateStr>(text);
                        if (gameManager.shapesManager.isFirstTurn)
                        {
                            UnityThread.executeInUpdate(async () =>
                            {
                                if (!this.gameManager.isAdmin())
                                {
                                    await gameManager.insertShapeMoveUpdateAction(
                                            activateGameStr.Player, activateGameStr.Piece,
                                            activateGameStr.Perm, 15, 15,
                                            activateGameStr.number_that_indicates_whether_the_move_was_legal,
                                            activateGameStr.number_of_squares_closed, true);

                                    await gameManager.shapesManager.endFirstMove();
                                }
                            });

                        }
                        else
                        {
                            if (!this.gameManager.isAdmin())
                            {
                                UnityThread.executeInUpdate(async () =>
                           {
                               await gameManager.insertShapeMoveUpdateAction(
                                       activateGameStr.Player, activateGameStr.Piece,
                                       activateGameStr.Perm, activateGameStr.x_coor, activateGameStr.y_coor,
                                       activateGameStr.number_that_indicates_whether_the_move_was_legal,
                                       activateGameStr.number_of_squares_closed);
                           });
                            }
                        }
                    }
                }
                else
                {
                    if (!gameManager.players[this.gameManager.shapesManager.currentPlayer].isItMe)
                    {
                        moveUpdateStr activateGameStr = JsonConvert.DeserializeObject<moveUpdateStr>(text);
                        if (gameManager.shapesManager.isFirstTurn)
                        {
                            UnityThread.executeInUpdate(async () =>
                            {
                                await gameManager.insertShapeMoveUpdateAction(
                                    activateGameStr.Player, activateGameStr.Piece,
                                    activateGameStr.Perm, 15, 15,
                                    activateGameStr.number_that_indicates_whether_the_move_was_legal,
                                    activateGameStr.number_of_squares_closed, true);
                                await gameManager.shapesManager.endFirstMove();
                            });
                        }
                        else
                        {
                            UnityThread.executeInUpdate(async () =>
                            {
                                await gameManager.insertShapeMoveUpdateAction(
                                    activateGameStr.Player, activateGameStr.Piece,
                                    activateGameStr.Perm, activateGameStr.x_coor, activateGameStr.y_coor,
                                    activateGameStr.number_that_indicates_whether_the_move_was_legal,
                                    activateGameStr.number_of_squares_closed);
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                print("An exception occourd in callback move!\n The exception is:");
                print(e);
            }
        }
        );
    }
    #endregion

    #endregion
}