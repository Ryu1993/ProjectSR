using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoordCheck
{
    public static void SideCheck3D(Vector3Int origin, Func<Vector3Int, bool> condtion, Action<Vector3Int> acceptAction)
    {
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                for (int k = -1; k <= 1; k++)
                {
                    if (i == 0 & j == 0 & k == 0)
                        continue;
                    Vector3Int checkCoord = origin + new Vector3Int(i, j, k);
                    if (condtion.Invoke(checkCoord))
                        acceptAction.Invoke(checkCoord);
                }
    }
    public static void SideCheck2D(Vector3Int origin, Func<Vector3Int, bool> condition, Action<Vector3Int> acceptAction = null, bool isDiagonalAllowed = true)
    {
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
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

    public static void SideCheck2D(Vector2Int origin, Func<Vector2Int, bool> condition, Action<Vector2Int> acceptAction = null, bool isDiagonalAllowed = true)
    {
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 & j == 0)
                    continue;
                if (!isDiagonalAllowed)
                    if (i != 0 & j != 0)
                        continue;
                Vector2Int checkCoord = origin + new Vector2Int(i,j);
                if (condition.Invoke(checkCoord))
                    acceptAction?.Invoke(checkCoord);
            }
    }



}
