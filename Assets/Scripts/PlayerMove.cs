using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public class PlayerMove : MonoBehaviour,IInputEventable
{
    [SerializeField] private ConfirmUI confirmBox;
    [SerializeField] private LayerMask mask;
    [BoxGroup("TargetInfo")] public Character character;
    [BoxGroup("TargetInfo")] public Animator orderAnimator;
    [BoxGroup("TargetInfo")] public Transform order;
    [BoxGroup("TargetInfo")] public int movePoint;
    [BoxGroup("TargetInfo")] public int jumpHeight;

    private Dictionary<Vector2Int, AreaView> moveField = new Dictionary<Vector2Int, AreaView>();
    private List<AreaView> selectedList = new List<AreaView>();
    private List<AreaView> nextAreaList = new List<AreaView>(4);
    private List<AreaView> activeArea = new List<AreaView>();
    private List<AreaView> removeArea = new List<AreaView>();
    private Vector3[] wayPoints = new Vector3[4];

    private FieldGenerator field;

    private int moveCount;
    private bool isComplete = false;
    private bool isPass = false;
    private WaitUntil moveDelay;
    private float moveSpeed;
    private new int animation { get { return PlayerMotionManager.Instance.parameterAnimation; } }


    private Coroutine selectArea;
    private Coroutine moveProgress;


    public void Awake()
    {
        field = FieldGenerator.Instance;
        moveDelay = new WaitUntil(() => isComplete);
    }


    //public void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.K))
    //    {
            
    //        CreatMoveField(order, movePoint);
    //        MoveableArea(moveField[order.position.To2DInt()]);
    //    }
    //    MoveAreaSelect();
    //    if (moveProgress == null)
    //        MoveProgressing();
    //}


    public void InputStart()
    {
        CreatMoveField(order, movePoint);
        MoveableArea(moveField[order.position.To2DInt()]);
        selectArea = StartCoroutine(MoveAreaSelect());
    }

    public void InputBreak()
    {

    }



    public void CreatMoveField(Transform order,int moveableRange)
    {
        this.order = order;
        order.TryGetComponent(out orderAnimator);
        order.TryGetComponent(out character);
        movePoint = character.moveablePoint;
        jumpHeight = character.jumpableHeight;
        moveCount = movePoint;

        RemoveArea();
        Vector2Int playerPos = order.position.To2DInt();
        AreaViewManager.Instance.CallAreaField(playerPos, moveableRange, moveField);

        for(int i = -moveableRange; i<=moveableRange;i++)
            for(int j =-moveableRange;j<=moveableRange;j++)
            {
                int x = i < 0 ? -i : i;
                int z = j < 0 ? -j : j;
                if (x + z > moveableRange)
                    continue;                  
                Vector2Int targetPos = playerPos + new Vector2Int(i, j);
                switch(field.SurfaceState(targetPos))
                {
                    case CUBE_TYPE.Air:
                        activeArea.Add(moveField[targetPos]);
                        moveField[targetPos].SetType(TILE_TYPE.Active);
                        break;

                    case CUBE_TYPE.Water:
                        activeArea.Add(moveField[targetPos]);
                        moveField[targetPos].SetType(TILE_TYPE.Passable);
                        break;
                    case CUBE_TYPE.Out:
                        break;
                    default:
                        moveField[targetPos].SetType(TILE_TYPE.Disable);
                        break;
                }
                if (i == 0 & j == 0)
                    selectedList.Add(moveField[targetPos]);
            }

        //bool isFixed = true;
        //while(isFixed)
        //{
        //    isFixed = false;
        //    foreach (AreaView area in activeArea)
        //        if (!CrossHeightCheck(area.transform.position.To2DInt(), jumpHeight))
        //        {
        //            area.SetType(TILE_TYPE.Default);
        //            removeArea.Add(area);
        //            isFixed = true;
        //        }
        //    foreach(AreaView area in removeArea)
        //        activeArea.Remove(area);
        //    removeArea.Clear();
        //}

        foreach (AreaView area in activeArea)
            if(!CrossObstacleCheck(area.transform.position.To2DInt()))
            {
                area.SetType(TILE_TYPE.Default);
                removeArea.Add(area);
            }
        foreach (AreaView area in removeArea)
            activeArea.Remove(area);
        removeArea.Clear();

    }

    public IEnumerator MoveAreaSelect()
    {
        bool isPause = false;
        while(true)
        {
            if(!isPause)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (moveCount != 0)
                    {
                        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, mask))
                            if (hit.transform.TryGetComponent(out AreaView view))
                                if (view.curState == TILE_TYPE.Enable)
                                {
                                    moveCount--;
                                    view.SetState(TILE_TYPE.Selected);
                                    selectedList.Add(view);
                                    AreaDeActive();
                                    if (moveCount != 0)
                                        MoveableArea(view);
                                }
                    }
                    else
                    {
                        if (GameManager.Instance.isConfirmIgnore)
                        {
                            isPause = true;
                            MoveProgressing();
                        }
                        else
                        {
                            isPause = true;
                            confirmBox.yesClickAction = () => { MoveProgressing(); };
                            confirmBox.noClickAction = () => isPause = false;
                            confirmBox.transform.position = order.position;
                            confirmBox.transform.rotation = Camera.main.transform.rotation;
                            confirmBox.gameObject.SetActive(true);
                        }
                    }
                }
                if (Input.GetMouseButtonDown(1) & !isPause)
                    if (selectedList.Count > 1)
                    {
                        moveCount++;
                        AreaView lastView = selectedList[selectedList.Count - 1];
                        lastView.SetState(lastView.curType);
                        selectedList.Remove(lastView);
                        AreaDeActive();
                        MoveableArea(selectedList[selectedList.Count - 1]);
                    }
            }
            yield return null;
        }
    }



    public void MoveProgressing()
    {
        StopCoroutine(selectArea);
        selectArea = null;
        moveProgress = StartCoroutine(Move());
    }


    public IEnumerator Move()
    {
        field.Cube(order.position.ToInt()).data.onChracter = null;
        field.Cube(order.position.ToInt()).type = CUBE_TYPE.Air;
        InvisibleArea();
        wayPoints[0] = selectedList[0].transform.position;
        CameraManager.Instance.VCMove(true);
        for (int i = 1; i < selectedList.Count; i++)
        {
            if (selectedList[i].curType == TILE_TYPE.Passable)
            {
                isPass = true;
                continue;
            }

            /////////////////////이동 설정 초기화///////////////////
            moveSpeed = 0.5f;
            for (int j = 1; j < wayPoints.Length; j++)
                wayPoints[j] = selectedList[i].transform.position;
            //////////////////////////////////////////////////////


            /////////////////////이동경로 설정/////////////////////
            float height = wayPoints[3].y - wayPoints[0].y;
            if (height > 0)
            {
                wayPoints[1] = (wayPoints[3] + wayPoints[0]) / 2 + new Vector3(0, height / 2 + 0.5f, 0);
                moveSpeed = 0.7f;
            }
            else if (height < 0)
            {
                Vector3 temp = wayPoints[3] - wayPoints[0];
                wayPoints[1] = wayPoints[0] + new Vector3(temp.x = temp.x > 1 ? 0.5f : 0, 0, temp.z = temp.z > 1 ? 0.5f : 0);
                wayPoints[2] = new Vector3(wayPoints[3].x, wayPoints[0].y - 0.3f, wayPoints[3].z);
                moveSpeed = 0.6f;
            }
            else if (isPass)
            {
                wayPoints[1] = (wayPoints[3] + wayPoints[0]) / 2 + new Vector3(0, 0.2f, 0);
                moveSpeed = 0.6f;
            }
            //////////////////////////////////////////////////////

            ///////////////////이동방식 설정///////////////////////
            PathType pathType;
            if (height != 0 | isPass)
                pathType = PathType.CatmullRom;
            else
                pathType = PathType.Linear;
            //////////////////////////////////////////////////////

            ////////////////////////이동//////////////////////////
            isComplete = false;
            if ((order.transform.forward + order.transform.position).To2DInt() != wayPoints[3].To2DInt())
                yield return order.DOLookAt(new Vector3(wayPoints[3].x, order.transform.position.y, wayPoints[3].z), 0.3f).WaitForCompletion();

            //////////////////애니메이션 설정//////////////////////

            if (isPass | height != 0)
                orderAnimator.SetInteger(animation, 16);
            else
                orderAnimator.SetInteger(animation, 20);
            //////////////////////////////////////////////////////


            order.transform.DOPath(wayPoints, moveSpeed, pathType).SetEase(Ease.Linear).OnComplete(() =>
            {                                        
                wayPoints[0] = wayPoints[3];          
                isComplete = true;
                isPass = false;
            });
            yield return moveDelay;
        }
        selectedList.Clear();
        orderAnimator.SetInteger(animation, 0);


        Vector3Int endPos = order.position.ToInt();
        print(endPos);
        field.Cube(endPos).type = CUBE_TYPE.OnCharacter;
        if (field.Cube(endPos).data == null)
            field.Cube(endPos).data = new CubeData(character);
        field.Cube(endPos).data.onChracter = character;


        CameraManager.Instance.VCMove(false);
        moveCount = movePoint;
        moveProgress = null;
        CameraManager.Instance.InputStart();
    }







    public void RemoveArea()
    {
        foreach (KeyValuePair<Vector2Int, AreaView> view in moveField)
            view.Value.Return();
        moveField.Clear();
        activeArea.Clear();
    }

    public void InvisibleArea()
    {
        foreach (KeyValuePair<Vector2Int, AreaView> view in moveField)
            view.Value.Invisible();
    }



    public void CrossCheck(Vector2Int origin, int range, Func<Vector2Int, bool> condition, Action<Vector2Int> trueAction)
    {
        CubeCheck.CustomCheck(CHECK_TYPE.CROSS,origin,range,condition, trueAction);
    }

    public bool CrossHeightCheck(Vector2Int origin,int height)
    {
        bool result = false;
        float originHeigt = 0;
        int range = 1;
        if (moveField.TryGetValue(origin, out AreaView originArea))
            originHeigt = originArea.transform.position.y;
        else
            return false;

        CrossCheck(origin, range,
            //CheckFunc//
            (checkPoint) =>
            { 
                if(moveField.TryGetValue(checkPoint,out AreaView checkArea))
                {
                    if(activeArea.Contains(checkArea))
                    {
                        float heightDistance = originHeigt - checkArea.transform.position.y;
                        if (heightDistance >= -height & heightDistance <= height)
                            return true;
                    }
                }
                return false;
            },
            //ActiveFunc//
            (none) => { result = true; }
            );

        return result;
    }

    public bool CrossObstacleCheck(Vector2Int origin)
    {
        bool result = false;
        CrossCheck(origin,1,
            (checkPoint)=>
            {
                if (moveField.TryGetValue(checkPoint, out AreaView checkArea))
                    if (activeArea.Contains(checkArea))
                        return true;
                return false;
            },
            (none) => { result = true; }
            );
        return result;
    }








    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////           ActiveMoveArea Method           ///////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region ActiveMoveAreaPart

    public void MoveableArea(AreaView origin)=> CrossCheck(origin.transform.position.To2DInt(),1,(target) => MoveableCheck(target, AreaTypeCheck), AreaActivation);
    public bool MoveableCheck(Vector2Int target,Func<AreaView,bool> condition)
    {
        if (moveField.TryGetValue(target, out AreaView checkPoint))
        {
            if (!selectedList.Contains(checkPoint))
                if (activeArea.Contains(checkPoint))
                    if (CrossHeightCheck(target, jumpHeight))
                        return condition(checkPoint);

            return false;
        }
        else
            return false;
    }

    public bool AreaTypeCheck(AreaView checkPoint)
    {
        bool isMoveable = false;
        if (checkPoint.curType == TILE_TYPE.Active)
            isMoveable = true;
        if(checkPoint.curType == TILE_TYPE.Passable)
            if(moveCount>0)
                CrossCheck(checkPoint.transform.position.To2DInt(), moveCount - 1, (origin) => PassableCheck(origin, checkPoint), (none) => isMoveable = true);
        return isMoveable;             
    }

    public bool PassableCheck(Vector2Int checkPoint,AreaView exception)
    {
        if(moveField.TryGetValue(checkPoint,out AreaView view))
        {
            if (view != exception)
                if (!selectedList.Contains(view))
                    if (view.curType == TILE_TYPE.Active)
                        return true;
            print(view.curType);
        }
        return false;
    }







    public void AreaActivation(Vector2Int target)
    {
        moveField.TryGetValue(target, out AreaView activeArea);
        nextAreaList.Add(activeArea);
        activeArea.SetState(TILE_TYPE.Enable);
    }

    public void AreaDeActive()
    {
        foreach (var area in nextAreaList)
            if (area.curState != TILE_TYPE.Selected)
                area.SetState(area.curType);
        nextAreaList.Clear();
    }

    #endregion






}
