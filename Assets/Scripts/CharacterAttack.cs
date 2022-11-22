using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;



public enum AreaShape { Square,Cross, Diagonal}
public enum AreaType { Select, Attack };
public class CharacterAttack : MonoBehaviour,IInputEventable
{


    private FieldGenerator field { get { return FieldGenerator.Instance; } }

    

    private Dictionary<Vector2Int, AreaView> selectRangeList = new Dictionary<Vector2Int, AreaView>();
    private Dictionary<Vector2Int, AreaView> attackRangeList = new Dictionary<Vector2Int, AreaView>();
    public Coroutine rangeViewProgress;


    public Transform order;
    public Character character;
    public AttackInfo attack;

    private RaycastHit hit;
    [SerializeField] private LayerMask mask;

    public void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.J))
        {
            AttackAreaCreate(order);
            AreaCut(AreaType.Select, AreaShape.Square, transform.position.To2DInt());
        }
        if(Input.GetKeyDown(KeyCode.H))
            if (selectRangeList.Count != 0)
            {
                if (rangeViewProgress != null)
                {
                    StopCoroutine(rangeViewProgress);
                    RangeListClear(attackRangeList);
                    rangeViewProgress = null;
                }
                else
                {
                    rangeViewProgress = StartCoroutine(RangeAreaView());
                }
            }

    }

    public void InputStart()
    {

    }
    public void InputBreak()
    {

    }


    public void AttackAreaCreate(Transform character)
    {
        transform.position = character.position;
        attack = order.GetComponent<Character>().attackList[0];
        AreaViewManager.Instance.CallAreaField(transform.position.To2DInt(), attack.selectRange, selectRangeList);
    }

    public void AreaCut(AreaType targetArea,AreaShape shape,Vector2Int origin)
    {
        Dictionary<Vector2Int, AreaView> areaDic = null;
        TILE_TYPE type = TILE_TYPE.Default;
        CUBE_TYPE[] cubeRayMask = null;
        bool isHeightAllow = true;
        bool isTargetOnly = false;
        int range = 0;
        switch (targetArea)
        {
            case AreaType.Select:
                isHeightAllow = attack.selectHeightAllow;
                isTargetOnly = attack.targetOnly;
                cubeRayMask = attack.selectCubeMask;
                range = attack.selectRange;
                areaDic = selectRangeList;
                type = TILE_TYPE.Enable;
                break;
            case AreaType.Attack:
                isHeightAllow = attack.attakcHeightAllow;
                cubeRayMask = attack.attackCubeMask;
                range = attack.attakcRange;
                areaDic = attackRangeList;
                type = TILE_TYPE.Selected;
                break;
        }
        switch(shape)
        {
            case AreaShape.Square:
                foreach (KeyValuePair<Vector2Int, AreaView> view in areaDic)
                {
                    if (AreaCheck(origin, view.Key, areaDic,cubeRayMask,isHeightAllow, isTargetOnly))
                        view.Value.SetType(type);
                }
                break;
            case AreaShape.Cross:
                CubeCheck.CustomCheck(CHECK_TYPE.CROSS, origin, range,
                    (target) => { return AreaCheck(origin, target, areaDic,cubeRayMask ,isHeightAllow, isTargetOnly); },
                    (target) => areaDic[target].SetType(type));
                break;
            case AreaShape.Diagonal:
                CubeCheck.DiagonalCheck(origin, range,
                    (target) => { return AreaCheck(origin, target, areaDic,cubeRayMask ,isHeightAllow, isTargetOnly); },
                    (target) => areaDic[target].SetType(type));
                break;
        }
    }
    private bool AreaCheck(Vector2Int origin2D,Vector2Int target,Dictionary<Vector2Int, AreaView> areaDic, CUBE_TYPE[] cubeRayMask,bool isHeightAllow,bool isTargetOnly)
    {
        field.Surface(origin2D, out Vector3Int origin);     
        if (areaDic.TryGetValue(target, out AreaView area))
        {
            if (cubeRayMask.Length != 0)
                if (CubeCheck.CubeRayCast(origin, area.transform.position.ToInt(), cubeRayMask))
                    return false;
            if (!isHeightAllow)
                if (origin.y != Mathf.RoundToInt(area.transform.position.y))
                    return false;
            if (isTargetOnly)
                if (field.SurfaceState(area.transform.position.To2DInt()) != CUBE_TYPE.OnCharacter)
                    return false;
            return true;
        }
        return false;
    }


    public IEnumerator RangeAreaView()
    {
        Vector2Int prevPos = new Vector2Int(-100, -100);
        Vector2Int curPos = Vector2Int.zero;
        AreaView center = null;
        while (true)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit,Mathf.Infinity,mask))
            {
                curPos = hit.transform.position.To2DInt();
                if(selectRangeList.TryGetValue(curPos, out AreaView areaView))
                    if (areaView.curType == TILE_TYPE.Enable)
                        if (prevPos != curPos)
                        {
                            if (attackRangeList.Count != 0)
                                RangeListClear(attackRangeList);
   


                            character.transform.LookAt(new Vector3(curPos.x,character.transform.position.y,curPos.y));
                            AreaViewManager.Instance.CallAreaField(curPos, attack.attakcRange, attackRangeList);
                            AreaCut(AreaType.Attack, AreaShape.Cross, curPos);
                            attackRangeList.TryGetValue(curPos, out center);
                            center.SetState(TILE_TYPE.Disable);
 
                            prevPos = curPos;
                        }
            }
            if(Input.GetMouseButtonDown(0))
            {
                LoopDictionary(attackRangeList, (view) => view.Invisible());
                RangeListClear(selectRangeList);
                PlayerMotionManager.Instance.attackMotions[attack].Play(character.animator, center.transform.position, () => 
                { 
                    print("공격"); // 이부분 커스텀
                    RangeListClear(attackRangeList);
                    rangeViewProgress = null;
                });
                yield break;
            }
            yield return null;
        }
    }


    private void LoopDictionary(Dictionary<Vector2Int, AreaView> target,Action<AreaView> action)
    {
        foreach (KeyValuePair<Vector2Int, AreaView> view in target)
            action(view.Value);
    }

    private void RangeListClear(Dictionary<Vector2Int, AreaView> target)
    {
        LoopDictionary(target, (view) => view.Return());
        target.Clear();
    }
    


}
