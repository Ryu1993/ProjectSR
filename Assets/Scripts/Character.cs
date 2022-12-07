using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    protected Animator animator;
    public List<AttackInfo> attackList;
    public List<AttackInfo> attackableList = new List<AttackInfo>(4);
    public int moveablePoint;
    public int jumpableHeight;
    /// <summary>
    /// 0:Attack 1: Move 2 : Rest
    /// </summary>
    public bool[] actionable;
    protected float maxHp;
    protected int hp;
    public UnityAction<float> hpChangeAction;
    public CharacterInfo info;
    public bool isDiagonalMoveAllowed;
    public AnimationClip originalAttackClip;

    public void TurnReset()
    {
        ActionReset();
        attackableList.Clear();
        foreach (AttackInfo attackInfo in attackList)
            attackableList.Add(attackInfo);
    }


    public void ActionReset()
    {
        for (int i = 0; i < actionable.Length; i++)
            actionable[i] = true;
    }

    public void CharacterSetting(CharacterInfo chrInfo)
    {
        info = chrInfo;
        maxHp = info.maxHp;
        Hp = info.curHp;
        attackList = info.attackList;
        moveablePoint = info.movePower;
        jumpableHeight = info.jumpHeight;
        actionable = new bool[3];
        TurnReset();
    }






    public Animator Animator
    { 
        get { if (animator == null) transform.TryGetComponent(out animator); return animator; }
    }
    public int Hp
    {
        get { return hp; }
        set
        {
            hp = value;
            info.curHp = value;
            hpChangeAction?.Invoke(hp / maxHp);
        }
    }
}
