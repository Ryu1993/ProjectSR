using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CHECK_TYPE
{
    BOTH, CROSS, SIDE
}
public static class CubeCheck
{
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


}
