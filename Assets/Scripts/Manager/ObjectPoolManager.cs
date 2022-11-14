using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ObjectPoolManager : NonBehaviourSingleton<ObjectPoolManager>
{
    [SerializeField]
    private List<ObjectPool> m_pools = new List<ObjectPool>();
    private Transform m_active;
    private Transform m_inactive;
    //private static ObjectPoolManager instance;
    //public static ObjectPoolManager Instance
    //{
    //    get
    //    {
    //        if(instance == null)
    //            instance = new ObjectPoolManager();
    //        return instance;     
    //    }
    //}
    private void InactiveSet()
    {
        if(m_active == null)
        {
            m_active = new GameObject("Active").transform;
            m_inactive = new GameObject("InActive").transform;
            m_inactive.gameObject.SetActive(false);
        }
    }

    public ObjectPool PoolRequest(GameObject baseObj,int start,int add)
    {
        InactiveSet();
        if(!baseObj.TryGetComponent(out IPoolingable temp)) //대상이 오브젝트풀 인터페이스를 가지고 있는지 체크후 없다면 null반환
        {
            return null;
        }
        foreach(var pool in m_pools) //현재 생성된 풀중에 해당 오브젝트에 해당하는 오브젝트풀이 있는지 체크후 있다면 기존의 풀을 반환
        {
            if(pool.m_baseObj == baseObj)
            {
                return pool;
            }
        }
        //현재 생성된 풀중에 없다면 새롭게 오브젝트 풀을 생성후 반환
        ObjectPool resultPool = new ObjectPool(baseObj, start, add, m_inactive, m_active);
        m_pools.Add(resultPool);
        return resultPool;
    }
    //Addressable로 ObjectPool생성시 호출 
    public ObjectPool PoolRequest(ref AsyncOperationHandle<GameObject> handle, int start, int add)
    {
        InactiveSet();
        GameObject baseObj = handle.WaitForCompletion();//handle이 메모리에 오브젝트를 완전히 로드시킬 때까지 대기
        if (!baseObj.TryGetComponent(out IPoolingable temp)) //대상이 오브젝트풀 인터페이스를 가지고 있는지 체크후 없다면 오브젝트를 언로드시키고 null반환
        {
            Addressables.Release(handle);
            return null;
        }
        foreach (var pool in m_pools) // 현재 생성된 풀중에 해당 오브젝트에 해당하는 오브젝트풀이 있는지 체크후 만약 있다면 오브젝트를 언로드시키고 기존 풀을 반환 
        {
            if (pool.m_baseObj == baseObj)
            {
                Addressables.Release(handle);
                return pool;
            }
        }
        //현재 생성된 풀중에 없다면 새롭게 오브젝트 풀을 생성후 오브젝트 풀에 현재 handle을 세팅후 반환
        ObjectPool resultPool = new ObjectPool(baseObj, start, add, m_inactive, m_active);
        resultPool.HandleSet(ref handle);
        m_pools.Add(resultPool);
        return resultPool;
    }

    //Addressable이용해서 오브젝트 풀 생성시 오버로딩
    public ObjectPool PoolRequest(IResourceLocation location, int start, int add)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(location);
        return PoolRequest(ref handle, start, add);
    }
    public ObjectPool PoolRequest(AssetReference reference, int start, int add)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(reference);
        return PoolRequest(ref handle, start, add);
    }

    public ObjectPool PoolRequest(ref string assetName, int start, int add)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetName);
        return PoolRequest(ref handle, start, add);
    }

}
