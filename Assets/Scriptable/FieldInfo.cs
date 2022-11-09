using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;



public struct FieldCube
{
    public CUBE_TYPE type;
    public AssetReference[] assets;

}

[CreateAssetMenu(menuName = "FieldInfo")]
public class FieldInfo : ScriptableObject
{
    public FieldCube[] data;
    public Dictionary<CUBE_TYPE, AssetReference[]> field;


}
