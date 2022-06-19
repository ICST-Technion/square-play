using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public GameObject onlineMultiplayerCanvas;

    public GameObject mainCanvas;

    public void playLocalCompetitveGame()
    {
        SceneManager.LoadScene(1);
    }
    public void playOnlineMultiplayerCompetitveGame()
    {
        onlineMultiplayerCanvas.SetActive(true);
        mainCanvas.GetComponent<Canvas>().sortingLayerName="Default";
        mainCanvas.GetComponent<Canvas>().sortingOrder = 1;
    }
    public void playShapesGame()
    {
        //SceneManager.LoadScene(2);
    }
    public void openSettings()
    {
        //SceneManager.LoadScene(3);
    }
    public void viewAboutPage()
    {
        SceneManager.LoadScene(2);
    }
    public void viewRulesPage()
    {
        SceneManager.LoadScene(3);
    }
}
