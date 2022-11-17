
using System.Collections.Generic;
using UnityEngine;


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










}
