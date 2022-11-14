using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using System.Drawing;
using JetBrains.Annotations;

public enum CUBE_TYPE { Air,Null,Ground,Bed,Water,Out,Obstacle}
public enum DIRECTION
{
    Down =0,Up =1,Left =2,Right =3,
}

//����ü�� stack�� �Ҵ������� ť�� �����Ͱ� Ŀ������ ����ִ� Cube�� �޸� ������ �����ϰ� ����.
//Cube ���� ������ üũ�ϴ� type�� ����ü�� �Ҵ��ϰ� ���� ���Ǵ� ť�� �����ʹ� Ŭ������ �Ҵ�?
//null�� �������� �ʴ� Cube�鸸 ���� CubeData �Ҵ��ϰ� null�� ť��� �������� ������ �ְ� ���� -> �� �� ȿ�������� ���� ����������?
//class�� ����� enum�� ���� ������ ���� üũ ������ �͵鿡�� ���� �Ҵ��. �����°� �´�
public struct Cube
{
    public CUBE_TYPE type;
    public CubeData data;
    public Cube(CUBE_TYPE type)
    {
        this.type = type;
        data = null;
    }

}
public class CubeData
{

}


public class FieldGenerator : MonoBehaviour
{
    [SerializeField] private int riverCount;
    [SerializeField] private int waterDepth;
    [SerializeField] private Vector3Int size;
    [SerializeField,Range(0,1)] private float treeOffset;
    [SerializeField,Range(0,1)] private float stumpOffset;


    public List<Vector3Int> groundList = new List<Vector3Int>();
    public List<Vector3Int> waterList = new List<Vector3Int>();
    public FieldInfo field;
    public Cube[,,] cubes;
    public Cube outOfRange = new Cube(CUBE_TYPE.Out);
    private Vector3Int[] side = new Vector3Int[]
    {
        new Vector3Int(0,0,1),
        new Vector3Int(0,0,-1),
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0),
    };

    internal void AllCube(Action<int,int,int> action)
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
    public ref Cube Cube(Vector3Int coord)
    {
        try
        {
            return ref cubes[coord.x, coord.y, coord.z];
        }
        catch (IndexOutOfRangeException)
        {
            return ref outOfRange;
        }
    }



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
        Vector3Int[] zeroFloor = new Vector3Int[size.x * size.z];
        int count = 0;
        for(int i =0; i<size.x;i++)
        {
            for(int j = 0;j<size.z;j++)
            {
                cubes[i, 0, j].type = CUBE_TYPE.Bed;
                zeroFloor[count] = new Vector3Int(i, 0, j);
                count++;
            }
        }

        ///////////////////////���� ������ ����//////////////////////
        FloorCreate(zeroFloor, size.y - 1, 5);
        for(int i =0;i<riverCount;i++)
            RiverCreate();
        WaterCheck();
        ////////////////////////////////////////////////////////////

        ///////////////////////���� ǥ�� ��Ƴ���////////////////////
        AllCube((x, y, z) => {
            CUBE_TYPE cubeType = cubes[x, y, z].type;
            if (cubes[x, y, z].type!= CUBE_TYPE.Air)
            {
                GroundCreate(x, y, z);
                NullCreate(x, y, z);
            }
        });

        GroundRefine();

        AllCube((x, y, z) => {
            if (cubes[x, y, z].type != CUBE_TYPE.Air)
                CreateCube(new Vector3Int(x, y, z));
        });


        ////////////////////////////////////////////////////////////

        TreeCreator();



    }



    #region FloorCreator

    /// <summary>
    /// ��� ���� ���� �޼���
    /// </summary>
    /// <param name="baseFloor">���̽� �� </param>
    /// <param name="floorNum">�ְ� ����</param>
    /// <param name="offset">���� ����</param>
    internal void FloorCreate(Vector3Int[] baseFloor,int floorNum,int offset)
    {
        List<Vector3Int[]> curFloors = new List<Vector3Int[]>(floorNum);
        for (int i = 0; i < floorNum; i++)
            curFloors.Add(baseFloor);
        for (int i = floorNum; i > 0; i--)
        {
            List<Vector3Int[]> nextFloors = new List<Vector3Int[]>();
            for (int j = 0; j < i; j++)
            {
                int index = Random.Range(0, curFloors.Count);
                Vector3Int[] prevFloor = curFloors[index];
                if (FloorSet(prevFloor, out Vector3Int[] floor, offset))
                {
                    curFloors.RemoveAt(index);
                    nextFloors.Add(floor);
                }
            }
            curFloors = nextFloors;
        }
    }
  
    internal bool FloorSet(Vector3Int[] baseFloor,out Vector3Int[] floor, int offset)
    {
        int creatCount = 1;
        int floorSize = baseFloor.Length / offset;
        if (floorSize == 0)
        {
            floor = null;
            return false;
        }
        floor = new Vector3Int[floorSize];
        floor[0] = baseFloor[Random.Range(0, baseFloor.Length)];
        while(creatCount <floor.Length)//���� ���ռ� üũ
        {
            Vector3Int center = floor[Random.Range(0, creatCount)];
            Shuffle.Array(ref side);
            for (int j = 0; j < side.Length; j++)
            {
                if (floor.Contains(center + side[j]))
                    continue;
                if (Cube(center + side[j]).type == CUBE_TYPE.Bed)
                {
                    floor[creatCount] = center + side[j];
                    creatCount++;
                    break;
                }
            }
        }
        for(int i = 0; i < floor.Length; i++)
        {
            floor[i].y += 1;
            Cube(floor[i]).type = CUBE_TYPE.Bed;
        }
        return true;
    }

    #endregion

    #region RiverCreator
    /// <summary>
    /// ���ͽ�Ʈ�� ��� �� ���� ���� �޼���
    /// </summary>
    internal void RiverCreate()
    {
        DIRECTION[] direction = Enum.GetValues(typeof(DIRECTION)) as DIRECTION[];
        Shuffle.Array(ref direction);
        Vector3Int beginPoint = SetPoint(direction[0]);
        Vector3Int endPoint = SetPoint(direction[1]);
        float distance = Vector3Int.Distance(beginPoint, endPoint);
        while(true)
        {
            Vector3Int targetCoord = beginPoint;
            for(int i =0; i<size.y;i++)
            {
                WaterCube(targetCoord, waterDepth);
                targetCoord.y++;          
            }
            if (distance == 0) break;
            distance = NextCoord(ref beginPoint, endPoint, distance);
        }
    }


    /// <summary>
    /// ���,�������� ���� ��Ī
    /// </summary>
    internal Vector3Int SetPoint(DIRECTION direct)
    {
        switch(direct)
        {
            case DIRECTION.Down:
                return new Vector3Int(Random.Range(0,size.x),0,0);
            case DIRECTION.Up:   
                return new Vector3Int(Random.Range(0, size.x),0, size.z-1);
            case DIRECTION.Left: 
                return new Vector3Int(0, 0,Random.Range(0, size.z));
            case DIRECTION.Right:
                return new Vector3Int(size.x-1, 0,Random.Range(0, size.z));
            default:
                return Vector3Int.zero;
        }
    }


    internal float NextCoord(ref Vector3Int coord,Vector3Int endPoint,float minDistance)
    {
        Vector3Int nextCoord = coord;
        Shuffle.Array(ref side);
        foreach(var sideCheck in side)
        {
            float distance = Vector3Int.Distance(coord + sideCheck, endPoint);
            if (minDistance>distance)
            {
                nextCoord = coord+sideCheck;
                minDistance = distance;
            }
        }
        coord = nextCoord;
        return minDistance;
    }

    internal void WaterCube(Vector3Int coord,int depth)
    {
        if(SurfaceCheck(coord,CUBE_TYPE.Air)>1 | coord.y>depth)
        {
            Cube(coord).type = CUBE_TYPE.Air;
            return;
        }
        Cube(coord).type = CUBE_TYPE.Water;
        waterList.Add(coord);
    }


    internal void WaterCheck()
    {
        foreach (var water in waterList)
        {
            foreach (var checkPoint in side)
            {
                if (Cube(water + checkPoint).type == CUBE_TYPE.Air)
                    if (Cube(water + checkPoint + new Vector3Int(0, -1, 0)).type != CUBE_TYPE.Water)
                        Cube(water + checkPoint).type = CUBE_TYPE.Bed;
            }
        }
        foreach (var water in waterList)
        {
            if(SurfaceCheck(water,CUBE_TYPE.Air)>1)
                Cube(water).type = CUBE_TYPE.Air;
        }
    }

    #endregion

    #region GroundRefinement
    internal void GroundCreate(int x, int y, int z)
    {
        if (cubes[x,y,z].type != CUBE_TYPE.Bed)
            return;
        CUBE_TYPE topCubeType = Cube(new Vector3Int(x, y + 1, z)).type;
        if (topCubeType == CUBE_TYPE.Air | topCubeType == CUBE_TYPE.Out)
        {
            cubes[x, y, z].type = CUBE_TYPE.Ground;
            groundList.Add(new Vector3Int(x, y, z));
        }
            
    }

    internal void GroundRefine()
    {
        var refinedGround = new List<Vector3Int>();
        var bedGround = new List<Vector3Int>();
        CUBE_TYPE[] types = new CUBE_TYPE[] { CUBE_TYPE.Ground, CUBE_TYPE.Bed, CUBE_TYPE.Out };
        while (true)
        {
            int refineCount = 0;
            foreach (var ground in groundList)
            {
                Vector3Int checkPoint = ground + Vector3Int.up;
                if (Cube(checkPoint).type == CUBE_TYPE.Out) continue;
                if (SurfaceCheck(checkPoint, types) > 2)
                {
                    Cube(checkPoint).type = CUBE_TYPE.Ground;
                    Cube(ground).type = CUBE_TYPE.Bed;
                    refinedGround.Add(checkPoint);
                    bedGround.Add(ground);
                    refineCount++;
                }
            }
            for(int i =0; i< refineCount; i++)
            {
                groundList.Add(refinedGround[i]);
                groundList.Remove(bedGround[i]);
            }
            refinedGround.Clear();
            bedGround.Clear();
            if (refineCount == 0)
                break;
        }
    }


    /// <summary>
    /// ������ �� �� ���� ��ġ�� �ִ� ť��� ����
    /// </summary>
    internal void NullCreate(int x, int y, int z)
    {
        if (cubes[x, y, z].type == CUBE_TYPE.Ground) return;
        else
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    for (int k = -1; k <= 1; k++)
                    {
                        CUBE_TYPE checkType = Cube(new Vector3Int(x+i, y+j, z+k)).type;
                        if (i == 0 & j == 0 & k == 0) continue;
                        if (checkType == CUBE_TYPE.Air)
                            return;
                        if (checkType == CUBE_TYPE.Out & y + j >= 0)
                            return;
                    }
                }
            }
        }
        cubes[x, y, z].type = CUBE_TYPE.Null;

    }

    #endregion

    #region ObstacleCreator
    internal void TreeCreator()
    {
        for (int i = 0; i < groundList.Count * treeOffset; i++)
        {
            Vector3Int targetCoord = groundList[Random.Range(0, groundList.Count)];
            targetCoord.y++;
            if (Cube(targetCoord).type == CUBE_TYPE.Out) continue;
            Cube(targetCoord).type = CUBE_TYPE.Obstacle;
            RandomAddressable.Instantiate(field.tree, (Vector3)targetCoord+transform.position, Quaternion.identity, transform);
        }
    }



    #endregion



    /// <summary>
    /// 4���� ���˸� üũ
    /// </summary>
    /// <param name="coord">Ÿ�� ��ǥ</param>
    /// <param name="type">�����ϰ� ���� Ÿ��</param>
    /// <returns></returns>
    internal int SurfaceCheck(Vector3Int coord, CUBE_TYPE type)
    {
        int surface = 0;
        foreach (Vector3Int checkPoint in side)
            if (Cube(coord + checkPoint).type == type)
                surface++;
        return surface;
    }

    internal int SurfaceCheck(Vector3Int coord, CUBE_TYPE[] types)
    {
        int surface = 0;
        foreach (Vector3Int checkPoint in side)
            foreach(var type in types)
                if (Cube(coord + checkPoint).type == type)
                    surface++;
        return surface;
    }



    internal void CreateCube(Vector3Int coord)
    {
        Cube cube = Cube(coord);
        if (field.field.TryGetValue(cube.type, out AssetLabelReference target))
        {
            Vector3 point = transform.position + coord;
            if (cube.type == CUBE_TYPE.Water)
                point -= new Vector3(0, 0.3f, 0);
            RandomAddressable.Instantiate(target, point, Quaternion.identity, transform);//
        }
    }


}
