using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;



[System.Serializable]
public struct FieldCube
{
    public CUBE_TYPE type;
    public AssetLabelReference label;

}

[CreateAssetMenu(menuName = "FieldInfo")]
public class FieldInfo : ScriptableObject
{
    public FieldCube[] cubeData;
    public Dictionary<CUBE_TYPE, AssetLabelReference> field;

    public void FieldSet()
    {
        field = new Dictionary<CUBE_TYPE, AssetLabelReference>();
        foreach(var data in cubeData)
        {
            field.Add(data.type, data.label);
        }
    }



}
