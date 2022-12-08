
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;


public class CubeRayTest : MonoBehaviour
{
    public Transform target;
    private List<Vector3Int> coordList = new List<Vector3Int>();
    public Mesh mesh;
    public Material material;
    public Transform test;


    private Vector3[] ways = new Vector3[3];
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            //CoordCheck.VoxelRayCast(Vector3Int.RoundToInt(transform.position),Vector3Int.RoundToInt(target.position),out coordList);
            //foreach(var coord in coordList)
            //{
            //    GameObject go = new GameObject();
            //    go.transform.position = coord;
            //    go.transform.AddComponent<MeshFilter>().sharedMesh = mesh;
            //    go.transform.AddComponent<MeshRenderer>().sharedMaterial = material;
            //}


        }
    }



}
