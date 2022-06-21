using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class startGameCanvasScript : MonoBehaviour
{
    public CompetitiveGameManager manager;
    public GameObject gameCanvas;
    public GameObject preCanvas;
    public Button playGameButton;
    public Button pregameBackButton;

    public List<GameObject> namesInput;

    public GameObject numberOfPlayers;

    private bool choseNumAuto = false;

    public GameObject notification;

    // Start is called before the first frame update
    void Start()
    {
        gameCanvas.SetActive(false);
        preCanvas.SetActive(true);

        playGameButton.onClick.AddListener(async () => await startGame());
        pregameBackButton.onClick.AddListener(async () => await goBack());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async Task goBack()
    {
        await manager.byeBye();
        SceneManager.LoadScene(0);
    }

    

    

    public async Task startGame()
    {
        if (choseNumAuto)
        {
            await manager.msgNamesToServer();
            manager.updatePlayerNames();
            gameCanvas.SetActive(true);
            preCanvas.SetActive(false);
            await manager.startGame();
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

