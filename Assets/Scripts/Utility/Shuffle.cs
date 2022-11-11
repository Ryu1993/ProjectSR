using System;
using Random = UnityEngine.Random;
public static class Shuffle
{
    //Span을 활용해 힙 할당하지 않고 array 변경
    public static void Array<T>(ref T[] values)
    {
        Span<T> result = values;
        for(int i =0; i<result.Length;i++)
        {
            int tempIndex = Random.Range(i, result.Length);
            T temp = result[tempIndex];
            result[tempIndex] = result[i];
            result[i] = temp;
        }


    }



}
