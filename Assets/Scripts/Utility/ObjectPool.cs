using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class ObjectPool
{
    [SerializeField]
    public GameObject m_baseObj { get; private set; }
    private int m_add;
    private Stack<GameObject> m_pool = new Stack<GameObject>();
    private Transform m_inactive;
    private Transform m_active;
    private AsyncOperationHandle m_baseObjHandle;


    // ObjectPool 생성자, 생성시 입력받은대로 세팅 후 초기값만큼 풀을 채운 후 ObjectPool을 반환
    public ObjectPool(GameObject baseObj, int start, int add, Transform inactive,Transform active) 
    {
        m_baseObj = baseObj;
        m_add = add;
        m_inactive = inactive;
        m_active = active;
        PoolAdd(start);
    }
    //baseObj가 Addressable로 로드했을 경우 ObjectPool을 런타임중에 해제할때 리소스 언로드를 위하여 저장
    public void HandleSet(ref AsyncOperationHandle<GameObject> handle)
    {
        m_baseObjHandle = handle;
    }

    //ObjectPool에 seed값만큼 baseObj의 복제본 생성후 보관, 복제본 생성시 풀링 인터페이스를 가져와서 생성한 오브젝트 풀의 정보를 입력(해당 오브젝트가 생성된 풀에 Return하기 위해 필요)
    private void PoolAdd(int seed)
    {
        for(int i = 0; i < seed; i++)
        {
            GameObject createdObj = GameObject.Instantiate(m_baseObj, m_inactive);
            createdObj.TryGetComponent(out IPoolingable poolingable);
            poolingable.home = this;
            m_pool.Push(createdObj);
        }
    }

    //ObjectPool에 Return시킴. 생성시 입력한 ObjectPool에 대한 정보로 호출해서 반환할 수 있도록 public으로 선언
    public void Return(GameObject go)
    {
        go.transform.SetParent(m_inactive,false);
        go.transform.position = Vector3.zero;
        m_pool.Push(go);
    }

    //ObjectPool에서 오브젝트 호출 및 호출 관련 오버로딩
    public Transform Call(Vector3 position,Quaternion rotate,Transform parent,bool worldPositonStay,bool isMove)
    {
        if(m_pool.Count == 0)
            PoolAdd(m_add);
        m_pool.Pop().TryGetComponent(out Transform objectTransform);
        if(isMove)
            objectTransform.position = position;
        objectTransform.rotation = rotate;
        if(parent!=null)
            objectTransform.SetParent(parent, worldPositonStay);
        else
            objectTransform.SetParent(m_active, false);
        return objectTransform;
    }
    public Transform Call(Quaternion rotate) => Call(Vector3.zero, rotate, null, false, false);
    public Transform Call(Transform parent, bool worldPositonStay) => Call(Vector3.zero, Quaternion.identity, parent, worldPositonStay, false);
    public Transform Call(Quaternion rotate, Transform parent, bool worldPositonStay) => Call(Vector3.zero, rotate, parent, worldPositonStay, false);
    public Transform Call(Vector3 position) => Call(position, Quaternion.identity, null, false, true);
    public Transform Call(Vector3 position, Quaternion rotate) => Call(position, rotate, null, false, true);
    public Transform Call(Vector3 position, Transform parent) => Call(position, Quaternion.identity, parent, true, true);
    public Transform Call(Vector3 position, Transform parent, bool worldPositonStay) => Call(position, Quaternion.identity, parent, worldPositonStay, true);
    public Transform Call(Vector3 position, Quaternion rotate, Transform parent, bool worldPositonStay) => Call(position, rotate, parent, worldPositonStay, true);
    ~ObjectPool()//GC가 수집할때 만약 baseObj가 어드레서블로 메모리에 로드시켰다면 같이 해제시키도록 함
    {
        Addressables.Release(m_baseObjHandle);
    }

}
