using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class competitveGameCanvasScript : MonoBehaviour
{
    public CompetitiveGameManager manager;
    public GameObject gameCanvas;
    public GameObject preCanvas;
    public List<GameObject> playerNamesText;

    public List<GameObject> namesInput;

    public GameObject numberOfPlayers;

    private bool choseNumAuto = false;

    public GameObject notification;

    // Start is called before the first frame update
    void Start()
    {
        /*Disabled for tests only
        gameCanvas.SetActive(false);
        preCanvas.SetActive(true);*/
        /*Test*/
        gameCanvas.SetActive(true);
        preCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void playAgain()
    {
        SceneManager.LoadScene(1);
    }

    public void goBack()
    {
        SceneManager.LoadScene(0);
    }

    private void updateNames()
    {
        for (int i = 0; i < 4; i++)
        {
            this.playerNamesText[i].GetComponent<TextMeshProUGUI>().text = manager.players[i].playerName + ":";
        }
    }

    public void startGame()
    {
        if (choseNumAuto)
        {
            manager.msgNamesToServer();
            this.updateNames();
            gameCanvas.SetActive(true);
            preCanvas.SetActive(false);
            manager.startGame();
        }
        else
        {
            showNotification("You must insert the number of auto players in order to play!");
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

    public void numOfPlayers(string aiNumber)
    {
        int num = -1;
        int.TryParse(aiNumber, out num);
        if (num == -1)
        {
            showNotification("Please enter only numbers for number of auto players!");
            return;
        }
        foreach (var player in manager.players)
        {
            player.isAi = false;
            player.playerName = "";
        }
        manager.randomizePlayerNames();
        if (num > 4)
        {
            num = 4;
            numberOfPlayers.GetComponent<TMP_InputField>().text = num.ToString();
        }
        for (int i = num; i < 4; i++)
        {
            this.namesInput[i].GetComponent<TMP_InputField>().text = manager.players[i].playerName;
            manager.players[i].isAi = true;
        }
        choseNumAuto = true;
    }

    public void name1(string name1)
    {
        manager.players[0].playerName = name1;
    }
    public void name2(string name2)
    {
        manager.players[1].playerName = name2;
    }
    public void name3(string name3)
    {
        manager.players[2].playerName = name3;
    }
    public void name4(string name4)
    {
        manager.players[3].playerName = name4;
    }
}

