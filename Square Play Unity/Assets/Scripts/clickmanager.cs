using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clickmanager : MonoBehaviour
{
    public delegate void MouseClick();
    public static event MouseClick OnMouseClick;

    public static Vector3 mousePosition;
    public static BaseShape selectedObject;

    Vector3 offset;

    void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (OnMouseClick != null)
            {
                OnMouseClick();

                if (selectedObject)
                {
                    offset = selectedObject.transform.position - mousePosition;
                }
            }
        }

        if (selectedObject)
        {
            selectedObject.transform.position = mousePosition + offset;
        }

        if (Input.GetMouseButtonUp(0) && selectedObject)
        {
            selectedObject = null;
        }
    }
}