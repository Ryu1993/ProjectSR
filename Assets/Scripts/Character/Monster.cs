using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Monster : Character
{

    public Character target;
    private List<AttackInfo> attackCheckList = new List<AttackInfo>(4);


    public IEnumerator MonsterAction()
    {
        TargetSearch();
        AttackCheck();
        if(attackCheckList.Count==0)
        {
            if (MoveCheck())
            {
                yield return CharacterMove.Instance.MonsterMove();
                AttackCheck();
                if(attackCheckList.Count>0)
                {
                    yield return AttackAction();
                }
            }
            else
                Rest();
        }
        else
        {
            yield return AttackAction();
        }
    }

    private void Rest()
    {
        print("ÈÞ½Ä");
    }

    public void TargetSearch()
    {
        target = GameManager.Instance.party[Random.Range(0, GameManager.Instance.party.Count)];
    }


    private YieldInstruction AttackAction()
    {
        AttackInfo active = attackCheckList[Random.Range(0, attackCheckList.Count)];
        CharacterAttack.Instance.AttackAreaCreate(this, active, false);
        return CharacterAttack.Instance.MonsterAttack(target);
    }

    private bool MoveCheck()
    {
        return CharacterMove.Instance.MonsterMoveCheck(this,target.transform.position,moveablePoint);
    }

    private void AttackCheck()
    {
        attackCheckList.Clear();
        foreach(AttackInfo attack in attackableList)
        {
            if (CharacterAttack.Instance.AttackableCheck(this,attack,target))
                attackCheckList.Add(attack);
        }
    }

}
