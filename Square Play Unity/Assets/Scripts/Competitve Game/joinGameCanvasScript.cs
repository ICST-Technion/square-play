using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class joinGameCanvasScript : MonoBehaviour
{
    public CompetitiveGameManager manager;
    public GameObject gameCanvas;
    public GameObject joinCanvas;
    public Button playGameButton;
    public Button pregameBackButton;
    private string enteringPlayersName;
    private string joiningGameId;
    static bool wantsToCreateGame;
    public GameObject notification;

    // Start is called before the first frame update
    void Start()
    {
        gameCanvas.SetActive(false);
        joinCanvas.SetActive(true);

        if (wantsToCreateGame)
        {
            playGameButton.onClick.AddListener(async () => await startNewGame());
        }
        else
        {
            playGameButton.onClick.AddListener(async () => await joinGame());
        }
        
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

    public void gameId(string game_id)
    {
        this.joiningGameId = game_id;
    }


    public void playersName(string name)
    {
       this.enteringPlayersName = name;
    }


    public async Task startNewGame()
    {
        if (enteringPlayersName!="")
        {
            await manager.msgNewMultiplayerGameToServer(enteringPlayersName);
            gameCanvas.SetActive(true);
            joinCanvas.SetActive(false);
            //TODO: call a function from MANAGER that will display only the players that are currently in the game, and if the player that just joined is the last - start the game.
        }
        else
        {
            showNotification("You must insert the game ID in order to play!");
        }
    }

    public async Task joinGame()
    {
        if (enteringPlayersName != ""&&joiningGameId!="")
        {
            await manager.msgJoinMultiplayerGameToServer(joiningGameId,enteringPlayersName);
            gameCanvas.SetActive(true);
            joinCanvas.SetActive(false);
            //TODO: call a function from MANAGER that will display only the players that are currently in the game, and if the player that just joined is the last - start the game.
        }
        else
        {
            showNotification("You must insert the game ID in order to play!");
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
}

