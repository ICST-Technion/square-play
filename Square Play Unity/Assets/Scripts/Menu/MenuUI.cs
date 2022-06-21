using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    static bool wantsOnlineGame = false;
    public GameObject onlineMultiplayerCanvas;

    public GameObject mainCanvas;

    void Update()
    {
    }

    public void playLocalCompetitveGame()
    {
        SceneManager.LoadScene(1);
    }
    public void playOnlineMultiplayerCompetitveGame()
    {
        wantsOnlineGame = true;
        onlineMultiplayerCanvas.SetActive(true);
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
