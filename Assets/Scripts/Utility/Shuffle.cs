using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Shuffle
{
    public static void Array<T>(ref T[] values)
    {
        T[] result = new T[values.Length];
        for(int i = 0; i < values.Length; i++)
        {
            result[i] = values[i];
        }
        for(int i =0; i<result.Length;i++)
        {
            int tempIndex = Random.Range(i, result.Length);
            T temp = result[tempIndex];
            result[tempIndex] = result[i];
            result[i] = temp;
        }
        values = result;
    }



}
