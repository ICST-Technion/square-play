using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerClass : MonoBehaviour
{
    public List<BaseShape> playerShapes = new List<BaseShape>();

    public int playerNum = 0;

    public string playerName = "";
    public string playerCode = "";

    public bool isAi = true;

    public bool isItMe = false; //This will tell us whats the player object of the amer who playes this instance of a multiplayer game. 

    public GameObject playerNameTextObj;
    //The player class is actually atached to the players bank.

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void updateName()
    {
        this.playerNameTextObj.GetComponent<TextMeshProUGUI>().text = this.playerName;
    }

    public void thisIsMe(string my_name)
    {
        this.playerName = my_name;
        updateName();
        this.isItMe = true;
    }

    public void turnMeOff()
    {
        foreach (var piece in this.playerShapes)
        {
            piece.gameObject.SetActive(false);
        }
    }

    public void turnMeOn()
    {
        foreach (var piece in this.playerShapes)
        {
            piece.gameObject.SetActive(true);
        }
    }
}
