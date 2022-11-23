using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.IMGUI.Controls;
using UnityEngine;



public enum AreaShape { Square,Cross, Diagonal}
public enum AreaType { Select, Attack };
public class CharacterAttack : Singleton<CharacterAttack>
{
    private FieldGenerator field { get { return FieldGenerator.Instance; } }
    private Dictionary<Vector2Int, AreaView> selectRangeList = new Dictionary<Vector2Int, AreaView>();
    private Dictionary<Vector2Int, AreaView> attackRangeList = new Dictionary<Vector2Int, AreaView>();
    public Coroutine rangeViewProgress;
    public Character character;
    private AttackInfo attack;
    private RaycastHit hit;
    [SerializeField] private LayerMask mask;




    public void AttackAreaCreate(Character character,AttackInfo info)
    {
        this.character = character;
        transform.position = character.transform.position;
        attack = info;
        AreaCreate(AreaType.Select,transform.position.To2DInt());
        rangeViewProgress = StartCoroutine(AreaRangeView());      
    }

    public void AttackAreaRemove()
    {
        StopCoroutine(rangeViewProgress);
        character = null;
        attack = null;
        AreaFieldClear(selectRangeList);
        AreaFieldClear(attackRangeList);      
    }



    private void AreaCreate(AreaType targetArea,Vector2Int origin)
    {
        Dictionary<Vector2Int, AreaView> areaDic = null;
        TILE_TYPE type = TILE_TYPE.Default;
        FIELD_SHAPE shape = FIELD_SHAPE.Square;
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
                shape = attack.selectShape;
                break;
            case AreaType.Attack:
                isHeightAllow = attack.attakcHeightAllow;
                cubeRayMask = attack.attackCubeMask;
                range = attack.attakcRange;
                areaDic = attackRangeList;
                type = TILE_TYPE.Selected;
                shape = attack.attackShape;
                break;
        }

        AreaFieldClear(areaDic);
        AreaViewManager.Instance.CallAreaFieldShape(origin, range, areaDic, shape);
        areaDic.LoopDictionaryValue((target) =>
        {
            if (AreaCheck(origin, target, cubeRayMask, isHeightAllow, isTargetOnly))
                target.SetType(type);
        }) ;
    }


    private bool AreaCheck(Vector2Int origin2D,AreaView target, CUBE_TYPE[] cubeRayMask,bool isHeightAllow,bool isTargetOnly)
    {
        field.Surface(origin2D, out Vector3Int origin3D);
        Vector3Int targetPosition = target.transform.position.ToInt();
        if (cubeRayMask.Length != 0)
            if (CubeCheck.CubeRayCast(origin3D, targetPosition, cubeRayMask))
                return false;
        if (!isHeightAllow)
            if (origin3D.y != targetPosition.y)
                return false;
        if (isTargetOnly)
            if (field.Cube(targetPosition).type != CUBE_TYPE.OnCharacter)
                return false;
        return true;
    }

    private void RangeAreaCreate(Vector2Int centerPosition,ref AreaView center)
    {
        if(attack.attakcRange==0)
        {
            AreaFieldClear(attackRangeList);
            field.Surface(centerPosition, out Vector3 target);
            center = AreaViewManager.Instance.CallAreaView(target, null);
            attackRangeList.Add(centerPosition,center);
        }
        else
        {
            AreaCreate(AreaType.Attack, centerPosition);
            attackRangeList.TryGetValue(centerPosition, out center);
        }
        center.SetState(TILE_TYPE.Disable);
    }






    public IEnumerator AreaRangeView()
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
                                AreaFieldClear(attackRangeList);
                            character.transform.LookAt(new Vector3(curPos.x,character.transform.position.y,curPos.y));
                            prevPos = curPos;

                            RangeAreaCreate(curPos, ref center);
                        }
            }
            if(Input.GetMouseButtonDown(0))
            {
                if(attackRangeList.Count!=0)
                {
                    AreaFieldClear(selectRangeList);
                    PlayerMotionManager.Instance.attackMotions[attack].Play(character.Animator, center.transform.position, () =>
                    {
                        print("공격"); // 이부분 커스텀

                        AreaFieldClear(attackRangeList);
                        rangeViewProgress = null;
                        character.actionable[0] = false;
                        InputManager.Instance.InputReset();
                    });
                    yield break;
                }
            }
            yield return null;
        }
    }

    private void AreaFieldClear(Dictionary<Vector2Int, AreaView> target)
    {
        target.LoopDictionaryValue((view) => view.Return());
        target.Clear();
    }
    


}
