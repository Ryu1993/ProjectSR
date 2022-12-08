using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public abstract class CurveMotionPlayer : MotionPlayer
{
    protected Vector3[] ways = new Vector3[3];
    protected void WayPointSet(Vector3Int orderPosition, Vector3Int targetPosition)
    {
        orderPosition += Vector3Int.up;
        CoordCheck.VoxelRayCast(orderPosition, targetPosition, out List<Vector3Int> obstacles);
        ways[0] = targetPosition;
        Vector3Int maxHeight = Mathf.Max(targetPosition.y, orderPosition.y) == targetPosition.y ? targetPosition : orderPosition;
        Vector3 middle = targetPosition - orderPosition;
        if (obstacles.Count != 0)
        {
            foreach (var obstacle in obstacles)
            {
                if (obstacle == targetPosition) continue;
                if (maxHeight.y <= obstacle.y)
                    maxHeight = obstacle;
            }
            float distance = Vector3Int.Distance(orderPosition, maxHeight);
            middle = middle.normalized;
            middle = orderPosition + middle * distance;
            middle.y = maxHeight.y;
        }
        else
        {
            middle = orderPosition + targetPosition;
            middle *= 0.5f;
        }
        ways[1] = (orderPosition + middle) * 0.5f;
        ways[2] = (targetPosition + middle) * 0.5f;
        ways[1].y += maxHeight.y;
        ways[2].y += maxHeight.y;
    }

}
