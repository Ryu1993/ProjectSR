
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum FIELD_SHAPE { Cross, Diagonal, Square ,RangeLimitSquare}

public class AreaViewManager : NonBehaviourSingleton<AreaViewManager>, ISetable
{
    private readonly string assetName = "AreaTile";
    private ObjectPool areaViewPool;
    public void Set()
    {
        areaViewPool = ObjectPoolManager.Instance.PoolRequest(assetName, 20, 5);
    }
    private Dictionary<Vector2Int, AreaView> areaViewDic = new Dictionary<Vector2Int, AreaView>();
    public Dictionary<Vector2Int,AreaView> AreaViewDic { get { return areaViewDic; } }

    private Dictionary<Vector2Int, Vector3Int> areaCoordDic = new Dictionary<Vector2Int, Vector3Int>();
    public Dictionary<Vector2Int, Vector3Int> AreaCoordDic { get { return areaCoordDic; } }

    private Action<AreaView> areaReturn = (area) => { area.Return(); };


    public void CallAreaFieldShape(Vector2Int origin,int range,Dictionary<Vector2Int,AreaView> viewDictionary, FIELD_SHAPE shape = FIELD_SHAPE.Square)
    {
        Func<int, int, bool> accept = (x, y) => true;
        switch (shape)
        {
            case FIELD_SHAPE.RangeLimitSquare: accept = (i, j) =>
                {
                    int x = i < 0 ? -i : i;
                    int z = j < 0 ? -j : j;
                    if (x + z <= range)
                        return true;
                    return false;
                };
                break;
            case FIELD_SHAPE.Cross: accept = (i, j) =>
                {
                    if (i == 0 | j == 0)
                        return true;
                    return false;
                };
                break;

            case FIELD_SHAPE.Diagonal:
                for (int i = -range; i <= range; i++)
                {
                    if (i == 0) continue;
                    AddAreaView(origin + new Vector2Int(i, i), viewDictionary);
                    AddAreaView(origin + new Vector2Int(i, -i), viewDictionary);
                }
                return;
        }
        for (int i= -range;i<=range;i++)
            for(int j = -range; j <= range; j++)
            {
                if (accept(i, j))
                    AddAreaView(origin + new Vector2Int(i, j), viewDictionary);
            }
    }

    private void AddAreaView(Vector2Int target2DCoord, Dictionary<Vector2Int, AreaView> viewDictionary)
    {
        Vector3 arewViewHeight = Vector3.up * 0.1f;
        if(FieldManager.Instance.surfaceDic.TryGetValue(target2DCoord,out Vector3Int targetCoord))
            viewDictionary.Add(target2DCoord, areaViewPool.Call(targetCoord).GetComponent<AreaView>());
    }

    

    public void ReturnAreaViews()
    {
        areaViewDic.LoopDictionary(areaReturn);
        areaViewDic.Clear();
    }


    public AreaView CallAreaView(Vector3Int position)
    {
        return areaViewPool.Call(position).GetComponent<AreaView>();
    }

    public void CallAreaViews()
    {
        ReturnAreaViews();
        areaCoordDic.LoopDictionary((Vector3Int coord) => areaViewDic.Add(new Vector2Int(coord.x, coord.z), CallAreaView(coord)));           
    }

    private void AddCoord(Vector2Int target2DCoord)
    {
        if (FieldManager.Instance.surfaceDic.TryGetValue(target2DCoord, out Vector3Int targetCoord))
            areaCoordDic.Add(target2DCoord, targetCoord);
    }
    public void AreaCoordSet(Vector2Int origin, int range, FIELD_SHAPE shape = FIELD_SHAPE.Square)
    {
        areaCoordDic.Clear();
        Func<int, int, bool> accept = (x, y) => true;
        switch (shape)
        {
            case FIELD_SHAPE.RangeLimitSquare:
                accept = (i, j) =>
                {
                    int x = i < 0 ? -i : i;
                    int z = j < 0 ? -j : j;
                    if (x + z <= range)
                        return true;
                    return false;
                };
                break;
            case FIELD_SHAPE.Cross:
                accept = (i, j) =>
                {
                    if (i == 0 | j == 0)
                        return true;
                    return false;
                };
                break;

            case FIELD_SHAPE.Diagonal:
                for (int i = -range; i <= range; i++)
                {
                    if (i == 0) continue;
                    AddCoord(origin + new Vector2Int(i, i));
                    AddCoord(origin + new Vector2Int(i, -i));
                }
                return;
        }
        for (int i = -range; i <= range; i++)
            for (int j = -range; j <= range; j++)
            {
                if (accept(i, j))
                    AddCoord(origin + new Vector2Int(i, j));
            }
    }

}
