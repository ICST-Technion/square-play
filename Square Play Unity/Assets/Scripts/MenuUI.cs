using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public void playCompetitveGame()
    {
        print("h");
        SceneManager.LoadScene(1);
    }
    public void playShapesGame()
    {
        SceneManager.LoadScene(2);
    }
    public void openSettings()
    {
        SceneManager.LoadScene(3);
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
