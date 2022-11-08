using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName ="FieldInfo")]
public class FieldInfo : ScriptableObject
{
    public AssetReference fieldSuburb;
    public AssetReference[] fieldBase;
    public AssetReference[] fieldHill;

}
