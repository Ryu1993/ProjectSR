using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance; // T타입 전역변수 instance
    public static T Instance // 위의 T타입 전역변수 instance에 대한 프로퍼티
    {
        get 
        {
            if(instance == null)//instance가 비어있을 경우
            {
                instance = FindObjectOfType(typeof(T)) as T; //현재 활성화된 컴포넌트 중 T타입 컴포넌트를 가진 오브젝트를 찾아서 T타입으로 형변환 후 대입
                if (instance == null) // T타입 컴포넌트를 가진 오브젝트가 없어서 여전히 null상태일 경우
                {
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();//T타입 이름을 가진 Object를 생성, Object에 T타입 컴포넌트를 추가 후 대입
                }
            }
            return instance;            
        }
    }

}
