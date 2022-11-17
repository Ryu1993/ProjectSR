using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    private FieldGenerator field { get { return FieldGenerator.Instance; } }

    public AttackInfo attack;

    Dictionary<Vector2Int, AreaView> selectRange = new Dictionary<Vector2Int, AreaView>();

    public Transform test;

    public void Start()
    {
        AttackAreaCreate(test);
    }

    public void AttackAreaCreate(Transform character)
    {
        selectRange.Clear();
        transform.position = character.position;
        AreaViewManager.Instance.CallAreaField(transform.position.To2DInt(), attack.selectRange, selectRange);




    }

    public void CrossArea(Func<Vector2Int,bool> condition,Action<Vector2Int> action)
    {
        for (int i = -attack.selectRange; i <= attack.selectRange; i++)
            for (int j = -attack.selectRange; j <= attack.selectRange; j++)
            {
                if (i == 0 & j == 0) continue;
                if (i != 0 & j != 0) continue;
                if (condition(new Vector2Int(i, j)))
                    action(new Vector2Int(i, j));
            }
    }

    public bool AreaViewCall(Vector2Int localcoord)
    {
        Vector2Int coord =localcoord+ new Vector2Int((int)transform.position.x, (int)transform.position.z);
        if (field.Surface(coord,out Vector3 targetCoord))
        {
            selectRange.Add(localcoord, AreaViewManager.Instance.CallAreaView(targetCoord + new Vector3(0, 0.1f, 0), transform));
            return true;
        }
        return false;
    }


 






}
