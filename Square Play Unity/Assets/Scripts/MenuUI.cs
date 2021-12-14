using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    public void playCompetitveGame()
    {
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
}
