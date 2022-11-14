using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

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

    public AreaView CallAreaView(Vector3 position,Transform transform)=> areaViewPool.Call(position,Quaternion.Euler(new Vector3(90,0,0))).GetComponent<AreaView>();










}
