using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

public class FieldManager : Singleton<FieldManager>
{
    [HideInInspector] public Voxel[][][] voxels;
    [SerializeField] private Vector3Int fieldSize;
    [SerializeField] private int floorHeight;
    [SerializeField] private int riverCount;
    [SerializeField, Range(0, 1f)] private float riverDepth;
    /// <summary>
    /// 0 : bed, 1: water 2~ : ground
    /// </summary>
    private List<GameObject> sampleGos = new List<GameObject>();
    private List<GameObject> createdGos = new List<GameObject>();
    public FieldThema thema;

    [HideInInspector]
    public Dictionary<Vector2Int, Vector3Int> surfaceDic = new Dictionary<Vector2Int, Vector3Int>();

    public void Start()
    {
        Generate();
    }


    public Voxel Voxel(Vector3Int coord)
    {
        try
        {
            return voxels[coord.x][coord.y][coord.z];
        }
        catch(IndexOutOfRangeException)
        {
            return null;
        }     
    }
    public void VoxelsLoopY(int y,Action<Vector3Int> action)
    {
        for (int x = 0; x < fieldSize.x; x++)
            for (int z = 0; z < fieldSize.z; z++)
                action.Invoke(new Vector3Int(x, y, z));
    }
    public void VoxelsLoopXZ(Vector2Int coord2D,Action<Vector3Int> action)
    {
        int x = coord2D.x;
        int z = coord2D.y;
        for (int i = 0; i < fieldSize.y; i++)
            action.Invoke(new Vector3Int(x,i,z));
    }

    public void VoxelsLoop(Action<Vector3Int> action)
    {
        for(int i = 0; i<fieldSize.x;i++)
            for(int j = 0; j<fieldSize.y;j++)
                for(int k = 0; k<fieldSize.z;k++)
                    action.Invoke(new Vector3Int(i,j, k));
    }


    public void Generate()
    {
        VoxelFieldCreate();      
        FloorCreate();
        RiverCreate();
        GroundCreate();
        NullVoxelCheck();
        SceneFieldCreate();
    }


    private void VoxelFieldCreate()
    {
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
                float height = Mathf.PerlinNoise((i + randomSeed) * scaleX, (j + randomSeed) * scaleZ);
                int y = (int)(height * floorHeight);
                for (int k = 0; k < y; k++)
                    Voxel(new Vector3Int(i, k + 1, j)).type = VoxelType.Bed;
            }
    }

    public void RiverCreate()
    {
        Vector3Int[] sideDirections2D = new Vector3Int[] { Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right };
        DIRECTION[] direction = Enum.GetValues(typeof(DIRECTION)) as DIRECTION[];
        List<Vector3Int> waterList = new List<Vector3Int>();
        Voxel voxel = null;
        int waterDepth = (int)(floorHeight * riverDepth);
        for (int i = 0; i < riverCount; i++)
        {       
            Shuffle.Array(ref direction);
            Vector3Int beginPoint = SetPoint(direction[0]);
            Vector3Int endPoint = SetPoint(direction[1]);
            float distance = Vector3Int.Distance(beginPoint, endPoint);
            while (true)
            {
                Vector2Int targetCoord = beginPoint.To2DInt();
                VoxelsLoopXZ(targetCoord, (coord) =>
                {
                    voxel = Voxel(coord);
                    if (voxel?.type == VoxelType.Bed)
                    {
                        if (coord.y <= waterDepth)
                        {
                            voxel.type = VoxelType.Water;
                            waterList.Add(coord);
                        }                           
                        else
                            voxel.type = VoxelType.Air;
                    }
                });

                if (distance == 0) break;

                Shuffle.Array(ref sideDirections2D);
                foreach (Vector3Int direct in sideDirections2D)
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

        RiverCorrection(waterList);

    }
    internal void RiverCorrection(List<Vector3Int> waterList)
    {

        foreach (Vector3Int waterCoord in waterList)
        {
            CoordCheck.SideCheck2D(waterCoord,
                (checkCoord) =>
                {
                    if (Voxel(checkCoord)?.type == VoxelType.Air)
                        if (Voxel(checkCoord + Vector3Int.down)?.type != VoxelType.Water)
                            return true;
                    return false;
                },
                (checkCoord) => Voxel(checkCoord).type = VoxelType.Ground);
        }
        while (true)
        {
            int correctedCount = 0;
            foreach(Vector3Int waterCoord in waterList)
            {
                if (Voxel(waterCoord)?.type == VoxelType.Air)
                    continue;
                int airContactSurfaceCount = 0;
                CoordCheck.SideCheck2D(waterCoord,
                    (checkCoord) => Voxel(checkCoord)?.type == VoxelType.Air,
                    (none) => airContactSurfaceCount++,
                    false) ;
                if(airContactSurfaceCount>1)
                {
                    Voxel(waterCoord).type = VoxelType.Air;
                    correctedCount++;
                }
            }
            if (correctedCount == 0)
                break;
        }
    }


    internal void GroundCreate()
    {
        Voxel topVoxel = null;
        VoxelsLoop((coord) =>
        {
            if (Voxel(coord).type == VoxelType.Air)
                return;
            topVoxel = Voxel(coord + Vector3Int.up);
            if (topVoxel == null | topVoxel?.type == VoxelType.Air)
            {
                if (Voxel(coord).type == VoxelType.Bed)
                    Voxel(coord).type = VoxelType.Ground;
                surfaceDic.Add(new Vector2Int(coord.x, coord.z), coord);
            }               
        });
    }

    private void NullVoxelCheck()
    {
        VoxelsLoop((voxelCoord) =>
        {
            if (Voxel(voxelCoord).type == VoxelType.Air)
                return;
            bool isNull = true;
            CoordCheck.SideCheck3D(voxelCoord,
                (checkCoord) =>
                {
                    if (checkCoord.y < 0)
                        return false;
                    if (Voxel(checkCoord) == null | Voxel(checkCoord)?.type == VoxelType.Air)
                        return true;
                    return false;
                },
                (none) => { isNull = false; });
            if (isNull)
                Voxel(voxelCoord).type = VoxelType.Null;
        });
    }

    private void SceneFieldCreate()
    {
        FieldReset();
        sampleGos.Add(Addressables.LoadAssetAsync<GameObject>(thema.bed).WaitForCompletion());
        sampleGos.Add(Addressables.LoadAssetAsync<GameObject>(thema.water).WaitForCompletion());
        foreach(var reference in thema.ground)
            sampleGos.Add(Addressables.LoadAssetAsync<GameObject>(reference).WaitForCompletion());

        GameObject sampleGo = null;
        VoxelsLoop((coord) =>
        {
            Vector3 position = coord;
            switch(Voxel(coord).type)
            {
                case VoxelType.Bed:
                    sampleGo = sampleGos[0];
                    break;
                case VoxelType.Water:
                    sampleGo = sampleGos[1];
                    position += Vector3.down * 0.3f;
                    break;
                case VoxelType.Ground:
                    sampleGo = sampleGos[Random.Range(2, sampleGos.Count)];
                    break;
                default: 
                    return;
            }
            createdGos.Add(Instantiate(sampleGo, position, Quaternion.identity));
        });
    }

    private void FieldReset()
    {
        foreach (GameObject go in createdGos)
            Addressables.Release(go);
        foreach (GameObject go in sampleGos)
            Addressables.Release(go);
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






}
