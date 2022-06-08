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
    public GameObject preCanvas;
    public Button playGameButton;
    public Button pregameBackButton;

    public GameObject gameIdInput;

    private bool enteredGid = false;

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
        if (enteredGid)
        {
            //TODO: call a function from MANAGER that will tell the server that a new player connected to the specified game, and will get in return the players number.
            gameCanvas.SetActive(true);
            preCanvas.SetActive(false);
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

