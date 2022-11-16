using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    private FieldGenerator _field;
    private FieldGenerator field { get { if (_field == null) _field = FieldGenerator.Instance; return _field; } }

    public AttackInfo attack;

    private AreaView[,] selectRange;
    private int rangeSize;




    public void AttackAreaCreate(Transform character)
    {
        transform.position = character.position;
        rangeSize = attack.selectRange * 2 + 1;
        selectRange = new AreaView[rangeSize,rangeSize];




    }

    public Vector3 AreaToWorld(AreaView area) => area.transform.position + new Vector3(0, 0.1f, 0);
    public CUBE_TYPE CubeCheck(AreaView area) => field.Cube(AreaToWorld(area).ConvertInt()).type;

    public void CrossArea(Func<Vector2Int,bool> condition,Action<Vector2Int> action)
    {
        for(int i=0; i < rangeSize; i++)
            for (int j = 0; j < rangeSize; j++)
            {
                if (i != attack.selectRange & j != attack.selectRange) continue;
                if(condition(new Vector2Int(i,j)))
                    action(new Vector2Int(i,j));
            }             
    }

 






}
