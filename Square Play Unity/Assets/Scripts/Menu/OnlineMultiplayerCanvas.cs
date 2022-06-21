using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineMultiplayerCanvas : MonoBehaviour
{
    static bool wantsToCreateGame = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
     
   void Update()
     {
       //detectWhosTarget();
     }

     void detectWhosTarget(){
         if (Input.GetMouseButtonDown(0))
         {
             Debug.Log("Mouse down");
             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
             RaycastHit2D raycastHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)), Vector2.zero);
             if (raycastHit)
             {
                 Debug.Log("Hit: " + raycastHit.transform.gameObject.name);
             }
 
             Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
             RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
             if (hit.collider != null)
             {
                 Debug.Log(hit.collider.name);
             }
 
             RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);
             Vector2 v = Camera.main.ScreenToWorldPoint(Input.mousePosition);
 
             Collider2D[] c = Physics2D.OverlapPointAll(v);
             if (c.Length > 0)
             {
                 Debug.Log("Hit!!!");
             }
 
             RaycastHit2D[] allHits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
             if (allHits.Length > 0)
             {
                 Debug.Log("Hit!!!");
             }
         }
     }
    public void createOnlineGame(){
        SceneManager.LoadScene(1);
    }

    public void joinOnlineGame(){
        //wantsToCreateGame = false;
        SceneManager.LoadScene(1);
    }
}
