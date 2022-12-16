using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    private Character order;
    private Transform orderTransform;
    private Transform attackOriginTransform;
    private AttackInfo curAttackInfo;
    private Dictionary<Vector2Int, Vector3Int> attackViewCoord = new Dictionary<Vector2Int, Vector3Int>();
    private Dictionary<Vector2Int, AreaView> attackViewArea = new Dictionary<Vector2Int, AreaView>();

    private Camera mainCam;
    private RaycastHit hit;
    [SerializeField] private LayerMask mask;
    private Func<Vector2Int, bool> selectCondition;
    private Func<Vector2Int, bool> attackCondition;
    private Vector2Int curSelectedPosition;


    Action testAction;
    public Character testorder;

    public void Update()
    {
        testAction?.Invoke();
    }

    public void TestSet()
    {
        if(Input.GetKeyDown(KeyCode.T))
            AttackInfoSet(testorder);
    }

    public void AttackInfoSet(Character order)
    {
        this.order = order;
        orderTransform = order.transform;
        curAttackInfo = order.CurAttackInfo;
        selectCondition = null;
        attackCondition = null;
        if(curAttackInfo.isVisionCheck)
        {
            selectCondition = (checkCoord) => 
            {
                if (FieldManager.Instance.surfaceDic.TryGetValue(checkCoord, out Vector3Int surfaceCoord))
                    return CoordCheck.VoxelRayCast(Vector3Int.RoundToInt(orderTransform.position), surfaceCoord + Vector3Int.up, curAttackInfo.selectVoxelMask);
                else
                    return false;
            };
        }
        if(curAttackInfo.isBlockable)
        {
            attackCondition = (checkCoord) =>
            {
                if (FieldManager.Instance.surfaceDic.TryGetValue(checkCoord, out Vector3Int surfaceCoord))
                    return CoordCheck.VoxelRayCast(Vector3Int.RoundToInt(attackOriginTransform.position), surfaceCoord + Vector3Int.up, curAttackInfo.attackVoxelMask);
                else
                    return false;
            };
        }
        AreaViewManager.Instance.AreaCoordSet(orderTransform.position.To2DInt(), curAttackInfo.selectRange, curAttackInfo.selectShape,null,selectCondition);

        
    }

    private void AttackSelectViewCall()
    {
        AreaViewManager.Instance.CallAreaViews();
        AreaViewManager.Instance.AreaViewDic.LoopDictionary((view) => view.SetType(TILE_TYPE.Enable));
        testAction = null;
        testAction += AttackViewCall;
        testAction += Attack;
    }


    private void AttackViewCall()
    {
        if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, mask))
            if(curSelectedPosition != hit.transform.position.To2DInt())
            {
                curSelectedPosition = hit.transform.position.To2DInt();
                if (attackViewArea.TryGetValue(curSelectedPosition, out AreaView targetView))
                {
                    AreaViewManager.Instance.AreaCoordSet(hit.transform.position.To2DInt(), curAttackInfo.attakcRange, curAttackInfo.attackShape, attackViewCoord, attackCondition);
                    AreaViewManager.Instance.CallAreaViews(attackViewCoord, attackViewArea);
                }
            }
    }

    private void Attack()
    {
        if(Input.GetMouseButtonDown(0))
        {
            AreaViewManager.Instance.ReturnAreaViews(attackViewArea);
            AreaViewManager.Instance.ReturnAreaViews();           
            FieldManager.Instance.surfaceDic.TryGetValue(curSelectedPosition, out Vector3Int target);

            MotionManager.Instance.Attack(order, target);


        }
    }


}
