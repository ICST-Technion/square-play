using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuUI : MonoBehaviour
{
    private bool wantsToCreateGame = GameValues.wantsToCreateGame;
    private bool wantsOnlineGame = GameValues.wantsOnlineGame;
    public GameObject onlineMultiplayerCanvas;

    public GameObject mainCanvas;

    void Update()
    {
    }

    public void playLocalCompetitveGame()
    {
        GameValues.wantsOnlineGame = false;
        GameValues.wantsToCreateGame = false;
        SceneManager.LoadScene(1);
    }
    public void playOnlineMultiplayerCompetitveGame()
    {
        GameValues.wantsOnlineGame = true;
        onlineMultiplayerCanvas.SetActive(true);
    }

    public void createGame()
    {
        GameValues.wantsToCreateGame = true;
        SceneManager.LoadScene(1);
    }

    public void joinGame()
    {
        GameValues.wantsToCreateGame = false;
        SceneManager.LoadScene(1);
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
        SceneManager.LoadScene(4);
    }
    public void viewRulesPage()
    {
        SceneManager.LoadScene(5);
    }
}
