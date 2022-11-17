using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AttackType { Point,Straight, Range,Ignore}

[CreateAssetMenu(menuName ="Test")]
public class AttackInfo : ScriptableObject
{
    public int selectRange;
    public int attakcRange;
    public bool isTerrainIgnore;
    public bool isObstacleIgnore;
    public AttackType type;


}
