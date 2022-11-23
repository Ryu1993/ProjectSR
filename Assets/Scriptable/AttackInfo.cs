using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName ="Test")]
public class AttackInfo : ScriptableObject
{
    public bool targetOnly;
    
    [BoxGroup("SelectInfo")] public int selectRange;
    [BoxGroup("SelectInfo")] public bool selectHeightAllow;
    [BoxGroup("SelectInfo")] public CUBE_TYPE[] selectCubeMask;
    [BoxGroup("SelectInfo")] public FIELD_SHAPE selectShape;

    [BoxGroup("AttackInfo")] public int attakcRange;
    [BoxGroup("AttackInfo")] public bool attakcHeightAllow;
    [BoxGroup("AttackInfo")] public CUBE_TYPE[] attackCubeMask;
    [BoxGroup("AttackInfo")] public FIELD_SHAPE attackShape;

    public AssetReferenceT<AnimationClip> attackMotion;
    public AssetReferenceT<GameObject> attackEffect;

}
