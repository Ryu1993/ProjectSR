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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            SetMovePoint();
            CreateAreaView();
        }


    }




    public void SetMovePoint()
    {
        moveablePoint.Clear();
        Vector3Int playerPoint = new Vector3Int((int)player.position.x, (int)player.position.y, (int)player.position.z);
        Vector3Int nextPoint = playerPoint - Vector3Int.up;
        for(int i =0;i<movePoint;i++)
        {
            if (MoveableCheck(nextPoint + Vector3Int.right, jumpHeight, out nextPoint))
            {
                InputMoveable(nextPoint);
                Vector3Int originPoint = nextPoint;
                for (int j = 0; j < movePoint - (i+1); j++)
                {
                    if (MoveableCheck(originPoint + Vector3Int.forward, jumpHeight, out originPoint))
                        InputMoveable(originPoint);
                    else
                        break;
                }
                originPoint = nextPoint;
                for (int j = 0; j < movePoint - (i + 1); j++)
                {
                    if (MoveableCheck(originPoint - Vector3Int.forward, jumpHeight, out originPoint))
                        InputMoveable(originPoint);
                    else
                        break;
                }
            }
            else
                break;
        }
        nextPoint = playerPoint-Vector3Int.up;
        for (int i = 0; i < movePoint; i++)
        {
            if (MoveableCheck(nextPoint - Vector3Int.right, jumpHeight, out nextPoint))
            {
                InputMoveable(nextPoint);
                Vector3Int originPoint = nextPoint;
                for (int j = 0; j < movePoint - (i + 1); j++)
                {
                    if (MoveableCheck(originPoint + Vector3Int.forward, jumpHeight, out originPoint))
                        InputMoveable(originPoint);
                    else
                        break;
                }
                originPoint = nextPoint;
                for (int j = 0; j < movePoint - (i + 1); j++)
                {
                    if (MoveableCheck(originPoint - Vector3Int.forward, jumpHeight, out originPoint))
                        InputMoveable(originPoint);
                    else
                        break;
                }
            }
            else
                break;
        }
        nextPoint = playerPoint - Vector3Int.up;
        for (int i = 0; i < movePoint; i++)
        {
            if (MoveableCheck(nextPoint - Vector3Int.forward, jumpHeight, out nextPoint))
            {
                InputMoveable(nextPoint);
                Vector3Int originPoint = nextPoint;
                for (int j = 0; j < movePoint - (i + 1); j++)
                {
                    if (MoveableCheck(originPoint + Vector3Int.right, jumpHeight, out originPoint))
                        InputMoveable(originPoint);
                    else
                        break;
                }
                originPoint = nextPoint;
                for (int j = 0; j < movePoint - (i + 1); j++)
                {
                    if (MoveableCheck(originPoint - Vector3Int.right, jumpHeight, out originPoint))
                        InputMoveable(originPoint);
                    else
                        break;
                }
            }
            else
                break;
        }
        nextPoint = playerPoint - Vector3Int.up;
        for (int i = 0; i < movePoint; i++)
        {
            if (MoveableCheck(nextPoint + Vector3Int.forward, jumpHeight, out nextPoint))
            {
                InputMoveable(nextPoint);
                Vector3Int originPoint = nextPoint;
                for (int j = 0; j < movePoint - (i + 1); j++)
                {
                    if (MoveableCheck(originPoint + Vector3Int.right, jumpHeight, out originPoint))
                        InputMoveable(originPoint);
                    else
                        break;
                }
                originPoint = nextPoint;
                for (int j = 0; j < movePoint - (i + 1); j++)
                {
                    if (MoveableCheck(originPoint - Vector3Int.right, jumpHeight, out originPoint))
                        InputMoveable(originPoint);
                    else
                        break;
                }
            }
            else
                break;
        }
    }

    public void CreateAreaView()
    {
        for (int i = 0; i < moveablePoint.Count; i++)
            moveablePoint[i] += Vector3Int.up;
        foreach(Vector3Int moveable in moveablePoint)
        {
            Vector3 target = new Vector3(moveable.x, moveable.y+0.1f, moveable.z);
            AreaView tempView = AreaViewManager.Instance.CallAreaView(target, transform);
            if (field.Cube(moveable).type != CUBE_TYPE.Air)
                tempView.SetColor(TILE_TYPE.Disable);
            else
                tempView.SetColor(TILE_TYPE.Enable);
        }

    }
    public bool MoveableCheck(Vector3Int position,int jumpHeiht,out Vector3Int nextPosition)
    {
        bool result = false;
        nextPosition = position;
        switch(field.Cube(position).type)
        {
            case CUBE_TYPE.Ground:
                if(field.Cube(position+Vector3Int.up).type != CUBE_TYPE.Obstacle)
                    result = true;
                break;

            case CUBE_TYPE.Air:
                for(int i=0;i<jumpHeiht; i++)
                    if (field.Cube(position - Vector3Int.up*(i+1)).type == CUBE_TYPE.Ground)
                    {
                        nextPosition = position - Vector3Int.up * (i+1);
                        result = true;
                        break;
                    }
                break;
            case CUBE_TYPE.Bed:
                for (int i = 0; i < jumpHeiht; i++)
                    if (field.Cube(position + Vector3Int.up * (i+1)).type == CUBE_TYPE.Ground)
                    {
                        nextPosition = position + Vector3Int.up * (i + 1);
                        result = true;
                        break;
                    }
                break;   
        }
        return result;
    }

    public void InputMoveable(Vector3Int moveable)
    {
        if (moveablePoint.Contains(moveable)) return;
        moveablePoint.Add(moveable);
    }


}
