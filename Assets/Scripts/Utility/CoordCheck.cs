using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class CoordCheck
{
    private static VoxelType[] defaultCheckType = new VoxelType[] { VoxelType.Ground };
    private static int[] xyz = new int[3];
    private static List<Vector3Int> crashCoords = new List<Vector3Int>(15);


    public static void SideCheck3D(Vector3Int origin,Func<Vector3Int, bool> condtion, Action<Vector3Int> acceptAction = null,bool isOriginInclude = false)
    {
        CustomSideCheck3D(origin, 1, condtion, acceptAction, isOriginInclude);
    }
    public static void SideCheck2D(Vector3Int origin, Func<Vector3Int, bool> condition, Action<Vector3Int> acceptAction = null, bool isDiagonalAllowed = true)
    {
        CustomSideCheck2D(origin, 1, condition, acceptAction, isDiagonalAllowed);
    }

    public static void SideCheck2D(Vector2Int origin, Func<Vector2Int, bool> condition, Action<Vector2Int> acceptAction = null, bool isDiagonalAllowed = true)
    {
        CustomSideCheck2D(origin, 1, condition, acceptAction, isDiagonalAllowed);
    }

    public static void CustomSideCheck3D(Vector3Int origin, int range,Func<Vector3Int, bool> condtion, Action<Vector3Int> acceptAction = null, bool isOriginInclude = false)
    {
        for (int i = -range; i <= range; i++)
            for (int j = -range; j <= range; j++)
                for (int k = -range; k <= range; k++)
                {
                    if (!isOriginInclude)
                        if (i == 0 & j == 0 & k == 0)
                            continue;
                    Vector3Int checkCoord = origin + new Vector3Int(i, j, k);
                    if (condtion.Invoke(checkCoord))
                        acceptAction?.Invoke(checkCoord);
                }
    }
    public static void CustomSideCheck2D(Vector3Int origin, int range, Func<Vector3Int, bool> condition, Action<Vector3Int> acceptAction = null, bool isDiagonalAllowed = true)
    {
        for (int i = -range; i <= range; i++)
            for (int j = -range; j <= range; j++)
            {
                if (i == 0 & j == 0)
                    continue;
                if (!isDiagonalAllowed)
                    if (i != 0 & j != 0)
                        continue;
                Vector3Int checkCoord = origin + new Vector3Int(i, 0, j);
                if (condition.Invoke(checkCoord))
                    acceptAction?.Invoke(checkCoord);
            }
    }

    public static void CustomSideCheck2D(Vector2Int origin,int range,Func<Vector2Int, bool> condition, Action<Vector2Int> acceptAction = null, bool isDiagonalAllowed = true)
    {
        for (int i = -range; i <= range; i++)
            for (int j = -range; j <= range; j++)
            {
                if (i == 0 & j == 0)
                    continue;
                if (!isDiagonalAllowed)
                    if (i != 0 & j != 0)
                        continue;
                Vector2Int checkCoord = origin + new Vector2Int(i, j);
                if (condition.Invoke(checkCoord))
                    acceptAction?.Invoke(checkCoord);
            }
    }


    public static bool VoxelRayCast(Vector3Int origin,Vector3Int target, VoxelType[] checkTypes =null)
    {
        crashCoords.Clear();
        bool isCrash = false;
        checkTypes = checkTypes ?? defaultCheckType;
        Vector3Int direction = target - origin;
        xyz[0] = Mathf.Abs(direction.x);
        xyz[1] = Mathf.Abs(direction.y);
        xyz[2] = Mathf.Abs(direction.z);
        float longestLengt = Mathf.Max(xyz);
        float x =xyz[0] / longestLengt;
        float y =xyz[1] / longestLengt;
        float z =xyz[2] / longestLengt;
        Vector3 checkDistance = new Vector3(x, y, z);
        for(float i = 0; i < longestLengt; i+=0.1f)
        {
            Vector3Int checkCoord = Vector3Int.RoundToInt(origin + checkDistance * i);
            if (checkCoord == origin | checkCoord == target)
                continue;
            foreach(VoxelType checkType in checkTypes)
                if (FieldManager.Instance.Voxel(checkCoord)?.type == checkType)
                {
                    isCrash = true;
                    if (!crashCoords.Contains(checkCoord))
                        crashCoords.Add(checkCoord);
                }
        }
        return isCrash;
    }
    public static bool VoxelRayCast(Vector3Int origin,Vector3Int target,out List<Vector3Int> resultCrashCoords, VoxelType[] checkTypes = null )
    {
        resultCrashCoords = crashCoords;
        return VoxelRayCast(origin,target,checkTypes);
    }





}
