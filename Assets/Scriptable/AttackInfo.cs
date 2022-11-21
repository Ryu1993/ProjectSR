using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AttackType { Point,Straight, Range,Ignore}

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
    public AttackType type;


}
