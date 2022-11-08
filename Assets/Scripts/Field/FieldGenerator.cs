using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;



public enum CUBE_TYPE { Ground, Air,Hill }

public struct Cube
{
    public Vector3 coord;
    public CUBE_TYPE type;

    public Cube(Vector3 _coord,CUBE_TYPE _type)
    {
        coord = _coord;
        type = _type;

    }
}



public class FieldGenerator : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private int z;
    public FieldInfo field;
    public Cube[,,] cubes;
    private Vector3[] forCheck = new Vector3[]
    {
        new Vector3(0,0,1),
        new Vector3(0,0,-1),
        new Vector3(0,1,0),
        new Vector3(0,-1,0),
        new Vector3(1,0,0),
        new Vector3(-1,0,0)
    };



    public void Start()
    {
        GenerateField();
    }


    public void GenerateField()
    {
        GenerateField(field);
    }


    public void GenerateField(FieldInfo fieldInfo)
    {
        cubes = new Cube[x, y, z];

        for(int i = 0;i<x;i++)
        {
            for(int j = 0;j<y;j++)
            {
                for(int k = 0;k<z;k++)
                {
                    CUBE_TYPE generateType = CUBE_TYPE.Ground;
                    if(j>2)
                    {
                        generateType = CUBE_TYPE.Air;
                    }                   
                    cubes[i, j, k] = new Cube(new Vector3(i, j, k), generateType);
                }
            }
        }

        //TEST
        for(int i = 0;i<x;i++)
        {
            for(int j = 0; j<z;j++)
            {
                if(Random.Range(0,100)>90)
                {
                    cubes[i, 3, j].type = CUBE_TYPE.Ground;
                }
                   
            }
        }

        foreach(Cube cube in cubes)
        {
            if (cube.type == CUBE_TYPE.Air) return;

  
            foreach(Vector3 check in forCheck)
            {
                if(!CreateCheck(cube.coord+check))
                {
                    Addressables.InstantiateAsync(fieldInfo.fieldSuburb, transform.position + cube.coord, Quaternion.identity, transform);
                    break;
                }
            }
        }
    }


    internal bool CreateCheck(Vector3 position)
    {
        if (position.x < 0 || position.y < 0 || position.z < 0 || position.x >= x || position.y >= y || position.z >= z)
        {
            return false;
        }
        return cubes[(int)position.x, (int)position.y, (int)position.z].type == CUBE_TYPE.Ground;
    }

}
