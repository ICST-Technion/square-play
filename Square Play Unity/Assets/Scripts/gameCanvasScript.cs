using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class gameCanvasScript : MonoBehaviour
{

    public CompetitiveGameManager manager;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void playAgain()
    {
        manager.msgNamesToServer();
        manager.startGame();
    }

    public void goBack()
    {
        SceneManager.LoadScene(1);
    }

    public void choseToRotate()
    {
        manager.rotationMode = true;
    }
    public void chooseRotation(int a)
    {
        this.BroadcastMessage("setMyRotation", a);
    }
}
