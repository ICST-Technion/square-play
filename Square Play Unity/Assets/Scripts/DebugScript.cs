using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DebugScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public void hey()
    {
        Debug.Log("Hey");
    }

    // Update is called once per frame
  /*  public void Update()
    {
        if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
        {
            //--score;
            Debug.Log("Hit: " + EventSystem.current.transform.position.ToString());
        }
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            //--score;
            Debug.Log("Hit: " + EventSystem.current.transform.position.ToString());
        }
    }*/

}
