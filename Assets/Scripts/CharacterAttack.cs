using System.Collections;
using System.Collections.Generic;
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




    public void AttackAreaCreate(Character character,AttackInfo info,bool isPlayer = true)
    {
        ActionSelectUI.Instance.SelectBoxCancle();
        this.character = character;
        transform.position = character.transform.position;
        attack = info;
        AreaCreate(AreaType.Select,transform.position.To2DInt());
        if(isPlayer)
            rangeViewProgress = StartCoroutine(AreaRangeView());
    }

    public void AttackAreaRemove()
    {
        if(rangeViewProgress != null)
            StopCoroutine(rangeViewProgress);
        character = null;
        attack = null;
        AreaFieldClear(selectRangeList);
        AreaFieldClear(attackRangeList);
    }

    public bool AttackableCheck(Monster monster,AttackInfo info,Character target)
    {
        AttackAreaCreate(monster, info, false);
        bool result = false;
        selectRangeList.LoopDictionary((pair) =>
        {
            if (field.SurfaceState(pair.Key) == CUBE_TYPE.OnCharacter)
            {
                field.Surface(pair.Key, out Vector3Int coord);
                if (field.CubeDataCall(coord).onChracter == target)
                {
                    result = true;
                }                 
            }            
        }
        );
        AttackAreaRemove();
        return result;
    }

    public YieldInstruction MonsterAttack(Character target)
    {
        AreaView center = null;
        selectRangeList.LoopDictionary((key) =>
        {
            if (field.SurfaceState(key) == CUBE_TYPE.OnCharacter)
            {
                field.Surface(key, out Vector3Int coord);
                if (field.CubeDataCall(coord).onChracter == target)
                {
                    RangeAreaCreate(key, ref center);
                }
            }
        });
        return AttackAction(center.transform.position, false);
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
        areaDic.LoopDictionary((target) =>
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
            center = AreaViewManager.Instance.CallAreaView(Vector3Int.RoundToInt(target));
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
                    AttackAction(center.transform.position);
                    yield break;
                }
            }
            yield return null;
        }
    }

    private void AreaFieldClear(Dictionary<Vector2Int, AreaView> target)
    {
        target.LoopDictionary((view) => view.Return());
        target.Clear();
    }

    private YieldInstruction AttackAction(Vector3 target, bool isPlayer = true)
    {
        character.transform.LookAt(new Vector3(target.x, character.transform.position.y, target.z));
        Dictionary<AttackInfo, MotionPlayer> motionDic = null;
        if (isPlayer)
            motionDic = PlayerMotionManager.Instance.attackMotions;
        else
            motionDic = PlayerMotionManager.Instance.monsterMotions;
        AreaFieldClear(selectRangeList);
        return motionDic[attack].Play(character.Animator, target, () =>
        {
            print("공격"); // 이부분 커스텀
            attackRangeList.LoopDictionary((key) => 
            { 
                if(field.SurfaceState(key) == CUBE_TYPE.OnCharacter)
                {
                    field.Surface(key, out Vector3Int target);
                    Character targetChar = field.CubeDataCall(target).onChracter;
                    Monster monster =  targetChar as Monster;
                    if (isPlayer)
                    {                     
                        if (monster != null)
                            targetChar.Animator.SetInteger(AnimationHash.animation, 8);
                    }
                    else
                    {
                        if(monster == null)
                            targetChar.Animator.SetInteger(AnimationHash.animation, 8);

                    }
                    targetChar.Animator.Update(0);
                    targetChar.Animator.SetInteger(AnimationHash.animation,0);
                }        
            });


            AreaFieldClear(attackRangeList);
            rangeViewProgress = null;
            character.actionable[0] = false;
            InputManager.Instance.InputReset(isPlayer);
        });

    }


}
