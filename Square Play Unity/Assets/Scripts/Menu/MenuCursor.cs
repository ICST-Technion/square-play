using UnityEngine;

public class MenuCursor : MonoBehaviour
{
    public Texture2D cursorTexture;

    void Start()
    {
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }
    void Update()
    {
        detectWhosTarget();
    }

    void detectWhosTarget()
    {
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
}
