using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System.IO;
using System.Net.Sockets;
using System;


public class CompetitiveGameManager : MonoBehaviour
{
    public BoardClass board;
	public ShapesManager shapesManager;

    public competitveGameCanvasScript canvas;

    [HideInInspector]
    public int scaleFactor = 33;
    private int cols = 33;
    private int rows = 33;
    [HideInInspector]
    public string[] playernames=new string[4];
    
    // Use this for initialization
    void Start()
    {
        this.setupSocket();

        board.generate(cols, rows); //give that board instance access to the python comm functions, via the socket interface

        shapesManager.temporarySetup(board,this);
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
    private float[] dataOut, dataIn;

    private void setupSocket(){
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        client.Connect(ip, port);
        if (!client.Connected)
        {
            Debug.LogError("Connection Failed");
        }
        Debug.Log("Connected");
    }

    private void endSocket(){
        client.Close();
    }
    public bool msgMoveToServer(float[] dataOut)
    {
        Debug.Log("trying to send a msg to server");
        this.dataOut = dataOut;
        this.dataIn = sendMoveMsg(dataOut);
        Debug.Log("got data");
        if(this.dataIn.Equals((float)1)){
            return true;
        }
        return false;
    }

    private float[] sendMoveMsg(float[] dataOut)
    {
        float[] floatsReceived;

        //convert floats to bytes, send to port
        var action_code =  new float[1]{1};
        var byteArray = new byte[action_code.Length * 4];
        Buffer.BlockCopy(action_code, 0, byteArray, 0, byteArray.Length);
        client.Send(byteArray);
        byteArray = new byte[dataOut.Length * 4];
        Buffer.BlockCopy(dataOut, 0, byteArray, 0, byteArray.Length);
        client.Send(byteArray);
        Debug.Log("sent");
        //allocate and receive bytes
        byte[] bytes = new byte[4000];
        int idxUsedBytes = client.Receive(bytes);

        //convert bytes to floats
        floatsReceived = new float[idxUsedBytes / 4];
        Buffer.BlockCopy(bytes, 0, floatsReceived, 0, idxUsedBytes);

        return floatsReceived;
    }

    public bool msgNamesToServer()
    {
        Debug.Log("trying to send a msg to server");
        this.dataIn = sendNamesMsg(this.playernames);
        Debug.Log("got data");
        if(this.dataIn.Equals((float)1)){
            return true;
        }
        return false;
    }


    private float[] sendNamesMsg(string[] dataOut)
    {
        float[] floatsReceived;

        //convert floats to bytes, send to port
        var action_code =  new float[1]{0};
        var byteArray = new byte[action_code.Length * 4];
        Buffer.BlockCopy(action_code, 0, byteArray, 0, byteArray.Length);
        client.Send(byteArray);
        byteArray = new byte[dataOut.Length * 4];
        Buffer.BlockCopy(dataOut, 0, byteArray, 0, byteArray.Length);
        client.Send(byteArray);
        Debug.Log("sent");
        //allocate and receive bytes
        byte[] bytes = new byte[4000];
        int idxUsedBytes = client.Receive(bytes);

        //convert bytes to floats
        floatsReceived = new float[idxUsedBytes / 4];
        Buffer.BlockCopy(bytes, 0, floatsReceived, 0, idxUsedBytes);

        client.Close();
        return floatsReceived;
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
