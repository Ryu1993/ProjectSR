using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMove : MonoBehaviour
{
    private AreaView[,] moveArea;
    public FieldGenerator field;
    public Transform player;
    public int movePoint;
    private List<AreaView> selectedList = new List<AreaView>();
    private List<AreaView> nextAreaList = new List<AreaView>(4);
    public int jumpHeight;
    public LayerMask mask;
    public int moveCount;
    public Ease ease;


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            CreateMoveArea(player, movePoint);
            ActiveMoveArea(moveArea[movePoint,movePoint], movePoint);
            moveCount = movePoint;
        }
        MoveAreaSelect();


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
        Vector3 position = origin.transform.localPosition;
        position += new Vector3(movePoint, 0, movePoint);
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
                            view.SetColor(TILE_TYPE.Selected, ref view.curState);
                            selectedList.Add(view);
                            AreaDeActive();
                            ActiveMoveArea(view, movePoint);
                            moveCount--;
                        }
            }
            else
            {
                StartCoroutine(Move());
            }




    }
    public IEnumerator Move()
    {
        AreaDeActive();
        bool isComplete = false;
        //int passCount = 0;
        WaitUntil wait = new WaitUntil(() => isComplete);
        Vector3[] movePoints = new Vector3[selectedList.Count];
        Vector3[] wayPoints = new Vector3[3];
        for (int i = 0; i < movePoints.Length; i++)
            movePoints[i] = selectedList[i].transform.position + new Vector3(0, 0.1f, 0);
        InvisibleArea();
        for (int i=1; i < movePoints.Length; i++)
        {
            isComplete = false;
            float height = movePoints[i-1].y - movePoints[i].y;//이동할 칸이랑 높낮이 차이가 날 경우
            if(height!=0)
            {
                wayPoints[0] = movePoints[i - 1];
                wayPoints[1] = (movePoints[i - 1] + movePoints[i]) / 2 + new Vector3(0, height = height < 0 ? -height : height, 0);
                wayPoints[2] = movePoints[i];
                player.transform.DOPath(wayPoints,1,PathType.CatmullRom).SetEase(ease).OnComplete(() => isComplete = true);
            }
            //if (selectedList[i].curType == TILE_TYPE.Passable) // 수면 위를 지나갈경우
            //{

            //}
            else
                player.transform.DOMove(movePoints[i], 1).SetEase(ease).OnComplete(() => isComplete = true);
            yield return wait;
        }
        RemoveArea();
        selectedList.Clear();
        CreateMoveArea(player, movePoint);
        ActiveMoveArea(moveArea[movePoint, movePoint], movePoint);
        moveCount = movePoint;
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
    public void CrossCheck(Vector3 target, Func<Vector2Int,bool> condition,Action<Vector2Int> action)
    {
        Vector2Int checkPoint = new Vector2Int((int)target.x, (int)target.z);
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
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
        switch (checkPoint.curType)
        {
            case TILE_TYPE.Default:
                passable = true;
                break;
            case TILE_TYPE.Passable:
                {
                    CrossCheck(checkPoint.transform.localPosition+new Vector3(movePoint,0,movePoint), (target) => ActiveCheck(target, checkPoint,ActivePassableCheck), (temp) => passable = true);
                    break;
                }
        }
        return passable;
    }

    public bool ActivePassableCheck(AreaView checkPoint)
    {
        print(checkPoint.curType);
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
