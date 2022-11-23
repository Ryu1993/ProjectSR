using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterMove : MonoBehaviour
{

    private FieldGenerator field { get { return FieldGenerator.Instance; } }
    private Dictionary<Vector2Int, AreaView> moveField = new Dictionary<Vector2Int, AreaView>();
    private List<AreaView> selectedArea = new List<AreaView>();
    private List<AreaView> enableArea = new List<AreaView>(4);
    private Vector3[] wayPoints= new Vector3[3];
    private AreaView lastSelectedView;
    private AreaView rayCastHitView;
    private Character curCharacter;
    private Animator chrAnimator;
    private Coroutine pathSelect;
    private RaycastHit hit;

    private readonly int animatorParam = Animator.StringToHash("animation");
    private int jumpheight;
    private int moveCount;
    private int maxMoveCount;

    public LayerMask mask;
    public ConfirmUI confirmUI;
    public Camera mainCam;

    //public void Update()
    //{

    //    if (Input.GetKeyDown(KeyCode.K) & pathSelect == null)
    //        Move(curCharacter);

    //}


    public void Move(Character character)
    {
        CreateMoveField(character);
        pathSelect = StartCoroutine(PathSelect());
    }

    public void ReMove()
    {
        curCharacter = null;
        StopCoroutine(pathSelect);
        AreaReset();
    }

    private void AreaReset()
    {
        moveField.LoopDictionaryValue((view) => view.Return());
        moveField.Clear();
        selectedArea.Clear();
        enableArea.Clear();
    }

    private void AreaDisable()
    {
        moveField.LoopDictionaryValue((view) => view.Invisible());
    }


    #region MoveSelectBehaviour

    private IEnumerator PathSelect()
    {
        selectedArea.Clear();
        EnableCheck(curCharacter.transform.position.To2DInt());
        while(true)
        {
            if (moveCount > 0)
                PathClick();
            if (selectedArea.Count > 0 & !confirmUI.gameObject.activeSelf)
                DoMoveWindowActive();
            yield return null;
        }
    }

    private void PathClick()
    {
        if(Input.GetMouseButtonDown(0))
            if(Physics.Raycast(mainCam.ScreenPointToRay(Input.mousePosition),out hit,Mathf.Infinity,mask))
                if(hit.transform.TryGetComponent(out rayCastHitView))
                    if (rayCastHitView.curState == TILE_TYPE.Enable)
                    {
                        moveCount--;
                        foreach (AreaView areaView in enableArea)
                            areaView.SetState(areaView.curType);
                        enableArea.Clear();
                        rayCastHitView.SetState(TILE_TYPE.Selected);
                        selectedArea.Add(rayCastHitView);
                        EnableCheck(rayCastHitView.transform.position.To2DInt());
                        InputManager.Instance.CancleBehaviour.Push(PathCancle);
                    }
    }
    private void PathCancle()
    {
        moveCount++;
        foreach (AreaView areaView in enableArea)
            areaView.SetState(areaView.curType);
        enableArea.Clear();
        lastSelectedView = selectedArea.Last();
        lastSelectedView.SetState(lastSelectedView.curType);
        selectedArea.Remove(lastSelectedView);
        Vector2Int targetCood = Vector2Int.zero;
        if (selectedArea.Count == 0)
            targetCood = curCharacter.transform.position.To2DInt();
        else
            targetCood = selectedArea.Last().transform.position.To2DInt();
        EnableCheck(targetCood);
    }

    private void DoMoveWindowActive()
    {
        confirmUI.yesClickAction = () => { StopCoroutine(pathSelect); StartCoroutine(CharacterMovement()); confirmUI.gameObject.SetActive(false); };
        confirmUI.transform.position = curCharacter.transform.position;
        confirmUI.gameObject.SetActive(true);
    }




    #endregion

    #region CharacterMoveBehaviour


    private IEnumerator CharacterMovement()
    {
        AreaDisable();

        Transform characterTransform = curCharacter.transform;
        Vector3Int characterPosition = characterTransform.position.ToInt();
        Vector3 prevPosition = characterPosition;
        bool isPass = false;

        field.CubeDataCall(characterPosition).onChracter = null;
        field.Cube(characterPosition).type = CUBE_TYPE.Air;

        for (int i =0; i<selectedArea.Count;i++)
        {
            if (selectedArea[i].curType == TILE_TYPE.Passable)
            {
                isPass = true;
                continue;
            }

            PathType pathType = PathType.Linear;
            Vector3 targetPosition = selectedArea[i].transform.position.ToInt();
            float heightDistance = targetPosition.y - prevPosition.y;
            float moveSpeed = 0.5f;
              
            if (heightDistance!=0|isPass)
            {
                chrAnimator.SetInteger(animatorParam, 16);
                wayPoints[0] = targetPosition;
                wayPoints[1] = prevPosition;
                wayPoints[2] = targetPosition;
                pathType = PathType.CubicBezier;
                moveSpeed = 0.7f;
            }
            if(heightDistance==0&!isPass)
            {
                chrAnimator.SetInteger(animatorParam, 20);
                for (int j = 0; j < wayPoints.Length; j++)
                    wayPoints[j] = targetPosition;           
            }


            if ((characterTransform.forward + characterTransform.position).To2DInt() != targetPosition.To2DInt())
                yield return characterTransform.DOLookAt(new Vector3(targetPosition.x, characterTransform.position.y, targetPosition.z), 0.3f).WaitForCompletion();

            yield return characterTransform.DOPath(wayPoints, moveSpeed, pathType).OnComplete(() => 
            {
                isPass = false;
                prevPosition = targetPosition; 
            }).WaitForCompletion();
        }
        chrAnimator.SetInteger(animatorParam, 0);

        Vector3Int endPosition = prevPosition.ToInt();
        field.CubeDataCall(endPosition).onChracter = curCharacter;
        field.Cube(endPosition).type = CUBE_TYPE.OnCharacter;

        AreaReset();
        pathSelect = null;
    }

    #endregion

    #region MoveSelectCheck
    private void EnableCheck(Vector2Int origin)
    {
        float originHeight = moveField[origin].transform.position.y;
        CubeCheck.CustomCheck(CHECK_TYPE.CROSS, origin, 1,
            (checkPoint) =>
            {
                if (moveField.TryGetValue(checkPoint, out AreaView view))
                    if(!selectedArea.Contains(view))
                    {
                        float heightDistance = originHeight - view.transform.position.y;
                        if (heightDistance >= -jumpheight & heightDistance <= jumpheight)
                        {
                            if (view.curType == TILE_TYPE.Active)
                                return true;
                            if (view.curType == TILE_TYPE.Passable)
                            {
                                if (moveCount == 0)
                                    return false;
                                return SubEnableCheck(checkPoint);
                            }
                        }
                    }
                return false;
            },
            (checkPoint) =>
            {
                moveField[checkPoint].SetState(TILE_TYPE.Enable);
                enableArea.Add(moveField[checkPoint]);
            }
            );
    }

    private bool SubEnableCheck(Vector2Int origin)
    {
        bool result = false;
        CubeCheck.CustomCheck(CHECK_TYPE.CROSS, origin, moveCount,
            (checkPoint) =>
            {
                if (moveField.TryGetValue(checkPoint, out AreaView view))
                    if (!selectedArea.Contains(view))
                        if (view.curType == TILE_TYPE.Active)
                            return true;
                return false;
            },
            (none) => result = true
            );
        return result;

    }

    #endregion

    #region MoveFieldCreate

    public void CreateMoveField(Character order)
    {
        curCharacter = order;
        jumpheight = order.jumpableHeight;
        maxMoveCount = order.moveablePoint;
        order.TryGetComponent(out chrAnimator);
        moveCount = maxMoveCount;
        moveField.Clear();
        int moveRange = order.moveablePoint;
        Vector2Int orderPosition2D = order.transform.position.To2DInt();
        AreaViewManager.Instance.CallAreaFieldShape(orderPosition2D,moveRange ,moveField, FIELD_SHAPE.RangeLimitSquare);
        PassableCheck();
        print(moveField.Count);
    }

    private void PassableCheck()
    {
        moveField.LoopDictionary((pair) =>
        {
            CUBE_TYPE type = field.SurfaceState(pair.Key);
            switch (type)
            {
                case CUBE_TYPE.Water:
                    pair.Value.SetType(TILE_TYPE.Passable);
                    break;
                case CUBE_TYPE.Air:
                    pair.Value.SetType(TILE_TYPE.Active);
                    break;
                default:
                    pair.Value.SetType(TILE_TYPE.Disable);
                    break;
            }
        });
    }
    #endregion



}
