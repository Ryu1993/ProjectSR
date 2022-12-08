using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MotionPlayer : ISetable
{
    public GameObject effect;
    protected Character order;
    protected Vector3Int targetPostion;
    protected int attackRange;
    protected int attackDamage;
    protected float attackDelay;
    protected bool orderIsMonster;
    public abstract void Set();
    public virtual void Play(Character order, Vector3 target)
    {
        MotionManager.Instance.isMotionCompleted = false;
        orderIsMonster = order is Monster;
        targetPostion = Vector3Int.RoundToInt(target);
        attackRange = order.CurAttackInfo.attakcRange;
        attackDelay = order.CurAttackInfo.attackDelay;
        this.order = order;
    }
    protected virtual void Attack()
    {
        MotionManager.Instance.isMotionCompleted = true;
    }

    protected void RangeAttack()
    {
        Voxel checkVoxel = null;
        VoxelState checkState = orderIsMonster ? VoxelState.Enemy : VoxelState.Player;
        CoordCheck.CustomSideCheck3D(targetPostion, attackRange,
            (checkCoord) =>
            {
                checkVoxel = FieldManager.Instance.Voxel(checkCoord);
                return checkVoxel?.state != checkState & checkVoxel?.state != VoxelState.Null;
            },
            (checkCoord) =>
            {
                checkVoxel?.data.onCharacter.Hit(attackDamage);
            },
            true);
    }
    protected void PenetrateAttack()
    {
        Voxel checkVoxel = null;
        VoxelState checkState = orderIsMonster ? VoxelState.Enemy : VoxelState.Player;
        Vector3Int orderCoord = Vector3Int.RoundToInt(order.transform.position);
        Vector3Int targetCoord = Vector3Int.RoundToInt(targetPostion);
        CoordCheck.VoxelRayCast(orderCoord, targetCoord, out List<Vector3Int> crashList, order.CurAttackInfo.attackVoxelMask);
        foreach (Vector3Int crash in crashList)
        {
            checkVoxel = FieldManager.Instance.Voxel(crash);
            if (checkVoxel?.state != checkState)
                checkVoxel.data.onCharacter.Hit(attackDamage);
        }
    }

  
}
