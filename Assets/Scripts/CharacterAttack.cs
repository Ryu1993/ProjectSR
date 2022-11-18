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
    private Dictionary<Vector2Int, AreaView> attackRangeList = new Dictionary<Vector2Int, AreaView>();
    public Coroutine rangeViewProgress;
    public Transform test;

    private RaycastHit hit;
    [SerializeField] private LayerMask mask;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            AttackAreaCreate(test);
            RangeAttackArea(transform.position.To2DInt(), attack.selectRange, selectRangeList, TILE_TYPE.Enable);
            //CrossAttackArea(transform.position.To2DInt(), attack.selectRange, selectRangeList,TILE_TYPE.Enable);
            //DiagonalAttackArea(transform.position.To2DInt(), attack.selectRange, selectRangeList, TILE_TYPE.Enable);
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

    public void CrossAttackArea(Vector2Int origin,int range,Dictionary<Vector2Int,AreaView> areaDic,TILE_TYPE type)
    {
        CubeCheck.CustomCheck(CHECK_TYPE.CROSS, origin, range,
            (target) => areaDic.TryGetValue(target, out AreaView area), 
            (target) => areaDic[target].SetType(type));
    }

    public void DiagonalAttackArea(Vector2Int origin, int range, Dictionary<Vector2Int, AreaView> areaDic, TILE_TYPE type)
    {
        CubeCheck.DiagonalCheck(origin, range,
            (target) => areaDic.TryGetValue(target, out AreaView area),
            (target) => areaDic[target].SetType(type));
    }

    public void RangeAttackArea(Vector2Int origin, int range, Dictionary<Vector2Int, AreaView> areaDic,TILE_TYPE type)
    {
        foreach (KeyValuePair<Vector2Int, AreaView> view in areaDic)
            view.Value.SetType(type);
    }

    

    public IEnumerator RangeAreaView()
    {
        Vector2Int prevPos = new Vector2Int(-100, -100);
        while(true)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit,Mathf.Infinity,mask))
            {
                Vector2Int curPos = hit.transform.position.To2DInt();
                if(selectRangeList.TryGetValue(curPos, out AreaView areaView))
                    if (areaView.curType == TILE_TYPE.Enable)
                        if (prevPos != curPos)
                        {
                            if (attackRangeList.Count != 0)
                                RangeListClear();
                            AreaViewManager.Instance.CallAreaField(curPos, attack.attakcRange, attackRangeList);


                            RangeListSetColor(TILE_TYPE.Selected, curPos, RangeAttackArea);


                            prevPos = curPos;
                        }
            }
            yield return null;
        }
    }


    private void RangeListClear()
    {
        foreach (KeyValuePair<Vector2Int, AreaView> view in attackRangeList)
            view.Value.Return();
        attackRangeList.Clear();
    }
    private void RangeListSetColor(TILE_TYPE type, Vector2Int origin,Action<Vector2Int,int,Dictionary<Vector2Int,AreaView>,TILE_TYPE> action)
    {
        action.Invoke(origin,attack.attakcRange,attackRangeList,type);
        attackRangeList.TryGetValue(origin, out AreaView areaView);
        areaView.SetState(TILE_TYPE.Disable);        
    }




}
