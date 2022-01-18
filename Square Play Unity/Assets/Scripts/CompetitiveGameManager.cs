using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Net.Sockets;
using System;


public class CompetitiveGameManager : MonoBehaviour
{
    public BoardClass board;
	public ShapesManager shapesManager;
    public GameObject notification;
    public competitveGameCanvasScript canvas;

    [HideInInspector]
    public int scaleFactor = 33;
    private int cols = 33;
    private int rows = 33;
    
    public string[] playernames=new string[4];
    
    // Use this for initialization
    void Start()
    {
        this.setupSocket();

        this.randomizePlayerNames();

        board.generate(cols, rows); //give that board instance access to the python comm functions, via the socket interface

        //shapesManager.temporarySetup(board,this);//TBD: change to setup
        shapesManager.Setup(this.board,this);
    }

    public void randomizePlayerNames(){
        string[] possibleNames={"Jonatan","Mai","Hadi","Daniel","Ron","Elazar","Moshe"};
        List<int> usedIndexes=new List<int>();
        System.Random rand =new System.Random();
        for (int i = 0; i < 4; i++)
        {
            var idx = rand.Next(possibleNames.Length);
            while(usedIndexes.Contains(idx)){
                idx = rand.Next(possibleNames.Length);
            }
            usedIndexes.Add(idx);
            this.playernames[i]=possibleNames[idx];
        }
    }

    private IEnumerator announceNotification(string notificationMsg){
        print("the notification to be shown:"+notificationMsg);
        this.notification.gameObject.SetActive(true);
        this.notification.GetComponent<TextMeshProUGUI>().text=notificationMsg;
        yield return new WaitForSeconds(5);
        this.notification.gameObject.SetActive(false);
        this.notification.GetComponent<TextMeshProUGUI>().text="";
    }

    public void showNotification(string msg){
       StartCoroutine(this.announceNotification(msg));
    }
    public void startGame(){
        this.showNotification(playernames[3]+", choose the piece you'd like to put in the middle of the board");
        this.shapesManager.startGame();
    }

    public void playAgain()
    {
        SceneManager.LoadScene(1);
    }

    public void goBack()
    {
        SceneManager.LoadScene(0);
    }

    #region python communcation
    public string ip = "127.0.0.1";
    public int port = 60000;
    private Socket client;
    [SerializeField]
    private int[] dataOut, dataIn;

    private void setupSocket()
    {
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(ip, port);
        if (!client.Connected)
        {
            //exit game!
            Debug.LogError("Connection Failed");
        }
        Debug.Log("Connected");
    }

    private void endSocket()
    {
        client.Close();
    }
    

    private void printDataIn(){
         print("Received from logic: ");
        for (int i = 0; i < this.dataIn.Length; i++)
        {
            Debug.Log(this.dataIn[i]);
        }
    }

    private void printDataOut(){
         print("Data sent to logic: ");
        for (int i = 0; i < this.dataOut.Length; i++)
        {
            Debug.Log(this.dataOut[i]);
        }
    }

    private void sendActionCode(int code,bool setupSock=true){
        if(setupSock){
            this.setupSocket();
        }
        //convert floats to bytes, send to port
        var action_code =  new int[1]{code};
        var byteArray = new byte[action_code.Length * 4];
        Buffer.BlockCopy(action_code, 0, byteArray, 0, byteArray.Length);
        client.Send(byteArray);
    }

    private int[] receiveResponse(){
        //allocate and receive bytes
        byte[] bytes = new byte[4000];
        int idxUsedBytes = client.Receive(bytes);

        //convert bytes to floats
        int[] response = new int[idxUsedBytes / 4];
        Buffer.BlockCopy(bytes, 0, response, 0, idxUsedBytes);
        this.endSocket();
        return response;
    }

    public int msgMoveToServer(int playerNum,int pieceNum, int permutation,int new_position_x,int new_position_y)
    {   
        try
        {
        this.dataOut = new int[5]{playerNum, pieceNum, permutation, new_position_x,new_position_y};
        this.dataIn = sendMoveMsg();
        this.printDataIn();
        return this.dataIn[0];
        } catch (Exception e){
            Debug.Log("An exception occourd in sending move to server!\n The exception is:");
            Debug.Log(e);
            this.endSocket();
            return -1;
        }
    }
    private int[] sendMoveMsg()
    {
        //send the action code.
        sendActionCode(2);
        this.printDataOut();

        //setup the actual data and send it.
        var byteArray = new byte[this.dataOut.Length * 4];
        Buffer.BlockCopy(this.dataOut, 0, byteArray, 0, byteArray.Length);
        client.Send(byteArray);

        //get rsponse from logic.
        return this.receiveResponse();
    }

    public int msgGameStartToServer(int shapeNum, int permutation)
    {   
        try
        {
        this.dataOut = new int[2]{shapeNum, permutation};
        this.dataIn = sendGameStartMsg();
        this.printDataIn();
        return this.dataIn[0];
        } catch (Exception e){
            Debug.Log("An exception occourd in sending move to server!\n The exception is:");
            Debug.Log(e);
            this.endSocket();
            return -1;
        }
    }
    private int[] sendGameStartMsg()
    {
        //send the action code.
        sendActionCode(1);
        this.printDataOut();

        //setup the actual data and send it.
        var byteArray = new byte[this.dataOut.Length * 4];
        Buffer.BlockCopy(this.dataOut, 0, byteArray, 0, byteArray.Length);
        client.Send(byteArray);

        //get rsponse from logic.
        return this.receiveResponse();
    }

    public int msgNamesToServer()
    {
        try{
        this.dataIn = sendNamesMsg();
        this.printDataIn();
        return this.dataIn[0];
        }  catch (Exception e){
            Debug.Log("An exception occourd in sending names to server!\n The exception is:");
            Debug.Log(e);
            this.endSocket();
            return -1;
        }
    }


    private int[] sendNamesMsg()
    {
        sendActionCode(0,false);
        this.printDataOut();

        string concatenatedNames="";
        for (int i = 0; i < this.playernames.Length; i++)
        {
            string stringToInsert = this.playernames[i]+",";
            if(i==this.playernames.Length-1){
                stringToInsert= this.playernames[i];
            }
            concatenatedNames+=stringToInsert;
        }

        var byteArray =new byte[concatenatedNames.Length];
        for (int i = 0; i < concatenatedNames.Length; i++) { 
            byteArray[i] = Convert.ToByte(concatenatedNames[i]); 
        } 
        client.Send(byteArray);
        
        return this.receiveResponse();
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
