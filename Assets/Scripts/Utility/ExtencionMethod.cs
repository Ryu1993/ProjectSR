using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class ExtencionMethod 
{

    public static Vector3Int ConvertInt(this Vector3 vector) => new Vector3Int((int) vector.x, (int) vector.y, (int) vector.z);


}
