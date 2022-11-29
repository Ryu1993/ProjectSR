using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;



public enum VoxelType {Air,Bed,Ground,Water,Null}
public enum VoxelState {Null,Obstacle,Character}

public class Voxel
{
    public VoxelType type;
    public VoxelState state;
    public VoxelData data;
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
    private Vector3Int[] directionVec = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };


    [SerializeField] private GameObject testGo;
    public Voxel Voxel(Vector3Int coord)
    {
        return voxels[coord.x][coord.z][coord.y];
    }
    public void VoxelsLoopY(int y,Action<Vector3Int> action)
    {
        for (int i = 0; i < fieldSize.x; i++)
            for (int j = 0; j < fieldSize.z; j++)
                action.Invoke(new Vector3Int(i, j, y));
    }
    public void VoxelsLoopXZ(Vector2Int coord2D,Action<Vector3Int> action)
    {
        for (int i = 0; i < fieldSize.y; i++)
            action.Invoke(new Vector3Int(coord2D.x, coord2D.y, i));
    }

    public void VoxelLoop(Action<Vector3Int> action)
    {
        for(int i = 0; i<fieldSize.x;i++)
            for(int j = 0; j<fieldSize.z;j++)
                for(int k = 0; k<fieldSize.y;k++)
                    action.Invoke(new Vector3Int(i, j, k));
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
            voxels[i] = new Voxel[fieldSize.z][];
            for (int j = 0; j < fieldSize.z; j++)
                voxels[i][j] = new Voxel[fieldSize.y];
        }
        

        foreach (Voxel[][] lineX in voxels)
            foreach (Voxel[] lineZ in lineX)
                lineZ[0].type = VoxelType.Bed;

        FloorCreate(randomList, floorList, voxelCoordQueue);
        VoxelLoop((coord) => 
        { 
            if(Voxel(coord).type == VoxelType.Bed)
                Instantiate(testGo,coord,Quaternion.identity);
        });

    }




    private void FloorCreate(List<Vector3Int> randomList,List<Vector3Int> floorList, Queue<Vector3Int> voxelCoordQueue)
    {
        for (int i = 0; i < floorCount; i++)
            for (int j = 0; j < fieldSize.y; j++)
            {
                randomList.Clear();
                voxelCoordQueue.Clear();
                VoxelsLoopY(j, (checkCoord) =>
                {
                    if (!floorList.Contains(checkCoord))
                        if (Voxel(checkCoord).type == VoxelType.Bed)
                            randomList.Add(checkCoord);
                });
                Vector3Int nextOriginCoord = randomList[Random.Range(0, randomList.Count)] + Vector3Int.up;
                Voxel(nextOriginCoord).type = VoxelType.Bed;
                voxelCoordQueue.Enqueue(nextOriginCoord);
                int nextFloorSize = (int)(randomList.Count * floorOffset);
                while (voxelCoordQueue.Count < nextFloorSize)
                {
                    Vector3Int origin = voxelCoordQueue.Dequeue();
                    SideCheck(origin,
                        (checkCoord) =>
                        {
                            Vector3Int checkPtUnder = checkCoord - Vector3Int.up;
                            if (Voxel(checkCoord).type != VoxelType.Bed)
                                if (Voxel(checkPtUnder).type == VoxelType.Bed)
                                    return true;
                            return false;
                        },
                        (checkCoord) =>
                        {
                            if (voxelCoordQueue.Count != nextFloorSize)
                            {
                                Voxel(checkCoord).type = VoxelType.Bed;
                                voxelCoordQueue.Enqueue(checkCoord);
                                floorList.Add(checkCoord);
                            }
                        });
                    voxelCoordQueue.Enqueue(origin);
                }
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
