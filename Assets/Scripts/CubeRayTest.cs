using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CubeRayTest : MonoBehaviour
{
    public Transform target;
    public GameObject go;
    public LineRenderer line;
    Vector3[] positions = new Vector3[2];
    public CUBE_TYPE[] mask;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            CubeCheck.CubeRay(transform.position.ToInt(), target.transform.position.ToInt());
            positions[0] = transform.position;
            positions[1] = target.position;
            line.SetPositions(positions);
        }
            


    }



}
