using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MoveManager : MonoBehaviour
{
    //TweenValue//
    #region TweenValue
    private Tweener pathTween;
    private Vector3[] pathPoints = new Vector3[3];
    private float pathTime;
    private PathType pathType;
    #endregion
    //////////////

    private Transform orderTransform;
    private Character order;

    private FieldManager field;


    private void Awake()
    {
        //pathTween = orderTransform.DOPath(pathPoints, pathTime, pathType);
        //pathTween.SetAutoKill(false);
        field = FieldManager.Instance;
    }

    private void Update()
    {
        TestMove();


    }


    public Character test;
    private void TestMove()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            MoveOrderSet(test);
            CreateMoveAreas();
        }
            


    }


    private void MoveOrderSet(Character order)
    {
        this.order = order;
        orderTransform = order.transform;
        AreaViewManager.Instance.AreaCoordSet(orderTransform.position.To2DInt(), order.moveablePoint, FIELD_SHAPE.RangeLimitSquare);
    }
    
    private void CreateMoveAreas()
    {
        AreaViewManager.Instance.CallAreaViews();

        CoordCheck.SideCheck2D(orderTransform.position.To2DInt(),
            (checkCoord) =>
            {
                if (AreaViewManager.Instance.AreaViewDic.TryGetValue(checkCoord, out AreaView areaView))
                    return IsMoveableCheck(Vector3Int.RoundToInt(areaView.transform.position), order.moveablePoint,order.jumpableHeight, false);
                return false;
            },
            (checkCoord) =>
            {
                AreaViewManager.Instance.AreaViewDic[checkCoord].SetState(TILE_TYPE.Enable);
            },
            false
            );

    }


    private void NextPathSelect()
    {




    }


    //재귀를 이용해 이동 가능한 지역인지 판정
    private bool IsMoveableCheck(Vector3Int origin,int moveablePower ,int jumpableHeight, bool isDiagonalAllowed)
    {
        if (moveablePower <= 1)
            if (field.Voxel(origin).type != VoxelType.Ground)
                return false;
        bool isMoveable = false;
        Vector2Int origin2D = origin.To2DInt();
        CoordCheck.SideCheck2D(origin2D,
            (checkCoord) =>
            {
                if (!AreaViewManager.Instance.AreaCoordDic.TryGetValue(checkCoord, out Vector3Int areaCoord))
                    return false;
                if (Mathf.Abs(areaCoord.y - origin.y) > jumpableHeight)
                    return false;
                if(moveablePower>1)
                    return IsMoveableCheck(areaCoord, moveablePower - 1, jumpableHeight, isDiagonalAllowed);
                return true;
            },
            (none) => { isMoveable = true; },
            isDiagonalAllowed);
        return isMoveable;
    }




}
