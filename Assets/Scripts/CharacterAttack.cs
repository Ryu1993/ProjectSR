using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class CharacterAttack : MonoBehaviour
{
    private FieldGenerator field { get { return FieldGenerator.Instance; } }

    public AttackInfo attack;

    private Dictionary<Vector2Int, AreaView> selectRangeList = new Dictionary<Vector2Int, AreaView>();
    private List<AreaView> attackRangeList = new List<AreaView>();
    public Coroutine rangeViewProgress;
    public Transform test;

    private RaycastHit hit;
    [SerializeField] private LayerMask mask;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            AttackAreaCreate(test);
            RangeAttackArea();
        }
        if(Input.GetKeyDown(KeyCode.H))
            if (selectRangeList.Count != 0)
            {
                if (rangeViewProgress != null)
                {
                    StopCoroutine(rangeViewProgress);
                    RangeListClear();
                    rangeViewProgress = null;
                }
                else
                {
                    rangeViewProgress = StartCoroutine(RangeAreaView());
                }
            }

    }

    public void AttackAreaCreate(Transform character)
    {
        transform.position = character.position;
        AreaViewManager.Instance.CallAreaField(transform.position.To2DInt(), attack.selectRange, selectRangeList);




    }

    public void CrossAttackArea(Vector2Int origin,int range)
    {
        CubeCheck.CustomCheck(CHECK_TYPE.CROSS, origin, range,
            (target) => selectRangeList.TryGetValue(target, out AreaView area), 
            (target) => selectRangeList[target].SetType(TILE_TYPE.Enable));
    }

    public void DiagonalAttackArea(Vector2Int origin, int range)
    {
        CubeCheck.DiagonalCheck(origin, range,
            (target) => selectRangeList.TryGetValue(target, out AreaView area),
            (target) => selectRangeList[target].SetType(TILE_TYPE.Enable));
    }

    public void RangeAttackArea()
    {
        foreach (KeyValuePair<Vector2Int, AreaView> view in selectRangeList)
            view.Value.SetType(TILE_TYPE.Enable);
    }

    

    public IEnumerator RangeAreaView()
    {
        Vector2Int prevPos = new Vector2Int(-100, -100);
        while(true)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit,Mathf.Infinity,mask))
                if(hit.transform.TryGetComponent(out AreaView view))
                {
                    Vector2Int curPos = view.transform.position.To2DInt();
                    if(prevPos!=curPos)
                    {
                        if (attackRangeList.Count != 0)
                            RangeListClear();
                        AreaViewManager.Instance.CallAreaField(curPos, attack.attakcRange, attackRangeList);

                        RangeListSetColor(TILE_TYPE.Selected,curPos);

                        prevPos = curPos;
                    }
                }
            yield return null;
        }
    }


    private void RangeListClear()
    {
        foreach (AreaView view in attackRangeList)
            view.Return();
        attackRangeList.Clear();
    }
    private void RangeListSetColor(TILE_TYPE type, Vector2Int origin)
    {
        foreach (AreaView view in attackRangeList)
        {
            view.transform.position += new Vector3(0, 0.1f, 0);
            view.SetState(type);
            if (view.transform.position.To2DInt() == origin)
                view.SetState(TILE_TYPE.Disable);

        }
            
    }




}
