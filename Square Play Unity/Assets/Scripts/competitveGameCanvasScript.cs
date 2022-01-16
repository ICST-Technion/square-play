using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class competitveGameCanvasScript : MonoBehaviour
{
    public CompetitiveGameManager manager;
    public GameObject gameCanvas;
    public GameObject preCanvas;
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
        SceneManager.LoadScene(1);
    }

	public void goBack()
    {
        SceneManager.LoadScene(0);
    }

    public void startGame()
    {
     gameCanvas.SetActive(true);
     preCanvas.SetActive(false);
    }

    public void name1(string name1){
        manager.playernames[0]=name1;
    }
    public void name2(string name2){
        manager.playernames[1]=name2;
    }
    public void name3(string name3){
       manager.playernames[2]=name3;
    }
    public void name4(string name4){
        manager.playernames[3]=name4;
    }
}
