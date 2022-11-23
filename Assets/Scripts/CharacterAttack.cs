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


    //public Transform order;
    public Character character;
    private AttackInfo attack;

    private RaycastHit hit;
    [SerializeField] private LayerMask mask;

    public void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.J))
        {
            AttackAreaCreate(character);
            AreaCreate(AreaType.Select, FIELD_SHAPE.Square, transform.position.To2DInt());
        }
        if(Input.GetKeyDown(KeyCode.H))
            if (selectRangeList.Count != 0)
            {
                if (rangeViewProgress != null)
                {
                    StopCoroutine(rangeViewProgress);
                    AreaFieldClear(attackRangeList);
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


    public void AttackAreaCreate(Character character)
    {
        transform.position = character.transform.position;
        attack = character.attackList[0];
        //AreaViewManager.Instance.CallAreaField(transform.position.To2DInt(), attack.selectRange, selectRangeList);
    }

    public void AreaCreate(AreaType targetArea,FIELD_SHAPE shape,Vector2Int origin)
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

        AreaFieldClear(areaDic);
        AreaViewManager.Instance.CallAreaFieldShape(origin, range, areaDic, shape);
        areaDic.LoopDictionaryValue((target) =>
        {
            if (AreaCheck(origin, target, cubeRayMask, isHeightAllow, isTargetOnly))
                target.SetType(type);
        }) ;

        //switch (shape)
        //{
        //    case FIELD_SHAPE.Square:
        //        foreach (KeyValuePair<Vector2Int, AreaView> view in areaDic)
        //        {
        //            if (AreaCheck(origin, view.Key, areaDic, cubeRayMask, isHeightAllow, isTargetOnly))
        //                view.Value.SetType(type);
        //        }
        //        break;
        //    case FIELD_SHAPE.Cross:
        //        CubeCheck.CustomCheck(CHECK_TYPE.CROSS, origin, range,
        //            (target) => { return AreaCheck(origin, target, areaDic, cubeRayMask, isHeightAllow, isTargetOnly); },
        //            (target) => areaDic[target].SetType(type));
        //        break;
        //    case FIELD_SHAPE.Diagonal:
        //        CubeCheck.DiagonalCheck(origin, range,
        //            (target) => { return AreaCheck(origin, target, areaDic, cubeRayMask, isHeightAllow, isTargetOnly); },
        //            (target) => areaDic[target].SetType(type));
        //        break;
        //}
    }
    //private bool AreaCheck(Vector2Int origin2D,Vector2Int target,Dictionary<Vector2Int, AreaView> areaDic, CUBE_TYPE[] cubeRayMask,bool isHeightAllow,bool isTargetOnly)
    //{
    //    field.Surface(origin2D, out Vector3Int origin);     
    //    if (areaDic.TryGetValue(target, out AreaView area))
    //    {
    //        Vector3Int areaPosition = area.transform.position.ToInt();
    //        if (cubeRayMask.Length != 0)
    //            if (CubeCheck.CubeRayCast(origin,areaPosition, cubeRayMask))
    //                return false;
    //        if (!isHeightAllow)
    //            if (origin.y != areaPosition.y)
    //                return false;
    //        if (isTargetOnly)
    //            if (field.SurfaceState(new Vector2Int(areaPosition.x,areaPosition.z))!= CUBE_TYPE.OnCharacter)
    //                return false;
    //        return true;
    //    }
    //    return false;
    //}

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
                                AreaFieldClear(attackRangeList);
   


                            character.transform.LookAt(new Vector3(curPos.x,character.transform.position.y,curPos.y));
                            //AreaViewManager.Instance.CallAreaField(curPos, attack.attakcRange, attackRangeList);
                            AreaCreate(AreaType.Attack, FIELD_SHAPE.Square, curPos);
                            attackRangeList.TryGetValue(curPos, out center);
                            center.SetState(TILE_TYPE.Disable);
 
                            prevPos = curPos;
                        }
            }
            if(Input.GetMouseButtonDown(0))
            {
                //attackRangeList.LoopDictionaryValue((view) => view.Invisible());
                AreaFieldClear(selectRangeList);
                PlayerMotionManager.Instance.attackMotions[attack].Play(character.animator, center.transform.position, () => 
                { 
                    print("공격"); // 이부분 커스텀
                    AreaFieldClear(attackRangeList);
                    rangeViewProgress = null;
                });
                yield break;
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
