using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class gameCanvasScript : MonoBehaviour
{
    public Button competitveAgainButton;
    public Button competitveBackButton;
    public CompetitiveGameManager manager;
    // Start is called before the first frame update
    void Start()
    {
        competitveAgainButton.onClick.AddListener(async () => await playAgain());
        competitveBackButton.onClick.AddListener(async () => await goBack());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public async Task playAgain()
    {
        print("again");
        await manager.msgNamesToServer();
        await manager.startGame();
    }

    public async Task goBack()
    {
        await manager.byeBye();
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
