using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;
using JetBrains.Annotations;

public enum CUBE_TYPE { Air,Null,Ground,Hill,Bed,}


//����ü�� stack�� �Ҵ������� ť�� �����Ͱ� Ŀ������ ����ִ� Cube�� �޸� ������ �����ϰ� ����.
//Cube ���� ������ üũ�ϴ� type�� ����ü�� �Ҵ��ϰ� ���� ���Ǵ� ť�� �����ʹ� Ŭ������ �Ҵ�?
//null�� �������� �ʴ� Cube�鸸 ���� CubeData �Ҵ��ϰ� null�� ť��� �������� ������ �ְ� ���� -> �� �� ȿ�������� ���� ����������?
//class�� ����� enum�� ���� ������ ���� üũ ������ �͵鿡�� ���� �Ҵ��. �����°� �´�
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
        FloorCreate(FloorCreate(FloorCreate(baseList.ToArray())));
        FloorCreate(FloorCreate(FloorCreate(baseList.ToArray())));
        FloorCreate(FloorCreate(FloorCreate(baseList.ToArray())));
        //FloorCreate(FloorCreate(baseList.ToArray()));
        ForCube((x, y, z) =>
        {
            if (cubes[x, y, z].type != CUBE_TYPE.Air)
            {
                SortingCube(ref cubes[x, y, z],new Vector3Int(x,y,z));
            }
        }
        );
    }

    //��ü cube�迭 ��ȯ
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




    //������ ����
    internal Vector3Int[] FloorCreate(Vector3Int[] baseFloor)
    {
        int floorSize = baseFloor.Length / 5;
        if (floorSize == 0)
            return null;
        Vector3Int[] floor = new Vector3Int[floorSize];
        floor[0] = baseFloor[Random.Range(0, baseFloor.Length)];
        int creatCount = 1;
        while(creatCount <floor.Length)//���� ���ռ� üũ
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
            cubes[floor[i].x,floor[i].y,floor[i].z].type = CUBE_TYPE.Ground;
        }
        return floor;
    }


    //��ǥ�� �׶��� üũ
    public bool GroundCheck(Vector3Int position)
    {
        if (position.x<0||position.z<0||position.y<0|| position.x >= size.x ||position.z>= size.z ||position.y>= size.y)
            return false;
        return cubes[position.x, position.y, position.z].type != CUBE_TYPE.Air;
    }


    //ť�� ������ ���� �� ���� ���� �׷��� ť�� ����
    internal void SortingCube(ref Cube cube,Vector3Int coord)
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
                    if (coord.x + i < 0 || coord.x + i >= size.x || coord.z + k < 0 || coord.z + k >= size.z || coord.y + j >= size.y)
                        isground = true;
                    else if (cubes[coord.x + i, coord.y + j, coord.z + k].type == CUBE_TYPE.Air)
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
            CreateCube(ref cube, coord);
        }
        else
            cube.type = CUBE_TYPE.Null;
    }

    internal void CreateCube(ref Cube cube, Vector3Int coord)
    {
        if (field.field.TryGetValue(cube.type, out AssetLabelReference target))
            RandomAddressable.Instantiate(target, transform.position + coord, Quaternion.identity, transform);//
    }


}
