using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName ="Attack")]
public class AttackInfo : ScriptableObject
{
    
    public new string name;
    public bool targetOnly;
    [BoxGroup("SelectInfo")] public int selectRange;
    [BoxGroup("SelectInfo")] public bool selectHeightAllow;
    [BoxGroup("SelectInfo")] public CUBE_TYPE[] selectCubeMask;
    [BoxGroup("SelectInfo")] public VoxelType[] selectVoxelMask;
    [BoxGroup("SelectInfo")] public FIELD_SHAPE selectShape;
    [BoxGroup("SelectInfo")] public bool isVisionCheck;

    [BoxGroup("AttackInfo")] public int attakcRange;
    [BoxGroup("AttackInfo")] public bool attakcHeightAllow;
    [BoxGroup("AttackInfo")] public CUBE_TYPE[] attackCubeMask;
    [BoxGroup("AttackInfo")] public VoxelType[] attackVoxelMask;
    [BoxGroup("AttackInfo")] public FIELD_SHAPE attackShape;
    [BoxGroup("AttackInfo")] public bool isBlockable;
    public AssetReferenceT<AnimationClip> attackMotion;
    public AssetReferenceT<GameObject> attackEffect;
    public float attackDelay;

}
