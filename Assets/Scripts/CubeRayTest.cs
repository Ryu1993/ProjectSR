using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRayTest : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            print(CubeCheck.CubeRay(transform.position.ToInt(), target.position.ToInt(), CUBE_TYPE.Obstacle));


    }


}
