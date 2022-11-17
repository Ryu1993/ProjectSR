using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class ExtencionMethod 
{

    public static Vector3Int ToInt(this Vector3 vector) => new Vector3Int((int) vector.x, (int) vector.y, (int) vector.z);

    public static Vector2Int To2DInt(this Vector3 vector) => new Vector2Int((int)vector.x, (int)vector.z);

    public static Vector2Int To2DInt(this Vector3Int vector) => new Vector2Int(vector.x, vector.z);

}
