using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;



public enum VoxelType {Air,Bed,Ground,Water,Null}
public enum VoxelState {Null,Obstacle,Character}

public class Voxel
{
    public VoxelType type = VoxelType.Air;
    public VoxelState state;
    public VoxelData data;

    public Voxel(VoxelType type)=>this.type = type;
    public Voxel() { }

}

public class VoxelData
{

}
public enum Direction { Up, Down, Left, Right };

public class FieldGenearateTest : MonoBehaviour
{
    [HideInInspector] public Voxel[][][] voxels;
    [SerializeField] private Vector3Int fieldSize;
    [SerializeField] private float floorOffset;
    [SerializeField] private int floorCount;
    [SerializeField] private int floorHeight;
    [SerializeField] private int waterDepth;
    [SerializeField] private int riverCount;
    private Vector3Int[] directionVec = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
    private Vector3Int[] sideDirections = new Vector3Int[] { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right };
    private readonly Voxel outVoxel = new Voxel(VoxelType.Air);

    [SerializeField] private GameObject testBed;
    [SerializeField] private GameObject testGround;
    [SerializeField] private GameObject testWater;
    public Voxel Voxel(Vector3Int coord)
    {
        try
        {
            return voxels[coord.x][coord.y][coord.z];
        }
        catch(IndexOutOfRangeException)
        {
            return outVoxel;
        }     
    }
    public void VoxelsLoopY(int y,Action<Vector3Int> action)
    {
        for (int i = 0; i < fieldSize.x; i++)
            for (int j = 0; j < fieldSize.z; j++)
                action.Invoke(new Vector3Int(i, y, j));
    }
    public void VoxelsLoopXZ(Vector2Int coord2D,Action<Vector3Int> action)
    {
        for (int i = 0; i < fieldSize.y; i++)
            action.Invoke(new Vector3Int(coord2D.x,i,coord2D.y));
    }

    public void VoxelLoop(Action<Vector3Int> action)
    {
        for(int i = 0; i<fieldSize.x;i++)
            for(int j = 0; j<fieldSize.y;j++)
                for(int k = 0; k<fieldSize.z;k++)
                    action.Invoke(new Vector3Int(i,j, k));
    }


    public void Start()
    {
        Generate();
    }


    public void Generate()
    {
        List<Vector3Int> randomList = new List<Vector3Int>();
        Queue<Vector3Int> voxelCoordQueue = new Queue<Vector3Int>();
        List<Vector3Int> floorList = new List<Vector3Int>();


        voxels = new Voxel[fieldSize.x][][];
        for (int i = 0; i < voxels.Length; i++)
        {
            voxels[i] = new Voxel[fieldSize.y][];
            for (int j = 0; j < fieldSize.y; j++)
            {
                voxels[i][j] = new Voxel[fieldSize.z];
                for (int k = 0; k < fieldSize.z; k++)
                    voxels[i][j][k] = new Voxel();
            }              
        }

        VoxelsLoopY(0, (zeroFloorVoxel) => Voxel(zeroFloorVoxel).type = VoxelType.Bed);
        FloorCreate();
        RiverCreate();
        GroundCreate();
       
        VoxelLoop((coord) => 
        { 
            if(Voxel(coord).type == VoxelType.Bed)
                Instantiate(testBed,coord,Quaternion.identity);
            if (Voxel(coord).type == VoxelType.Water)
                Instantiate(testWater, coord+(Vector3.down*0.3f), Quaternion.identity);
            if(Voxel(coord).type == VoxelType.Ground)
                Instantiate(testGround, coord, Quaternion.identity);
        });

    }


    private void FloorCreate()
    {
        int randomSeed = Random.Range(0, 1000);
        int rangeX = fieldSize.x;
        int rangeZ = fieldSize.z;
        float scaleX = (1 - 1 / (float)rangeX) * 0.1f;
        float scaleZ = (1 - 1 / (float)rangeZ) * 0.1f;
        for (int i = 0; i < rangeX; i++)
            for (int j = 0; j < rangeZ; j++)
            {
                float height = Mathf.PerlinNoise((i + randomSeed)*scaleX, (j + randomSeed)*scaleZ);
                int y = (int)(height * floorHeight);
                for (int k = 0; k < y; k++)
                    Voxel(new Vector3Int(i, k+1, j)).type = VoxelType.Bed;
            }
    }


    internal void GroundCreate()
    {
        VoxelLoop((coord) => 
        {
            if (Voxel(coord).type == VoxelType.Bed)
                if (Voxel(coord + Vector3Int.up)==outVoxel|Voxel(coord+Vector3Int.up).type == VoxelType.Air)
                    Voxel(coord).type = VoxelType.Ground;
        });
    }



    public Vector2Int RandomOriginPoint()
    {
        switch(Random.Range(0,3))
        {
            case 0: 
                return new Vector2Int(0,fieldSize.z-1);
            case 1:
                return new Vector2Int(fieldSize.x-1, 0);
            case 2:
                return new Vector2Int(fieldSize.x-1, fieldSize.z-1);
        }
        return Vector2Int.zero;
    }

    public void RiverCreate()
    {
        List<Vector3Int> waterList = new List<Vector3Int>();
        for(int i=0; i<riverCount;i++)
        {
            DIRECTION[] direction = Enum.GetValues(typeof(DIRECTION)) as DIRECTION[];
            Shuffle.Array(ref direction);
            Vector3Int beginPoint = SetPoint(direction[0]);
            Vector3Int endPoint = SetPoint(direction[1]);
            float distance = Vector3Int.Distance(beginPoint, endPoint);
            while (true)
            {
                Vector2Int targetCoord = beginPoint.To2DInt();
                VoxelsLoopXZ(targetCoord, (coord) =>
                {
                    if (Voxel(coord).type == VoxelType.Bed)
                    {
                        if (coord.y > waterDepth)
                            Voxel(coord).type = VoxelType.Air;
                        else
                        {
                            Voxel(coord).type = VoxelType.Water;
                            waterList.Add(coord);
                        }
                    }
                });
                if (distance == 0) break;
                Shuffle.Array(ref sideDirections);
                foreach (Vector3Int direct in sideDirections)
                {
                    float checkDistance = Vector3Int.Distance(beginPoint + direct, endPoint);
                    if (checkDistance <= distance)
                    {
                        beginPoint += direct;
                        distance = checkDistance;
                        break;
                    }
                }
            }
        }
        foreach (var waterCoord in waterList)
        {
            foreach (var side in sideDirections)
            {
                if (Voxel(side + waterCoord).type == VoxelType.Air)
                    if (Voxel(side + waterCoord + Vector3Int.down).type != VoxelType.Water)
                        Voxel(side + waterCoord).type = VoxelType.Ground;
            }
        }

    }

    internal Vector3Int SetPoint(DIRECTION direct)
    {
        switch (direct)
        {
            case DIRECTION.Down:
                return new Vector3Int(Random.Range(1, fieldSize.x - 2), 0, 0);
            case DIRECTION.Up:
                return new Vector3Int(Random.Range(1, fieldSize.x - 2), 0, fieldSize.z - 1);
            case DIRECTION.Left:
                return new Vector3Int(0, 0, Random.Range(1, fieldSize.z - 2));
            case DIRECTION.Right:
                return new Vector3Int(fieldSize.x - 1, 0, Random.Range(1, fieldSize.z - 2));
            default:
                return Vector3Int.zero;
        }
    }







    private bool VoxelDirectionCheck(Vector3Int origin,Direction checkDirection,Func<Vector3Int,bool> checkFunc)
    {
        Vector3Int checkCoord = origin + directionVec[(int)checkDirection];
        return checkFunc(checkCoord);
    }

    private void SideCheck(Vector3Int origin,Func<Vector3Int,bool> condition,Action<Vector3Int> trueAction)
    {
        for(int i= -1;i<=1;i++)
            for(int j = -1;j<=1;j++)
            {
                if (i == 0 & j == 0) continue;
                Vector3Int checkTarget = new Vector3Int(origin.x + i, origin.y, origin.z + j);
                if (condition(checkTarget))
                    trueAction(checkTarget);
            }
    }



}
