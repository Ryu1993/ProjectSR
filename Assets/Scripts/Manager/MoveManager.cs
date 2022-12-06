using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Linq;
using static UnityEditor.ShaderData;
using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Validation;
using static UnityEngine.UI.Image;

public class MoveManager : MonoBehaviour
{
    //TweenValue//
    #region TweenValue
    private Vector3[] pathPoints = new Vector3[3];
    private float pathTime;
    private PathType pathType;
    #endregion
    //////////////

    private Transform orderTransform;
    private Character order;
    private int moveablePower;
    private int maxMoveablePower;
    private int jumpableHeight;
    private FIELD_SHAPE moveableFieldShape;
    private bool isDiagonalAllowed;


    private FieldManager field;
    

    private RaycastHit hit;
    private Camera mainCam;
    [SerializeField]private LayerMask areaViewMask;
    private List<AreaView> moveableAreaList = new List<AreaView>(4);
    private List<Vector3Int> selectedAreaCoordList = new List<Vector3Int>();


    private Action<Vector2Int> defaultAcceptAction;
    private Vector3 targetCoord;
    private bool isMoveable;
    private Dictionary<Vector2Int, Vector2Int> prevCoordDic = new Dictionary<Vector2Int, Vector2Int>();
    private List<Vector2Int> ditectPools = new List<Vector2Int>();

    private void Awake()
    {
        field = FieldManager.Instance;
        mainCam = Camera.main;
        defaultAcceptAction = (none) => { isMoveable = true; };
    }

    private void Update()
    {
        TestMove();


    }


    public Character test;
    private bool isTestCreate;
    private void TestMove()
    {
        //if (Input.GetKeyDown(KeyCode.K)&!isTestCreate)
        //{
        //    MoveOrderSet(test);
        //    CreateMoveAreas();
        //    isTestCreate = true;
        //}
        //if (isTestCreate)
        //    if (Input.GetMouseButtonDown(0))
        //        NextPathSelect();
        //if (selectedAreaCoordList.Count > 0)
        //{
        //    if(Input.GetKeyDown(KeyCode.L))
        //        StartCoroutine(OrderMove());
        //}

        if (Input.GetKeyDown(KeyCode.G))
            AutoMove(test);

            

    }

    private IEnumerator OrderMove()
    {
        if(AreaViewManager.Instance.AreaViewDic.Count!=0)
        {
            MoveableAreaListClear();
            AreaViewManager.Instance.ReturnAreaViews();
        }

        Voxel targetVoxel;
        bool isPass = false;
        for(int i = 0; i<selectedAreaCoordList.Count; i++)
        {
            targetVoxel = field.Voxel(selectedAreaCoordList[i]);
            if(targetVoxel.type !=VoxelType.Ground)
            {
                isPass = true;
                continue;
            }
            PathSetting(i, ref isPass);
            if ((orderTransform.position + orderTransform.forward).To2DInt() != selectedAreaCoordList[i].To2DInt())
                yield return orderTransform.DOLookAt(new Vector3(selectedAreaCoordList[i].x, orderTransform.position.y, selectedAreaCoordList[i].z), 0.3f);
            yield return orderTransform.DOPath(pathPoints, pathTime, pathType).WaitForCompletion();
        }
        selectedAreaCoordList.Clear();

        isTestCreate = false;
    }

    private void PathSetting(int i,ref bool isPass)
    {
        Vector3 orderPoint = orderTransform.position;
        pathPoints[2] = selectedAreaCoordList[i] + Vector3Int.up;
        Vector3 middlePoint = (orderPoint + pathPoints[2]) * 0.5f;
        int targetHeight = Mathf.RoundToInt(pathPoints[2].y);
        int orderHeight = Mathf.RoundToInt(orderPoint.y);
        int heightDistance = targetHeight - orderHeight;
        if (heightDistance > 0)
        {
            pathPoints[0] = pathPoints[2];
            pathPoints[1] = new Vector3(orderPoint.x, middlePoint.y, orderPoint.z);
            pathPoints[2] = new Vector3(middlePoint.x, pathPoints[0].y + 0.5f, middlePoint.z);
            pathType = PathType.CubicBezier;
            pathTime = 1f;
        }
        if (heightDistance < 0)
        {
            pathPoints[0] = (orderPoint + pathPoints[2]) / 2;
            pathPoints[1] = pathPoints[2];
            pathPoints[1].y = pathPoints[0].y;
            pathPoints[0].y = orderPoint.y;    
            pathType = PathType.CatmullRom;
            pathTime = 1f;
        }
        if (heightDistance == 0)
        {
            pathPoints[0] = orderPoint;
            if (isPass)
            {
                pathPoints[1] = middlePoint + Vector3.up * 0.5f;
                pathType = PathType.CatmullRom;
                isPass = false;
                pathTime = 1.5f;
            }
            else
            {
                pathPoints[1] = orderPoint;
                pathType = PathType.Linear;
                pathTime = 0.6f;
            }
        }
    }


    private void MoveOrderSet(Character order)
    {
        Monster monsterOrder = order as Monster;
        targetCoord = monsterOrder ? monsterOrder.target.transform.position : Vector3.zero;
        this.order = order;        
        orderTransform = order.transform;
        jumpableHeight = order.jumpableHeight;
        maxMoveablePower = order.moveablePoint;
        isDiagonalAllowed = order.isDiagonalMoveAllowed;
        moveableFieldShape = isDiagonalAllowed ? FIELD_SHAPE.Square : FIELD_SHAPE.RangeLimitSquare;
        moveablePower = maxMoveablePower;
        AreaViewManager.Instance.AreaCoordSet(orderTransform.position.To2DInt(),maxMoveablePower, moveableFieldShape);
    }
    
    private void CreateMoveAreas()
    {
        AreaViewManager.Instance.CallAreaViews();
        MoveableAreaSet(Vector3Int.RoundToInt(orderTransform.position+Vector3.down),isDiagonalAllowed);
    }

    private void NextPathSelect()
    {
        if (moveablePower < 1)
            return;
        if (Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, areaViewMask))
        {
            if (!hit.transform.parent.TryGetComponent(out AreaView hitAreaView))
                return;

            moveablePower--;
            hitAreaView.SetState(TILE_TYPE.Selected);
            Vector3Int hitAreaViewCoord = Vector3Int.RoundToInt(hitAreaView.transform.position);
            selectedAreaCoordList.Add(hitAreaViewCoord);
            MoveableAreaListClear();
            if(moveablePower>0)
                MoveableAreaSet(hitAreaViewCoord,isDiagonalAllowed);
            InputManager.Instance.CancleBehaviour.Push(NextPathSelectCancle);
        }
    }

    private void NextPathSelectCancle()
    {
        moveablePower++;
        MoveableAreaListClear();
        AreaView lastSelecterdArea = AreaViewManager.Instance.AreaViewDic[selectedAreaCoordList.Last().To2DInt()];
        lastSelecterdArea.SetState(lastSelecterdArea.curType);
        selectedAreaCoordList.RemoveAt(selectedAreaCoordList.Count - 1);
        if (selectedAreaCoordList.Count == 0)
            MoveableAreaSet(Vector3Int.RoundToInt(orderTransform.position + Vector3.down), isDiagonalAllowed);
        else
            MoveableAreaSet(selectedAreaCoordList.Last(), isDiagonalAllowed);
    }


    private void MoveableAreaListClear()
    {
        foreach (var area in moveableAreaList)
            if (area.curState != TILE_TYPE.Selected)
                area.SetState(area.curType);
        moveableAreaList.Clear();
    }


    private void MoveableAreaSet(Vector3Int originCoord, bool isDiagonalAllowed = false)
    {
        Vector2Int originCoord2D = originCoord.To2DInt();
        CoordCheck.SideCheck2D(originCoord2D,
            (checkCoord) =>
            {
                if (AreaViewManager.Instance.AreaViewDic.TryGetValue(checkCoord, out AreaView areaView))
                    if(Mathf.Abs(areaView.transform.position.y-originCoord.y)<=jumpableHeight)
                        return IsMoveableCheck(Vector3Int.RoundToInt(areaView.transform.position), moveablePower, jumpableHeight, isDiagonalAllowed);
                return false;
            },
            (checkCoord) =>
            {
                AreaView targekAreaView = AreaViewManager.Instance.AreaViewDic[checkCoord];
                if(targekAreaView.curState!=TILE_TYPE.Selected)
                {
                    targekAreaView.SetState(TILE_TYPE.Enable);
                    moveableAreaList.Add(targekAreaView);
                }
            },
            false
            );
    }

    //재귀를 이용해 이동 가능한 지역인지 판정
    private bool IsMoveableCheck(Vector3Int origin,int moveablePower ,int jumpableHeight, bool isDiagonalAllowed,Action<Vector2Int> acceptAction = null)
    {
        Vector2Int origin2D = origin.To2DInt();
        isMoveable = false;
        if (acceptAction == null)
            acceptAction = defaultAcceptAction;
        if (moveablePower <= 1)
            if (field.Voxel(origin).type != VoxelType.Ground)
                return false;
        CoordCheck.SideCheck2D(origin2D,
            (checkCoord) =>
            {
                if (!AreaViewManager.Instance.AreaCoordDic.TryGetValue(checkCoord, out Vector3Int areaCoord))
                    return false;
                if (Mathf.Abs(areaCoord.y - origin.y) > jumpableHeight)
                    return false;
                if(moveablePower>1)
                    return IsMoveableCheck(areaCoord, moveablePower - 1, jumpableHeight, isDiagonalAllowed,acceptAction);
                return true;
            },
            acceptAction,
            isDiagonalAllowed);
        return isMoveable;
    }

    



    private void AutoMove(Character character)
    {
        MoveOrderSet(character);
        PathFinding();
        StartCoroutine(OrderMove());
    }

   



  



    private void PathFinding()
    {
        prevCoordDic.Clear();
        ditectPools.Clear();
        Vector2Int orderPosition2D = orderTransform.position.To2DInt();
        Vector2Int targetCoord2D = targetCoord.To2DInt();
        ditectPools.Add(orderPosition2D);
        prevCoordDic.Add(orderPosition2D, orderPosition2D);
        while(ditectPools.Count > 0)
        {
            Vector2Int curDitectCoord = ditectPools.PriorityPop(targetCoord2D);
            if (curDitectCoord == targetCoord2D)
                break;
            int curCoordHeight = field.surfaceDic[curDitectCoord].y;
            CoordCheck.SideCheck2D(curDitectCoord,
                (checkCoord) =>
                {
                    if (!field.surfaceDic.TryGetValue(checkCoord, out Vector3Int surfaceCoord))
                        return false;
                    if (Mathf.Abs(surfaceCoord.y - curCoordHeight) > jumpableHeight)
                        return false;
                    if (field.Voxel(surfaceCoord + Vector3Int.up) == null | field.Voxel(surfaceCoord + Vector3Int.up)?.type != VoxelType.Air)
                        return false;
                    return true;
                }, 
                (checkCoord) =>
                {
                    if (prevCoordDic.TryGetValue(checkCoord, out Vector2Int prev))
                    {
                        if (PathCost(checkCoord) <= PathCost(curDitectCoord) + 1)
                            return;
                        else
                        {
                            prevCoordDic[checkCoord] = curDitectCoord;
                            if (!ditectPools.Contains(checkCoord))
                                ditectPools.Add(checkCoord);
                        }
                    }
                    else
                    {
                        prevCoordDic.Add(checkCoord, curDitectCoord);
                        if (!ditectPools.Contains(checkCoord))
                            ditectPools.Add(checkCoord);
                    }
                },
                isDiagonalAllowed);
        }

        Vector2Int moveTarget = targetCoord2D;
        int curCost = 0;
        prevCoordDic.LoopDictionary((pair) =>
        {
            int pathCost = PathCost(pair.Key);
            if (pathCost <= moveablePower & pathCost > curCost)
            {
                moveTarget = pair.Key;
                curCost = pathCost;
            }             
        });

        selectedAreaCoordList.Clear();
        while (prevCoordDic[moveTarget]!=moveTarget)
        {
            selectedAreaCoordList.Add(field.surfaceDic[moveTarget]);
            moveTarget = prevCoordDic[moveTarget];
        }
        selectedAreaCoordList.Reverse();
    }

    private int PathCost(Vector2Int checkCoord)
    {
        int count = 0;
        while (prevCoordDic[checkCoord]!=checkCoord)
        {
            checkCoord = prevCoordDic[checkCoord];
            count++;
        }
        return count;
    }





}
