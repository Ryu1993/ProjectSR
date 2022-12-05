using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonBehaviourSingleton<T> where T : ISetable, new()
{
    private static T instance; // TŸ�� �������� instance
    public static T Instance // ���� TŸ�� �������� instance�� ���� ������Ƽ
    {
        get
        {
            if (instance == null)//instance�� ������� ���
            {
                instance = new T();
                instance.Set();
            }
            return instance;
        }
    }

}
