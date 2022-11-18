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


    private static Vector2Int[] side = new Vector2Int[]
    {
        new Vector2Int(0,1),
        new Vector2Int(0,-1),
        new Vector2Int(1,0),
        new Vector2Int(-1,0)
    };



    public enum Direction { Middle,Top,Bottom,Up,Down,Left,Right}

    public static bool CubeRay(Vector3Int origin,Vector3Int target,CUBE_TYPE type)
    {
        Vector3 distance = target - origin;
        Direction leftRight = distance.x < 0 ? Direction.Left : distance.x > 0 ? Direction.Right : Direction.Middle;
        Direction upDown = distance.z < 0 ? Direction.Down : distance.z > 0 ? Direction.Up : Direction.Middle;
        Direction topBottom = distance.y <0? Direction.Bottom : distance.y > 0 ? Direction.Top : Direction.Middle;
        float x = distance.x < 0 ? -distance.x : distance.x;
        float y = distance.y < 0 ? -distance.y : distance.y;
        float z = distance.z < 0 ? -distance.z : distance.z;

        int end = 0;
        float addX = leftRight == Direction.Left ? -1 : leftRight == Direction.Right ? 1 : 0;
        float addY = topBottom == Direction.Top ? 1 : topBottom == Direction.Bottom ? -1 : 0;
        float addZ = upDown == Direction.Up? 1 : upDown == Direction.Bottom ? -1 : 0;
        if(x==y&y==z)
        {
            end = (int)x;
        }
        else if(x>=y&x>=z)
        {
            end = (int)x;
            addY = addY * y / x;
            addZ = addZ * z / x;
        }
        else if(y>=x&y>=z)
        {
            end = (int)y;
            addX = addX* x / y;
            addZ = addZ * z / y;
        }
        else if(z>=x&z>=y)
        {
            end = (int)z;
            addX = addX* x / z;
            addY = addY * y / z;
        }
        for(int i= 0; i<end;i++)
        {
            Vector3 check = new Vector3(addX, addY, addZ) * i;
            Vector3Int targetCoord = origin + check.ToInt();
            Debug.Log(targetCoord);
            if (field.Cube(targetCoord).type == type)
                return true;
        }
        return false;

    }


  



}
