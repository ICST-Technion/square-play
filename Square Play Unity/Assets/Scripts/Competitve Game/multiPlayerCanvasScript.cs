using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class multiPlayerCanvasScript : MonoBehaviour
{
    public CompetitiveGameManager manager;
    public gameCanvasScript gameCanvas;
    public multiPlayerCanvasScript joinCanvas;
    public GameObject waitingRoom;
    public GameObject mainJoin;
    public GameObject waitingRoomName;
    public GameObject[] joiningPlayers;
    public Button playGameButton;
    public Button pregameBackButton;
    public Button waitingRoomBackButton;
    [SerializeField]
    private string enteringPlayersName;
    [SerializeField]
    private string joiningRoomName;
    private bool wantsToCreateGame = GameValues.wantsToCreateGame;
    public GameObject notification;
    [SerializeField]
    private int currentlyInRoom = 1;
    public Button activateGameButton;
    public GameObject textToDisplay;

    // Start is called before the first frame update
    void Start()
    {
        if (wantsToCreateGame)
        {
            playGameButton.onClick.AddListener(async () => await startNewGame());
            activateGameButton.onClick.AddListener(async () => await activateNewGame());
            textToDisplay.GetComponent<TextMeshProUGUI>().text = "Please enter the name for your new room";
        }
        else
        {
            playGameButton.onClick.AddListener(async () => await joinGame());
            activateGameButton.gameObject.GetComponent<Button>().enabled = false;
            textToDisplay.GetComponent<TextMeshProUGUI>().text = "Please enter the name of the room you'd like to join";
        }
        waitingRoom.gameObject.SetActive(false);
        mainJoin.gameObject.SetActive(true);
        pregameBackButton.onClick.AddListener(async () => await goBack());
        waitingRoomBackButton.onClick.AddListener(async () => await waitingRoomBack());
    }

    private bool timerSet = false;
    private DateTime countdownUp;
    public async Task activateNewGame()
    {
        await manager.netManager.msgQueryRoom();
        await manager.activateGame(true);
    }

    // Update is called once per frame
    void Update()
    {
        /*TimeSpan duration = DateTime.Now.Subtract(countdownUp);
        if (timerSet && duration >= TimeSpan.FromSeconds(30))
        {
            Debug.Log("Query!");
            manager.netManager.msgQueryRoom();
        }*/
    }

    public async Task goBack()
    {
        await manager.byeBye();
        SceneManager.LoadScene(0);
    }

    public async Task waitingRoomBack()
    {
        if (this.manager.isAdmin())
        {
            await manager.netManager.closeRoom();
        }
        else
        {
            await manager.netManager.leaveRoom();
        }
        SceneManager.LoadScene(1);
    }

    public void gameId(string game_id)
    {
        this.joiningRoomName = game_id;
    }


    public void playersName(string name)
    {
        this.enteringPlayersName = name;
    }


    public async Task startNewGame()
    {
        if (enteringPlayersName != "" && joiningRoomName != "")
        {
            var result = await manager.msgCreateMultiplayerGameToServer(joiningRoomName, enteringPlayersName);
            if (result[0] != Int16.MinValue)
            {
                this.currentlyInRoom++;
            }
            joiningPlayers[0].GetComponent<TextMeshProUGUI>().text = enteringPlayersName;
        }
        else
        {
            showNotification("You must insert the room name and your name in order to play!");
        }
    }

    public void showWaitingRoom()
    {
        try
        {
            waitingRoom.gameObject.SetActive(true);
            mainJoin.gameObject.SetActive(false);
            waitingRoomName.GetComponent<TextMeshProUGUI>().text = "Waiting room: " + this.manager.netManager.roomName;
        }
        catch (Exception e)
        {
            print(e);
        }
    }

    public void addPlayerToRoom(string[] names)
    {
        //When there is 1 person in the room, currentlyInRoom=1, thus the new joining player would have index =1 in the players array.
        //When there are 2 people in the room, currentlyInRoom=2, thus the new joining player would have index =2 in the players array.
        var new_idx = currentlyInRoom;
        var now_joined = names[new_idx];
        if (!this.manager.isPlayerAlreadyInRoom(now_joined))
        {
            manager.players[new_idx].name = now_joined;
            //manager.players[new_idx].playerNum = new_idx;
            joiningPlayers[new_idx].GetComponent<TextMeshProUGUI>().text = now_joined;
            joiningPlayers[new_idx].SetActive(true);
            currentlyInRoom++;
        }
    }

    public void updateQuery(string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            manager.players[i].name = names[i];
            //manager.players[i].playerNum = i;
            if (!manager.players[i].name.Contains("AI"))
            {
                joiningPlayers[i].gameObject.SetActive(true);
                currentlyInRoom = i + 1;
            }
            joiningPlayers[i].GetComponent<TextMeshProUGUI>().text = manager.players[i].name;
        }
    }

    public async Task joinGame()
    {
        if (enteringPlayersName != "" && joiningRoomName != "")
        {
            await manager.msgJoinMultiplayerGameToServer(joiningRoomName, enteringPlayersName);
        }
        else
        {
            showNotification("You must insert the room name and your name in order to play!");
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
        print("notification!");
        StartCoroutine(this.announceNotification(msg));
    }
}

