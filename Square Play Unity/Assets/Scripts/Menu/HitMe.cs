using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitMe : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
     {
     }

      public void OnMouseDown()
    {
        print(this.name);
    }

    public void OnMouseUp()
    {
        print(this.name);
    }

    public void OnMouseOver()
    {
        print(this.name);
    }

    public void OnMouseDrag()
    {
        print(this.name);
    }
}
