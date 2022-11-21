using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName ="Test")]
public class AttackInfo : ScriptableObject
{
    public int selectRange;
    public int attakcRange;
    public bool selectHeightAllow;
    public bool attakcHeightAllow;
    public bool targetOnly;
    public CUBE_TYPE[] selectCubeMask;
    public CUBE_TYPE[] attackCubeMask;
    public AssetReferenceT<AnimationClip> attackMotion;
    public AssetReferenceT<GameObject> attackEffect;

}
