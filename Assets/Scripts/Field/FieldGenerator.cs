using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;
using JetBrains.Annotations;
using System.Data;
using Unity.VisualScripting;

public enum CUBE_TYPE { Air,Null,Ground,Hill,Bed,Water,Error}
public enum DIRECTION
{
    Down =0,Up =1,Left =2,Right =3,
}

//구조체라 stack에 할당하지만 큐브 데이터가 커질수록 비어있는 Cube가 메모리 공간을 차지하고 있음.
//Cube 존재 유무를 체크하는 type만 구조체로 할당하고 실제 사용되는 큐브 데이터는 클래스로 할당?
//null로 존재하지 않는 Cube들만 힙에 CubeData 할당하고 null인 큐브는 참조값만 가지고 있게 하자 -> 좀 더 효율적으로 관리 가능할지도?
//class로 선언시 enum을 통해 간단한 상태 체크 가능한 것들에도 힙에 할당됨. 나누는게 맞다
public struct Cube
{

    public CUBE_TYPE type;
    public CubeData data;
}
public class CubeData
{

}


public class FieldGenerator : MonoBehaviour
{
    [SerializeField] private Vector3Int size;
    public FieldInfo field;
    public Cube[,,] cubes;
    public Cube outOfRange;
    internal ref Cube Cubes(Vector3Int coord)
    {
        try
        {
            return ref cubes[coord.x, coord.y, coord.z];
        }
        catch(IndexOutOfRangeException)
        {
            outOfRange.type = CUBE_TYPE.Error;
            return ref outOfRange;
        }
        
    }

    private Vector3Int[] check = new Vector3Int[]
    {
        new Vector3Int(0,0,1),
        new Vector3Int(0,0,-1),
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0),
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
        fieldInfo.FieldSet();
        cubes = new Cube[size.x, size.y, size.z];
        List<Vector3Int> baseList = new List<Vector3Int>();
        ForCube((x, y, z) =>
        {
            if (y == 0)
            {
                cubes[x, y, z].type = CUBE_TYPE.Ground;
                baseList.Add(new Vector3Int(x, y, z));
            }
        }
        );
        Vector3Int[] baseFloor = baseList.ToArray();
        

        FloorCreate(FloorCreate(FloorCreate(baseFloor)));
        FloorCreate(FloorCreate(FloorCreate(baseFloor)));
        FloorCreate(FloorCreate(FloorCreate(baseFloor)));


        baseList.Clear();
        ForCube((x, y, z) =>
        {
            if (cubes[x, y, z].type != CUBE_TYPE.Air)
            {
                GroundCube(ref cubes[x, y, z],new Vector3Int(x,y,z),baseList);
            }
        }
        );

        WaterCube();
        WaterCube();
        WaterCube();
        WaterCube();

        ForCube((x, y, z) =>
        {

            if (cubes[x, y, z].type != CUBE_TYPE.Air)
            {
                CreateCube(ref cubes[x, y, z], new Vector3Int(x, y, z));
            }
        }
        );

    }

    //전체 cube배열 순환
    internal void ForCube(Action<int, int, int> action)
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    action(i, j, k);
                }
            }
        }
    }



    //고저차 생성
    internal Vector3Int[] FloorCreate(Vector3Int[] baseFloor)
    {
        int floorSize = baseFloor.Length / 5;
        if (floorSize == 0)
            return null;
        Vector3Int[] floor = new Vector3Int[floorSize];
        floor[0] = baseFloor[Random.Range(0, baseFloor.Length)];
        int creatCount = 1;
        while(creatCount <floor.Length)//지형 적합성 체크
        {
            Vector3Int center = floor[Random.Range(0, creatCount)];
            Shuffle.Array(ref check);
            for (int j = 0; j < check.Length; j++)
            {
                if (floor.Contains(center + check[j]))
                    continue;
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
            floor[i].y += 1;
            Cubes(floor[i]).type = CUBE_TYPE.Ground;
        }
        return floor;
    }


    //좌표로 그라운드 체크
    public bool GroundCheck(Vector3Int position)
    {
        if (position.x<0|position.x >= size.x|
            position.z<0|position.z >= size.z| 
            position.y<0|position.y >= size.y )
            return false;
        return Cubes(position).type != CUBE_TYPE.Air;
    }


    //큐브 데이터 정리 및 실제 씬에 그려질 큐브 생성
    internal void GroundCube(ref Cube cube,Vector3Int coord, List<Vector3Int> groundList)
    {
        bool isground = false;
        for(int i = -1;i<=1;i++)
        {
            for(int j = -1;j<=1;j++)
            {
                for(int k = -1;k<=1;k++)
                {
                    if (i == 0 & j == 0 & k == 0)
                        continue;
                    if (coord.y + j < 0)
                        continue;
                    Vector3Int checkCoord = new Vector3Int(coord.x + i, coord.y + j, coord.z + k);
                    if (checkCoord.x < 0 |
                        checkCoord.z < 0 |
                        checkCoord.x >= size.x|
                        checkCoord.z >= size.z|
                        checkCoord.y >= size.y)
                        isground = true;
                    else if (Cubes(checkCoord).type == CUBE_TYPE.Air)
                        isground = true;
                }
            }
        }
        if(isground)
        {
            cube.type = CUBE_TYPE.Ground;
            if (coord.y+1< size.y)
                if (cubes[coord.x, coord.y + 1, coord.z].type != CUBE_TYPE.Air)
                    cube.type = CUBE_TYPE.Bed;
            if(cube.type == CUBE_TYPE.Ground)
            {
                groundList.Add(coord);
            }
        }
        else
            cube.type = CUBE_TYPE.Null;
    }


    internal void WaterCube()
    {
        Vector3Int origin = new Vector3Int();
        int up = 0;
        int right = 0;
        switch(Random.Range(0,4)) // 시작지점과 방향 설정
        {
            case (int)DIRECTION.Up:
                origin.x = Random.Range(0, size.x);
                up = 1;
                break;
            case (int)DIRECTION.Down:
                origin.x = Random.Range(0, size.x);
                origin.z = size.z - 1;
                up = -1;
                break;
            case (int)DIRECTION.Left:
                origin.z = Random.Range(0, size.z);
                right = 1;
                break;
            case (int)DIRECTION.Right:
                origin.z = Random.Range(0, size.z);
                origin.x = size.x - 1;
                right = -1;
                break;
        }
        Vector3Int target = origin;

        for(int i =0; i<size.y;i++)
        {
            target = origin;
            target.y = i;
            for (int j =0;j<size.x;j++)
            {
                CUBE_TYPE cubetype = Cubes(target).type;
                if (cubetype!=CUBE_TYPE.Air)
                {
                    CUBE_TYPE targetType;
                    if (SideCheck(ref target, up == 0))
                        targetType = CUBE_TYPE.Water;
                    else
                        targetType = CUBE_TYPE.Air;
                    if (!WaterSideCheck(ref target))
                        targetType = CUBE_TYPE.Air;
                    Cubes(target).type = targetType;
                }
                target.x += right;
                target.z += up;
            }
        }
    }


    internal bool WaterSideCheck(ref Vector3Int origin)
    {
        int surfaceCount = 0;
        for(int i = 0; i<check.Length;i++)
        {
            if (Cubes(origin + check[i]).type == CUBE_TYPE.Air)
                surfaceCount++;
        }
        if (surfaceCount > 1)
            return false;
        return true;
    }







    internal bool SideCheck(ref Vector3Int origin,bool isRight)
    {
        CUBE_TYPE cubeType;
        for (int i = -1; i <= 1; i++)
        {
            if (i == 0) continue;
            if (isRight)
                
                cubeType = Cubes(new Vector3Int(origin.x, origin.y, origin.z + i)).type;
            else
                cubeType = Cubes(new Vector3Int(origin.x+i, origin.y, origin.z)).type;

            if (cubeType == CUBE_TYPE.Air)
                return false;
        }
        return true;
    }


    internal void CreateCube(ref Cube cube, Vector3Int coord)
    {
        if (field.field.TryGetValue(cube.type, out AssetLabelReference target))
        {
            Vector3 point = transform.position + coord;
            if (cube.type == CUBE_TYPE.Water)
                point -= new Vector3(0, 0.3f, 0);
            RandomAddressable.Instantiate(target, point, Quaternion.identity, transform);//
        }


    }


}
