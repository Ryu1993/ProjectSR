using System;
using System.Collections.Generic;
using System.Diagnostics;
using Random = UnityEngine.Random;
public static class Shuffle
{
    public static void Array<T>(ref T[] array)
    {
        for(int i =0; i<array.Length;i++)
        {
            int tempIndex = Random.Range(i, array.Length);
            T temp = array[tempIndex];
            array[tempIndex] = array[i];
            array[i] = temp;
        }
    }

    public static void ShuffleSpan<T>(ref Span<T> array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int tempIndex = Random.Range(i, array.Length);
            T temp = array[tempIndex];
            array[tempIndex] = array[i];
            array[i] = temp;
        }
    }

    public static void ShuffleList<T>(List<T> list) where T : struct
    {
        for(int i = 0; i<list.Count;i++)
        {
            int tempIndex = Random.Range(i, list.Count);
            T temp = list[tempIndex];
            list[tempIndex] = list[i];
            list[i] = temp;
        }
    }



    

}
