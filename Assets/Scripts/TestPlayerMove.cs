using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestPlayerMove : MonoBehaviour
{
    public Transform player;
    public FieldGenerator field;
    public int movePoint;
    public int jumpHeight;

    List<Vector3Int> moveablePoint = new List<Vector3Int>();
    List<AreaView> curAreaView = new List<AreaView>(24);

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            Create();
            //MoveablePoint(player.position.ConvertInt());
            CreateAreaView();
        }


    }

    public void MoveablePoint(Vector3Int playerPosition)
    {
        moveablePoint.Clear();
        int upCount = 0;
        int downCount = 0;
        for(int i=-movePoint;i<=movePoint;i++)
        {
            for(int j=-movePoint;j<=movePoint;j++)
            {
                if (i == 0 & j == 0) continue;
                int x = i < 0 ? x = -i : x = i;
                int z = j < 0 ? z = -j : z = j;
                if(x+z<=movePoint)
                    if (MoveableCheck(playerPosition + new Vector3Int(i, 0, j),out Vector3Int next))
                    {
                        moveablePoint.Add(next);
                        if (next.y > playerPosition.y)
                            upCount++;
                        if (next.y < playerPosition.y)
                            downCount++;
                    }                 
            }
        }
        Span<Vector3Int> moveableSpanUp = stackalloc Vector3Int[upCount];
        Span<Vector3Int> moveableSpanDown = stackalloc Vector3Int[downCount];
        upCount = 0;
        downCount = 0;
        foreach (var moveable in moveablePoint)
        {
            if (moveable.y > playerPosition.y)
            {
                moveableSpanUp[upCount] = moveable;
                upCount++;
            }
            if (moveable.y < playerPosition.y)
            {
                moveableSpanDown[downCount] = moveable;
                downCount++;
            }
        }
        JumpableCheck(ref moveableSpanUp, true);
        JumpableCheck(ref moveableSpanDown, false);

        //Cost();
    }


    Vector3Int[] checkList = new Vector3Int[]
    {
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(0,0,1),
        new Vector3Int(0,0,-1)
    };

    public void Create()
    {
        Vector3Int playePosition = player.position.ConvertInt();
        foreach(var check in checkList)
        {
            for(int i=1; i<=movePoint;i++)
            {
                if (MoveableCheck(playePosition + check * i, out Vector3Int moveable))
                {
                    for (int j = 0; j < movePoint - i; j++)
                        if (check.x == 0)
                        {
                            if (MoveableCheck(moveable + checkList[0] * j, out Vector3Int connetable))
                                InputMoveable(connetable);
                            else
                                break;
                        }
                        else
                        {
                            if (MoveableCheck(moveable + checkList[2] * j, out Vector3Int connetable))
                                InputMoveable(connetable);
                            else
                                break;

                        }
                    for (int j = 0; j < movePoint - i; j++)
                        if (check.x == 0)
                        {
                            if (MoveableCheck(moveable + checkList[1] * j, out Vector3Int connetable))
                                InputMoveable(connetable);
                            else
                                break;
                        }
                        else
                        {
                            if (MoveableCheck(moveable+ checkList[3] * j, out Vector3Int connetable))
                                InputMoveable(connetable);
                            else
                                break;

                        }
                }
                else
                    break;        
            }
        }
    }



    public void InputMoveable(Vector3Int target)
    {
        if (!moveablePoint.Contains(target))
            moveablePoint.Add(target);
    }




    public bool MoveableCheck(Vector3Int position,out Vector3Int moveable)
    {
        CUBE_TYPE type = field.Cube(position).type;
        moveable = position;
        if (type == CUBE_TYPE.Air | type == CUBE_TYPE.Bed|type == CUBE_TYPE.Null)
            for (int i = 0; i < field.size.y+1; i++)
            {
                Vector3Int checkPoint = new Vector3Int(position.x, i, position.z);
                if (field.Cube(checkPoint).type == CUBE_TYPE.Ground)
                {
                    moveable = checkPoint;
                    type = CUBE_TYPE.Ground;
                    break;
                }                    
            }
        if(type == CUBE_TYPE.Ground & field.Cube(moveable + Vector3Int.up).type != CUBE_TYPE.Obstacle)
            return true;
        return false;
    }


    public void JumpableCheck(ref Span<Vector3Int> checkPoint,bool isUP)
    {
        SpanSort(ref checkPoint, isUP);
        int updown = isUP ? -1 : 1;
        for (int i = 0; i < checkPoint.Length; i++)
            Jumpable(ref checkPoint[i], jumpHeight, updown);
    }


    public void Jumpable(ref Vector3Int point,int jumpHeight,int isUp)
    {
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 & j == 0) continue;
                if (i != 0 & j != 0) continue;
                for (int k = 0; k <= jumpHeight; k++)
                    if (moveablePoint.Contains(point + new Vector3Int(i, k * isUp, j)))
                        return;
            }
        moveablePoint.Remove(point);
    }

    public void SpanSort(ref Span<Vector3Int> span,bool isUP)
    {
        Span<Vector3Int> copy = stackalloc Vector3Int[span.Length];
        span.CopyTo(copy);
        for (int i = 0; i < copy.Length; i++)
        {
            Vector3Int temp = copy[i];
            int count = i;
            for (int j = i + 1; j < copy.Length; j++)
                if(isUP)
                {
                    if (temp.y > copy[j].y)
                    {
                        temp = copy[j];
                        count = j;
                    }
                }
                else
                {
                    if (temp.y < copy[j].y)
                    {
                        temp = copy[j];
                        count = j;
                    }
                }
            copy[count] = copy[i];
            copy[i] = temp;
        }
        span.Clear();
        copy.CopyTo(span);
    }


    public void CostCheck(Vector3Int target,Vector3Int origin,int distance,Queue<Vector3Int> candidates)
    {
        for(int i = -1;i<=1;i++)
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 & j == 0) continue;
                if (i != 0 & j != 0) continue;
                Vector3Int temp = Vector3Int.zero;

                //검색 지점이 이동 가능한지 체크
                bool contain = false;
                for(int k=0;k<field.size.y;k++)
                    if(moveablePoint.Contains(target+new Vector3Int(i,k,j)))
                    {
                        contain = true;
                        temp = target+new Vector3Int(i,k,j);
                    }
                if (!contain) continue;
                int x = temp.x - origin.x;
                int z = temp.z - origin.z;
                x = x < 0 ? -x : x;
                z = z < 0 ? -z : z;
                if(x+z<distance)
                {
                    candidates.Enqueue(temp);
                }
            }

    }


    public void Cost()
    {
        Span<Vector3Int> span = stackalloc Vector3Int[moveablePoint.Count];
        Queue<Vector3Int> candidates = new Queue<Vector3Int>();
        Vector3Int playerPos = player.position.ConvertInt();
        int spanCount = 0;
        foreach(var check in moveablePoint)
        {
            candidates.Clear();
            candidates.Enqueue(check);
            for (int i = movePoint; i > 0; i--)
            {
                int count = candidates.Count;
                for(int j=0; j<count;j++)
                {
                    CostCheck(candidates.Dequeue(),playerPos,movePoint,candidates);
                }
            }
            if(candidates.Count==0)
            {
                span[spanCount] = check;
                spanCount++;
            }
        }
        for (int i = 0; i < spanCount; i++)
            moveablePoint.Remove(span[i]);
    }


    public void CreateAreaView()
    {
        foreach (var view in curAreaView)
            view.Return();
        curAreaView.Clear();
        for (int i = 0; i < moveablePoint.Count; i++)
            moveablePoint[i] += Vector3Int.up;
        foreach(Vector3Int moveable in moveablePoint)
        {
            Vector3 target = new Vector3(moveable.x, moveable.y+0.1f, moveable.z);
            AreaView tempView = AreaViewManager.Instance.CallAreaView(target, transform);
            tempView.playerMove = this;
            curAreaView.Add(tempView);
            if (field.Cube(moveable).type != CUBE_TYPE.Air)
                tempView.SetColor(TILE_TYPE.Disable);
            else
                tempView.SetColor(TILE_TYPE.Enable);
        }

    }




}
