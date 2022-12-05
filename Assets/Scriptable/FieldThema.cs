using UnityEngine;
using UnityEngine.AddressableAssets;


[CreateAssetMenu(menuName ="FieldThema")]
public class FieldThema : ScriptableObject
{
    public AssetReferenceT<GameObject> bed;
    public AssetReferenceT<GameObject>[] ground;
    public AssetReferenceT<GameObject> water;


}
