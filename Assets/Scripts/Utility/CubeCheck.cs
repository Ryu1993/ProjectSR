using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public enum CHECK_TYPE
{
    BOTH, CROSS, SIDE
}




public static class CubeCheck
{
    private static FieldGenerator field { get { return FieldGenerator.Instance; } }

    public static void CustomCheck(CHECK_TYPE type, Vector2Int origin, int range, Func<Vector2Int, bool> condition, Action<Vector2Int> trueAction)
    {
        bool isTypeCheck;
        for (int i = -range; i <= range; i++)
            for (int j = -range; j <= range; j++)
            {
                switch (type)
                {
                    case CHECK_TYPE.CROSS:
                        isTypeCheck = (i != 0 & j != 0);
                        break;
                    default:
                        isTypeCheck = false;
                        break;
                }
                if (isTypeCheck) continue;
                if (i == 0 & j == 0) continue;

                Vector2Int checkPoint = new Vector2Int(i, j) + origin;

                if (condition.Invoke(checkPoint))
                    trueAction.Invoke(checkPoint);
            }
    }
    public static void DiagonalCheck(Vector2Int origin,int range, Func<Vector2Int, bool> condition, Action<Vector2Int> trueAction)
    {
        for (int i = -range; i <= range; i++)
        {
            if (i == 0) continue;
            Vector2Int checkPoint = new Vector2Int(i, -i) + origin;
            Vector2Int symmetryPoint = new Vector2Int(i,i)+ origin;
            if (condition.Invoke(checkPoint))
                trueAction.Invoke(checkPoint);
            if (condition.Invoke(symmetryPoint))
                trueAction.Invoke(symmetryPoint);
        }

    }


    public static bool CubeRay(Vector3Int origin,Vector3Int target)
    {
        return CubeRay(origin, target, (targetCoord) => field.Cube(targetCoord).type != CUBE_TYPE.Air);
    }

    public static bool CubeRay(Vector3Int origin,Vector3Int target,CUBE_TYPE type)
    {
        return CubeRay(origin, target, (targetCoord) => field.Cube(targetCoord).type == type);
    }

    public static bool CubeRay(Vector3Int origin,Vector3Int target, CUBE_TYPE[] types)
    {
        return CubeRay(origin, target, (targetCoord) => 
        {
            foreach (var type in types)
            {
                if (field.Cube(targetCoord).type == type)
                    return true;
            }
            return false;
        });
    }

    private static bool CubeRay(Vector3Int origin,Vector3Int target,Func<Vector3Int,bool> typecheck)
    {
        Vector3 distance = target - origin;
        float x = distance.x < 0 ? -distance.x : distance.x;
        float y = distance.y < 0 ? -distance.y : distance.y;
        float z = distance.z < 0 ? -distance.z : distance.z;
        float addX = distance.x < 0 ? -1 : distance.x > 0 ? 1 : 0;
        float addY = distance.y < 0 ? -1 : distance.y > 0 ? 1 : 0;
        float addZ = distance.z < 0 ? -1 : distance.z > 0 ? 1 : 0;
        float end = 0;
        if (x==y&y==z)
        {
            end = x;
        }
        else if(x>=y&x>=z)
        {
            end = x;
            addY = addY * y / x;
            addZ = addZ * z / x;
        }
        else if(y>=x&y>=z)
        {
            end = y;
            addX = addX* x / y;
            addZ = addZ * z / y;
        }
        else if(z>=x&z>=y)
        {
            end = z;
            addX = addX* x / z;
            addY = addY * y / z;
        }
        for(float i= 0; i<end;i+=0.1f)
        {
            Vector3 check = new Vector3(addX, addY, addZ) * i;
            Vector3Int targetCoord = origin + check.ToInt();
            if (targetCoord == origin)
                continue;
            if (typecheck.Invoke(targetCoord))
                return true;
        }
        return false;
    }


  



}
