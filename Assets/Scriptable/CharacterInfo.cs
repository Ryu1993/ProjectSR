using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName ="Character")]
public class CharacterInfo : ScriptableObject
{
    public int maxHp;
    public int curHp;
    public int movePower;
    public int jumpHeight;

    public List<AttackInfo> attackList;
    public AssetReference characterPrefab;

}
