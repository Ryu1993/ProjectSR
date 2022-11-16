using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using JetBrains.Annotations;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private LayerMask mask;
    private List<AreaView> selectedList = new List<AreaView>();
    private List<AreaView> nextAreaList = new List<AreaView>(4);
    private Vector3[] wayPoints = new Vector3[4];
    private Coroutine moveProgress;
    private AreaView[,] moveArea;
    private FieldGenerator field;
    private int moveCount;


    public Transform order;
    public int movePoint;
    public int jumpHeight;


    public void Awake()
    {
        field = FieldGenerator.Instance;
    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            moveCount = movePoint;
            CreateMoveArea(order, movePoint);
            ActiveMoveArea(moveArea[movePoint,movePoint], movePoint);
        }
        if(moveArea!=null)
            MoveAreaSelect();
        if (moveProgress == null)
            MoveProgressing();
            
    }


    public void RemoveArea()
    {
        for (int i = 0; i < moveArea.GetLength(0); i++)
            for (int j = 0; j < moveArea.GetLength(1); j++)
            {
                moveArea[i, j]?.Return();
                moveArea[i, j] = null;
            }
    }

    public void InvisibleArea()
    {
        for (int i = 0; i < moveArea.GetLength(0); i++)
            for (int j = 0; j < moveArea.GetLength(1); j++)
                moveArea[i, j]?.Invisible();
    }


    public void CreateMoveArea(Transform player,int movePoint)
    {
        transform.position = player.position;
        Vector3Int playerPos = player.position.ConvertInt() - new Vector3Int(movePoint, 0, movePoint);
        moveArea = new AreaView[movePoint * 2 + 1, movePoint * 2 + 1];
        for(int i=0; i< movePoint * 2 + 1;i++)
            for (int j = 0; j < movePoint * 2 + 1; j++)
            {
                if (i == movePoint & j == movePoint)
                {
                    moveArea[i, j] = AreaViewManager.Instance.CallAreaView(playerPos + new Vector3(i, 0.1f, j), transform);
                    moveArea[i, j].SetColor(TILE_TYPE.Selected, ref moveArea[i, j].curType);
                    selectedList.Add(moveArea[i, j]);
                    continue;
                }
                int x = movePoint - i < 0 ? -(movePoint - i) : movePoint - i;
                int z = movePoint - j < 0 ? -(movePoint - j) : movePoint - j;
                if (x + z > movePoint) continue;
                CheckMoveArea(playerPos + new Vector3Int(i, 0, j),out moveArea[i,j]);
            }
    }

    public void CheckMoveArea(Vector3Int target,out AreaView areaCoord)
    {
        areaCoord = null;
        if(field.Cube(target).type == CUBE_TYPE.Out)
        {
            areaCoord = AreaViewManager.Instance.CallAreaView(target+new Vector3(0, 0.1f, 0), transform);
            areaCoord.SetColor(TILE_TYPE.Disable, ref areaCoord.curType);
            return;
        }
        for (int i = 0; i < field.size.y+1; i++)
        {
            Vector3Int checkCoord = new Vector3Int(target.x, i, target.z);
            CUBE_TYPE type = field.Cube(checkCoord).type;
            if (type == CUBE_TYPE.Air | type == CUBE_TYPE.Obstacle|type == CUBE_TYPE.Out)
            {
                areaCoord = AreaViewManager.Instance.CallAreaView(checkCoord + new Vector3(0, 0.1f, 0), transform);
                if (type == CUBE_TYPE.Obstacle)
                    areaCoord.SetColor(TILE_TYPE.Disable,ref areaCoord.curType);
                if (field.Cube(checkCoord - Vector3Int.up).type == CUBE_TYPE.Water)
                    areaCoord.SetColor(TILE_TYPE.Passable,ref areaCoord.curType);
                break;
            }
        }
    }

    public void ActiveMoveArea(AreaView origin, int movePoint)
    {
        Vector3 position = origin.transform.localPosition+ new Vector3(movePoint, 0, movePoint);
        CrossCheck(position, (target) => ActiveCheck(target, origin, ActiveMoveableCheck), AreaActivation);
    }


    public void MoveAreaSelect()
    {
        if(Input.GetMouseButtonDown(0))
            if(moveCount!=0)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, mask))
                    if (hit.transform.TryGetComponent(out AreaView view))
                        if (view.curState == TILE_TYPE.Enable)
                        {
                            moveCount--;
                            view.SetColor(TILE_TYPE.Selected, ref view.curState);
                            selectedList.Add(view);
                            AreaDeActive();
                            if(moveCount!=0)
                                ActiveMoveArea(view, movePoint);
                        }
            }
        if(Input.GetMouseButtonDown(1))
            if(selectedList.Count>1)
            {
                moveCount++;
                AreaView lastView = selectedList[selectedList.Count-1];
                lastView.SetColor(lastView.curType, ref lastView.curState);
                selectedList.Remove(lastView);
                AreaDeActive();
                ActiveMoveArea(selectedList[selectedList.Count - 1], movePoint);
            }
    }


    public void MoveProgressing()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            if (selectedList[selectedList.Count - 1].curType == TILE_TYPE.Passable)
                return;
            else
                moveProgress = StartCoroutine(Move());
    }
    public IEnumerator Move()
    {
        AreaDeActive();
        bool isComplete = false;
        bool isPass = false;
        WaitUntil moveDelay = new WaitUntil(() => isComplete);
        InvisibleArea();
        Vector3 prevPoint = selectedList[0].transform.position;
        PathType pathType = PathType.Linear;
        for(int i=1;i<selectedList.Count;i++)
        {
            isComplete = false;
            if (selectedList[i].curType == TILE_TYPE.Passable)
            {
                isPass = true;
                continue;
            }

            /////////////////////이동경로 초기화///////////////////
            for(int j =0; j<wayPoints.Length;j++)
                wayPoints[j] = selectedList[i].transform.position;
            wayPoints[0] = prevPoint;
            //////////////////////////////////////////////////////

            /////////////////////이동경로 설정/////////////////////
            float height = wayPoints[3].y - wayPoints[0].y;
            if (height > 0)
                wayPoints[1] = (wayPoints[3] + wayPoints[0]) / 2 + new Vector3(0, height / 2 + 0.5f, 0);
            else if (height < 0)
            {
                Vector3 temp = wayPoints[3] - wayPoints[0];
                wayPoints[1] = wayPoints[0] + new Vector3(temp.x = temp.x > 1 ? 0.5f : 0, 0, temp.z = temp.z > 1 ? 0.5f : 0);
                wayPoints[2] = new Vector3(wayPoints[3].x, wayPoints[0].y - 0.3f, wayPoints[3].z);
            }                
            else if (isPass)
                wayPoints[1] = (wayPoints[3] + wayPoints[0]) / 2 + new Vector3(0, 0.2f, 0);
            //////////////////////////////////////////////////////

            ///////////////////이동방식 설정///////////////////////
            if (height!=0|isPass)
                pathType = PathType.CatmullRom;
            else
                pathType = PathType.Linear;
            //////////////////////////////////////////////////////

            ////////////이동(이동 완료시 prevPoint재설정////////////
            order.transform.DOPath(wayPoints, 0.5f, pathType).OnComplete(() => { isComplete = true;isPass = false;prevPoint = wayPoints[3]; });
            yield return moveDelay;
        }
        RemoveArea();
        selectedList.Clear();


        moveCount = movePoint;
        CreateMoveArea(order, movePoint);
        ActiveMoveArea(moveArea[movePoint, movePoint], movePoint);
        moveProgress = null;
    }





    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////           ActiveMoveArea Method           ///////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region ActiveMoveAreaPart
    /// <summary>
    /// 십자모양 체크
    /// </summary>
    /// <param name="target">목표좌표</param>
    /// <param name="condition">조건</param>
    /// <param name="action">실행</param>
    public void CrossCheck(Vector3 target, Func<Vector2Int, bool> condition, Action<Vector2Int> action) => CustomCrossCheck(target, condition, action, 1);

    public void CustomCrossCheck(Vector3 target, Func<Vector2Int, bool> condition, Action<Vector2Int> action,int range)
    {
        Vector2Int checkPoint = new Vector2Int((int)target.x, (int)target.z);
        for (int i = -range; i <= range; i++)
            for (int j = -range; j <= range; j++)
            {
                if (i == 0 & j == 0) continue;
                if (i != 0 & j != 0) continue;
                if (condition.Invoke(checkPoint + new Vector2Int(i, j)))
                {
                    action.Invoke(checkPoint + new Vector2Int(i, j));
                }
            }
    }

    /// <summary>
    /// null체크,중복체크,높이체크 조건식 custom에 추가 조건식 추가
    /// </summary>
    /// <param name="target"></param>
    /// <param name="prev"></param>
    /// <param name="custom">추가 조건식</param>
    /// <returns></returns>
    public bool ActiveCheck(Vector2Int target, AreaView prev,Func<AreaView,bool> custom)
    {
        try
        {
            AreaView checkPoint = moveArea[target.x, target.y];
            if (checkPoint != null)
                if (checkPoint != prev)
                    if (!selectedList.Contains(checkPoint))
                    {
                        float height = prev.transform.localPosition.y - checkPoint.transform.localPosition.y;
                        if (height >= -jumpHeight & height <= jumpHeight)
                            return custom(checkPoint);
                    }
            return false;
        }
        catch (IndexOutOfRangeException)
        {
            return false;
        }

    }

    public bool ActiveMoveableCheck(AreaView checkPoint)
    {
        bool passable = false;
        if (checkPoint.curType == TILE_TYPE.Default)
            passable = true;
        if(checkPoint.curType == TILE_TYPE.Passable)
            if(moveCount>0)
            {
                AreaView zeroPoint = checkPoint;
                Vector3 zeroPointPosition = checkPoint.transform.localPosition + new Vector3(movePoint, 0, movePoint);
                CustomCrossCheck(zeroPointPosition, (checkTarget) => ActiveCheck(checkTarget, zeroPoint, ActivePassableCheck), (none) => passable = true,moveCount-1);
            }        
        return passable;               
    }

    public bool ActivePassableCheck(AreaView checkPoint)
    {
        if (checkPoint.curType == TILE_TYPE.Default)
            return true;
        return false;
    }


    public void AreaActivation(Vector2Int target)
    {
        AreaView activeArea = moveArea[target.x, target.y];
        nextAreaList.Add(activeArea);
        activeArea.SetColor(TILE_TYPE.Enable, ref activeArea.curState);
    }

    public void AreaDeActive()
    {
        foreach(var area in nextAreaList)
            if(area.curState!=TILE_TYPE.Selected)
                area.SetColor(area.curType, ref area.curState);
        nextAreaList.Clear();
    }

    #endregion






}
