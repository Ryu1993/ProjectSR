using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonBehaviourSingleton<T> where T : ISetable, new()
{
    private static T instance; // T타입 전역변수 instance
    public static T Instance // 위의 T타입 전역변수 instance에 대한 프로퍼티
    {
        get
        {
            if (instance == null)//instance가 비어있을 경우
            {
                instance = new T();
                instance.Set();
            }
            return instance;
        }
    }

}
