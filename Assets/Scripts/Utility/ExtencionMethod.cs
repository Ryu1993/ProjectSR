using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public static class ExtencionMethod 
{
    public static Vector3Int ToInt(this Vector3 vector) => new Vector3Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), Mathf.RoundToInt( vector.z));
    public static Vector2Int To2DInt(this Vector3 vector) => new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.z));
    public static Vector2Int To2DInt(this Vector3Int vector) => new Vector2Int(vector.x, vector.z);
    public static bool CurState(this Animator animator,int hash)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return info.shortNameHash == hash | info.fullPathHash == hash | info.tagHash == hash;
    }
    public static float CurStateProgress(this Animator animator,int hash)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.shortNameHash == hash | info.fullPathHash == hash | info.tagHash == hash)
            return info.normalizedTime;
        return 0f;
    }
    public static void LoopDictionary<T1,T2>(this Dictionary<T1,T2> dic,Action<T2> action)
    {
        LoopDictionary(dic, (pair) => action?.Invoke(pair.Value));
    }
    public static void LoopDictionary<T1,T2>(this Dictionary<T1,T2> dic,Action<T1> action)
    {
        LoopDictionary(dic, (pair) => action?.Invoke(pair.Key));
    }
    public static void LoopDictionary<T1,T2>(this Dictionary<T1, T2> dic, Action<KeyValuePair<T1,T2>> action)
    {
        foreach (KeyValuePair<T1, T2> pair in dic)
            action?.Invoke(pair);
    }

    public static void Loop<T>(this T[][][] array,Action<T> action)
    {
        for (int i = 0; i < array.GetLength(0); i++)
            for (int j = 0; j < array.GetLength(1); j++)
                for (int k = 0; k < array.GetLength(2); k++)
                    action.Invoke(array[i][j][k]);                  
    }
    public static void LoopY<T>(this T[][][] array,int y,Action<T> action)
    {
        for (int i = 0; i < array.GetLength(0); i++)
            for (int j = 0; j < array.GetLength(2); j++)
                action.Invoke(array[i][y][j]);
    }

    public static void LoopXZ<T>(this T[][][] array,Vector2Int coord,Action<T> action)
    {
        for (int i = 0; i < array.GetLength(1); i++)
            action.Invoke(array[coord.x][i][coord.y]);
    }

    public static T Coord<T>(this T[][][] array,Vector3Int coord)
    {
        return array[coord.x][coord.y][coord.z];
    }







}
