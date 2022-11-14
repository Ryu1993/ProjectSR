using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MousetClick : MonoBehaviour
{
    Camera main;
    RaycastHit hit;
    public LayerMask mask;
    Ray ray;
    private void Awake()
    {
        main = Camera.main;
    }

    public void Update()
    {
        Click();
        
    }

    private void Click()
    {
        if(Input.GetMouseButtonDown(0))
        {
            ray = main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                if (hit.transform.TryGetComponent(out AreaView view))
                    view.Move();
        }


    }



}
