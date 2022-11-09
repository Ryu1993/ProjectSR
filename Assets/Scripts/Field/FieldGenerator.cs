using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;



public enum CUBE_TYPE { Ground, Air,Hill,Bed,Null }

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
    private Vector3[] check = new Vector3[]
    {
        new Vector3(0,0,1),
        new Vector3(0,0,-1),
        new Vector3(1,0,0),
        new Vector3(-1,0,0),
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
        ForCube((x, y, z) =>
        {
            CUBE_TYPE creatType = CUBE_TYPE.Air;
            if (y == 0) creatType = CUBE_TYPE.Ground;
            cubes[x, y, z] = new Cube(new Vector3(x, y, z), creatType);
        }
        );
        FloorCreate(out Vector3[] oneFloorFirst, (x * z) / 4, new Vector3(UnityEngine.Random.Range(0, x), 0, UnityEngine.Random.Range(0, z)));
        FloorCreate(out Vector3[] oneFloorSecond, (x * z) / 4, new Vector3(UnityEngine.Random.Range(0, x), 0, UnityEngine.Random.Range(0, z)));
        FloorCreate(out Vector3[] oneFloorThird, (x * z) / 4, new Vector3(UnityEngine.Random.Range(0, x), 0, UnityEngine.Random.Range(0, z)));
        FloorCreate(out Vector3[] twoFloorFirst, oneFloorFirst.Length / 3, oneFloorFirst[UnityEngine.Random.Range(0, oneFloorFirst.Length)]);
        FloorCreate(out Vector3[] twoFloorSecond, oneFloorSecond.Length / 3, oneFloorSecond[UnityEngine.Random.Range(0, oneFloorSecond.Length)]);
        FloorCreate(out Vector3[] threeFloorFirst, twoFloorSecond.Length / 2, twoFloorSecond[UnityEngine.Random.Range(0, twoFloorSecond.Length)]);
        ForCube((x, y, z) =>
        {
            if (cubes[x, y, z].type != CUBE_TYPE.Air)
            {
                CreateCube(ref cubes[x, y, z]);
            }
        }
        );
    }

    //전체 cube배열 순환
    internal void ForCube(Action<int, int, int> action)
    {
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    action(i, j, k);
                }
            }
        }
    }


    //고저차 생성
    internal void FloorCreate(out Vector3[] floor,int floorSize,Vector3 origin)
    {
        floor = new Vector3[floorSize];
        if(floorSize == 0) return;
        floor[0] = origin;
        int creatCount = 1;
        while(creatCount <floor.Length)//지형 적합성 체크
        {
            Vector3 center = floor[UnityEngine.Random.Range(0, creatCount)];
            Shuffle.Array(ref check);
            for (int j = 0; j < check.Length; j++)
            {
                if (floor.Contains(center + check[j])) continue;
                if (GroundCheck(center + check[j]))
                {
                    floor[creatCount] = center + check[j];
                    creatCount++;
                    break;
                }
            }
        }
        for(int i = 0; i < floor.Length; i++)
        {
            floor[i] += new Vector3(0, 1, 0);
            cubes[(int)floor[i].x, (int)floor[i].y, (int)floor[i].z].type = CUBE_TYPE.Ground;
        }
    }


    //좌표로 그라운드 체크
    public bool GroundCheck(Vector3 position)
    {
        int coordx = (int)position.x;
        int coordy = (int)position.y;
        int coordz = (int)position.z;
        if (coordx<0||coordz<0||coordy<0||coordx>=x||coordz>=z||coordy>=y)
        {
            return false;
        }
        return cubes[(int)position.x, (int)position.y, (int)position.z].type != CUBE_TYPE.Air;
    }


    //큐브 데이터 정리 및 실제 씬에 그려질 큐브 생성
    internal void CreateCube(ref Cube cube)
    {
        int coordx = (int)cube.coord.x;
        int coordy = (int)cube.coord.y;
        int coordz = (int)cube.coord.z;
        bool isground = false;
        for(int i = -1;i<=1;i++)
        {
            for(int j = -1;j<=1;j++)
            {
                for(int k = -1;k<=1;k++)
                {
                    if (i == 0 & j == 0 & k == 0) continue;
                    if (coordx + i < 0 || coordx + i >= x || coordz + k < 0 || coordz + k >= z || coordy + j < 0 || coordy + j >= y)
                    {
                        isground = true;
                    }
                    else
                    {
                        if (cubes[coordx+i, coordy + j, coordz + k].type==CUBE_TYPE.Air)
                        {
                            isground = true;
                        }
                    }
                }
            }
        }
        if(isground)
        {
            cube.type = CUBE_TYPE.Ground;
            if (coordy+1<y)
            {
                if (cubes[coordx, coordy + 1, coordz].type != CUBE_TYPE.Air)
                {
                    cube.type = CUBE_TYPE.Bed;
                }
            }

            //Addressables.InstantiateAsync(field.fieldBase[UnityEngine.Random.Range(0, field.fieldBase.Length)], transform.position + cube.coord, Quaternion.identity, transform);


        }
        else
        {
            cube.type = CUBE_TYPE.Null;
        }
    }

    internal void FieldInfoSet(FieldInfo info)
    {
        info.field = new Dictionary<CUBE_TYPE, AssetReference[]>();
        foreach(var fieldCube in info.data)
        {
            info.field.Add(fieldCube.type, fieldCube.assets);
        }
    }
}
