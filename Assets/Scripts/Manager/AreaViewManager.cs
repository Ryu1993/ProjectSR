
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum FIELD_SHAPE { Cross, Diagonal, Square ,RangeLimitSquare}

public class AreaViewManager : NonBehaviourSingleton<AreaViewManager>
{
    private string assetName = "AreaTile";
    //public AssetReference reference;
    private ObjectPool _areaViewPool;
    private ObjectPool areaViewPool
    {
        get
        {
            if (_areaViewPool == null)
                _areaViewPool = ObjectPoolManager.Instance.PoolRequest(ref assetName, 20, 5);
            return _areaViewPool;
        }
    }
    private FieldGenerator field
    {
        get { return FieldGenerator.Instance; }
    }


    public AreaView CallAreaView(Vector3 position,Transform origin)=> areaViewPool.Call(position, Quaternion.Euler(new Vector3(90,0,0)),origin,true).GetComponent<AreaView>();

    public void CallAreaField(Vector2Int origin,int range, Dictionary<Vector2Int,AreaView> viewList)
    {
        viewList.Clear();
        for(int i=-range;i<=range;i++)
            for(int j=-range;j<=range;j++)
            {
                Vector2Int twoDimencionsCoord = origin + new Vector2Int(i, j);
                if (field.Surface(twoDimencionsCoord, out Vector3 coord))
                {
                    coord.y += 0.1f;
                    viewList.Add(twoDimencionsCoord, areaViewPool.Call(coord, Quaternion.Euler(new Vector3(90, 0, 0))).GetComponent<AreaView>());
                }
            }
    }
 
    public void CallAreaFieldShape(Vector2Int origin,int range,Dictionary<Vector2Int,AreaView> viewDictionary, FIELD_SHAPE shape = FIELD_SHAPE.Square)
    {
        Quaternion rightAngle = Quaternion.Euler(new Vector3(90, 0, 0));
        Func<int,int,bool> accept = (x,y) => true;
        int count = 0;
        switch(shape)
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
        }
        if(shape == FIELD_SHAPE.Diagonal)
        {
            for (int i = -range; i <= range; i++)
            {
                if (i == 0) continue;
                AddAreaView(origin + new Vector2Int(i, i), ref rightAngle, viewDictionary);
                AddAreaView(origin + new Vector2Int(i, -i), ref rightAngle, viewDictionary);
            }
            return;
        }
        for (int i= -range;i<=range;i++)
            for(int j = -range; j <= range; j++)
            {
                if (accept(i, j))
                    AddAreaView(origin + new Vector2Int(i, j), ref rightAngle, viewDictionary);
                else
                    count++;
            }
    }

    private void AddAreaView(Vector2Int targetCoord,ref Quaternion rightAngle, Dictionary<Vector2Int, AreaView> viewDictionary)
    {
        if (field.Surface(targetCoord, out Vector3Int coord))
            viewDictionary.Add(targetCoord, areaViewPool.Call(coord + Vector3.up * 0.1f, rightAngle).GetComponent<AreaView>());
    }









}
