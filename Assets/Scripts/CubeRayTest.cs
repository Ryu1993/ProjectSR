using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

public class CubeRayTest : MonoBehaviour
{
    public Transform target;
    public GameObject go;
    public LineRenderer line;
    Vector3[] positions = new Vector3[2];
    public CUBE_TYPE[] mask;
    public LayerMask lmask;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Vector3Int origin = transform.position.ToInt();
            Vector3Int target = transform.forward.ToInt() * 10;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for(int i =0;i<1000;i++)
            {
                CubeCheck.CubeRayCast(origin,target);
            }
            sw.Stop();
            print("cube : "+sw.ElapsedMilliseconds + "ms");

            Stopwatch tsw = new Stopwatch();
            sw.Start();
            for(int i =0; i<1000;i++)
            {
                Physics.Raycast(transform.position, transform.forward, 10, lmask,QueryTriggerInteraction.Collide);
            }
            sw.Stop();
            print("ray : " + tsw.ElapsedMilliseconds + "ms");

        }
            


    }



}
