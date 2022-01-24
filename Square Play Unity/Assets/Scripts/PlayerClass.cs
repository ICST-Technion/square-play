using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClass : MonoBehaviour
{
    public List<BaseShape> playerShapes = new List<BaseShape>();

    public int playerNum = 0;

    public string playerName = "";

    public bool isAi = true;
    //The player class is actually atached to the players bank.

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
