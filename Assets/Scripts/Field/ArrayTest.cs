using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ArrayTest : MonoBehaviour
{

    public int CompareY(Vector3Int x, Vector3Int y)
    {
        if (x.y < y.y) return -1;
        if (x.y == y.y) return 0;
        if (x.y > y.y) return 1;
        return 0;
    }
    Vector3Int[] testArray = new Vector3Int[3];
    List<Vector3Int> testList = new List<Vector3Int>();


    private void Start()
    {
        
        testList.Sort(CompareY);



    }



}
