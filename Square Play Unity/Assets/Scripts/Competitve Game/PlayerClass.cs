using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerClass : MonoBehaviour
{
    public List<BaseShape> playerShapes = new List<BaseShape>();

    public int playerNum = 0;

    public string playerName = "";

    public bool isAi = true;

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

    public void updateName(){
        this.playerNameTextObj.GetComponent<TextMeshProUGUI>().text = this.playerName;
    }
}
