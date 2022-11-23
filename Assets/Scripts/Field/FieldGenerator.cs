using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using System.Drawing;
using JetBrains.Annotations;

public enum CUBE_TYPE { Air,Null,Ground,Bed,Water,Out,Obstacle,OnCharacter}
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
    public Cube(CUBE_TYPE type)
    {
        this.type = type;
        data = null;
    }

}
public class CubeData
{
    public Character onChracter;

    public CubeData() { }
    public CubeData(Character onChracter)
    {
        this.onChracter = onChracter;
    }
}


public class FieldGenerator : Singleton<FieldGenerator>
{
    [SerializeField] private int riverCount;
    [SerializeField] private int waterDepth;
    [SerializeField] public Vector3Int size;
    [SerializeField,Range(0,1)] private float treeOffSet;
    [SerializeField,Range(0,1)] private float stumpOffSet;
    [SerializeField, Range(0, 1)] private float floorOffSet;

    [HideInInspector] public Dictionary<Vector2Int, int> surfaceList = new Dictionary<Vector2Int, int>();
    [HideInInspector] public List<Vector3Int> groundList = new List<Vector3Int>();
    [HideInInspector] public List<Vector3Int> waterList = new List<Vector3Int>();
    [HideInInspector] public Cube[,,] cubes;
    public FieldInfo field;
    private Cube outOfRange = new Cube(CUBE_TYPE.Out);
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


    private void Awake()
    {
        GenerateField(field);
    }





    public void GenerateField(FieldInfo fieldInfo)
    {
        fieldInfo.FieldSet();
        cubes = new Cube[size.x, size.y+1, size.z];
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

        ///////////////////////지형 데이터 생성//////////////////////
        FloorCreate(zeroFloor, size.y - 1, floorOffSet);

        for(int i =0;i<riverCount;i++)
            RiverCreate();
        RiverRefinement();

        AllCube((x, y, z) => GroundSet(x, y, z));
        GroundRefinement();

        SurfaceListSet();
        ////////////////////////////////////////////////////////////

        ////////////////////////큐브 생성////////////////////////////

        AllCube((x, y, z) =>CreateCube(x, y, z));
        ////////////////////////////////////////////////////////////

        /////////////////////맵 환경 설정///////////////////////////
        TreeCreator();
    }



    #region FloorCreator

    /// <summary>
    /// 계단 지형 생성 메서드
    /// </summary>
    /// <param name="baseFloor">베이스 층 </param>
    /// <param name="floorNum">최고 층수</param>
    /// <param name="offset">지형 비율</param>
    internal void FloorCreate(Vector3Int[] baseFloor,int floorNum,float offset)
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


    internal bool FloorSet(Vector3Int[] baseFloor,out Vector3Int[] floor, float offset)
    {
        int creatCount = 1;
        int floorSize =(int)((float)baseFloor.Length * offset);
        if (floorSize == 0)
        {
            floor = null;
            return false;
        }
        floor = new Vector3Int[floorSize];
        floor[0] = baseFloor[Random.Range(0, baseFloor.Length)];
        while(creatCount <floor.Length)//지형 적합성 체크
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
    /// 다익스트라 기반 강 지형 생성 메서드
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
                WaterCubeSet(targetCoord, waterDepth);
                targetCoord.y++;          
            }
            if (distance == 0) break;
            distance = NextCoord(ref beginPoint, endPoint, distance);
        }
    }


    /// <summary>
    /// 출발,도착지점 랜덤 매칭
    /// </summary>
    internal Vector3Int SetPoint(DIRECTION direct)
    {
        switch(direct)
        {
            case DIRECTION.Down:
                return new Vector3Int(Random.Range(1,size.x-1),0,0);
            case DIRECTION.Up:   
                return new Vector3Int(Random.Range(1, size.x-1),0, size.z-1);
            case DIRECTION.Left: 
                return new Vector3Int(0, 0,Random.Range(1, size.z-1));
            case DIRECTION.Right:
                return new Vector3Int(size.x-1, 0,Random.Range(1, size.z-1));
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

    internal void WaterCubeSet(Vector3Int coord,int depth)
    {
        if(SurfaceCheck(coord,CUBE_TYPE.Air)>1 | coord.y>depth)
        {
            Cube(coord).type = CUBE_TYPE.Air;
            return;
        }
        Cube(coord).type = CUBE_TYPE.Water;
        waterList.Add(coord);
    }


    internal void RiverRefinement()
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
    internal void GroundSet(int x,int y, int z)
    {
        if (cubes[x, y, z].type != CUBE_TYPE.Air)
        {
            GroundCubeSet(x, y, z);
            NullCubeSet(x, y, z);
        }
    }


    internal void GroundCubeSet(int x, int y, int z)
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

    /// <summary>
    /// 유저가 볼 수 없는 위치에 있는 큐브는 제거
    /// </summary>
    internal void NullCubeSet(int x, int y, int z)
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
                        CUBE_TYPE checkType = Cube(new Vector3Int(x + i, y + j, z + k)).type;
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

    internal void GroundRefinement()
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




    #endregion

    #region ObstacleCreator
    internal void TreeCreator()
    {
        for (int i = 0; i < groundList.Count * treeOffSet; i++)
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
    /// 4방위 접촉면 체크
    /// </summary>
    /// <param name="coord">타겟 좌표</param>
    /// <param name="type">검출하고 싶은 타입</param>
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



    internal void CreateCube(int x,int y,int z)
    {
        Vector3Int coord = new Vector3Int(x, y, z);
        Cube cube = Cube(coord);

        if(cube.type!=CUBE_TYPE.Air)
            if (field.field.TryGetValue(cube.type, out AssetLabelReference target))
            {
                Vector3 point = transform.position + coord;
                if (cube.type == CUBE_TYPE.Water)
                    point -= new Vector3(0, 0.3f, 0);
                RandomAddressable.Instantiate(target, point, Quaternion.identity, transform);//
            }
    }


    internal void SurfaceListSet()
    {
        for(int i=0; i<size.x;i++)
            for(int j=0; j<size.z;j++)
                for (int k = 0; k < size.y+1; k++)
                    if (Cube(new Vector3Int(i, k, j)).type == CUBE_TYPE.Air)
                    {
                        surfaceList.Add(new Vector2Int(i, j), k);
                        break;
                    }
    }
    public bool Surface(Vector2Int key,out Vector3 coord)
    {
        coord = Vector3.zero;
        if (surfaceList.TryGetValue(key, out int y))
        {
            coord = new Vector3(key.x, y, key.y);
            return true;
        }
        return false;
    }
    public bool Surface(Vector2Int key, out Vector3Int coord)
    {
        coord = Vector3Int.zero;
        if (surfaceList.TryGetValue(key, out int y))
        {
            coord = new Vector3Int(key.x, y, key.y);
            return true;
        }
        return false;
    }




    public CUBE_TYPE SurfaceState(Vector2Int key)
    {
        if (Surface(key, out Vector3Int coord))
        {
            if (Cube(coord -Vector3Int.up).type == CUBE_TYPE.Water)
                return CUBE_TYPE.Water;
            else
                return Cube(coord).type;
        }
        return CUBE_TYPE.Out;
    }

    public CubeData CubeDataCall(Vector3Int coord)
    {
        if(Cube(coord).type!=CUBE_TYPE.Out)
        {
            CubeData data;
            if (Cube(coord).data == null)
                data = new CubeData();
            else
                data = Cube(coord).data;

            return data;
        }
        return null;
    }

    public void PointSet(out Vector2Int partySpot,out Vector2Int enemySpot)
    {
        partySpot = Vector2Int.zero;
        enemySpot = new Vector2Int(size.x - 1, size.z - 1);
        for(int i=0; i<size.x;i++)
        {
            bool isBreak = false;
            for (int j = 0; j < size.z; j++)
            {
                Vector2Int spot = new Vector2Int(i, j);
                if (SurfaceState(spot) != CUBE_TYPE.Air)
                    continue;
                int acceptCount = 0;
                CubeCheck.CustomCheck(CHECK_TYPE.SIDE, spot, 1,
                    (check) => SurfaceState(check) == CUBE_TYPE.Air,
                    (none) => { acceptCount++; });
                if (acceptCount == 8)
                {
                    partySpot = spot;
                    isBreak = true;
                    break;
                }
            }
            if (isBreak)
                break;
        }
        for (int i = size.x-1; i>=0; i--)
        {
            bool isBreak = false;
            for (int j = size.z-1; j >=0; j--)
            {
                Vector2Int spot = new Vector2Int(i, j);
                if (SurfaceState(spot) != CUBE_TYPE.Air)
                    continue;
                int acceptCount = 0;
                CubeCheck.CustomCheck(CHECK_TYPE.SIDE, spot, 1,
                    (check) => SurfaceState(check) == CUBE_TYPE.Air,
                    (none) => acceptCount++);
                if (acceptCount == 8)
                {
                    enemySpot = spot;
                    isBreak = true;
                    break;
                }
            }
            if (isBreak)
                break;
        }
    }



}
