//using DG.Tweening;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using static UnityEngine.GraphicsBuffer;

//public class TestPlayerMove : MonoBehaviour
//{
//    public Transform player;
//    public FieldGenerator field;
//    public int movePoint;
//    public int jumpHeight;
//    public Dictionary<Vector2Int, AreaView> areaViewDic = new Dictionary<Vector2Int, AreaView>();
//    [HideInInspector]
//    public Stack<AreaView> areaViewStack = new Stack<AreaView>();
//    public List<AreaView> areaViewList = new List<AreaView>();
//    [HideInInspector]
//    public List<AreaView> curAreaViewList = new List<AreaView>();
//    public LayerMask mask;
//    private RaycastHit hit;
//    private Coroutine rmove;

//    private void Update()
//    {
//        if(Input.GetKeyDown(KeyCode.K))
//        {
//            MoveAreaCreate();
//            FirstActive();
//        }
//        OnMouse();
//        if(Input.GetKeyDown(KeyCode.J))
//        {
//            if (rmove == null)
//                rmove = StartCoroutine(MoveR());
//        }
//    }

//    public void MoveAreaCreate()
//    {
//        for(int i=-movePoint;i<=movePoint;i++)
//            for(int j=-movePoint;j<=movePoint;j++)
//            {
//                if (i == 0 & j == 0) continue;
//                int x = i < 0 ? -i : i;
//                int z = j < 0 ? -j : j;
//                if (x + z > movePoint) continue;
//                MoveableCheck(new Vector2Int((int)player.position.x + i, (int)player.position.z + j));
//            }
//    }
//    public void MoveableCheck(Vector2Int target)
//    {
//        for (int i = 0; i < field.size.y; i++)
//        {
//            Vector3Int checkCoord = new Vector3Int(target.x, i, target.y);
//            if (field.Cube(checkCoord).type == CUBE_TYPE.Air | field.Cube(checkCoord).type == CUBE_TYPE.Obstacle)
//            {
//                AreaView temp = AreaViewManager.Instance.CallAreaView(checkCoord + new Vector3(0, 0.1f, 0), transform);
//                areaViewDic.Add(target,temp);
//                if (field.Cube(checkCoord).type == CUBE_TYPE.Obstacle)
//                    temp.SetColor(TILE_TYPE.Disable);
//                if(field.Cube(checkCoord-Vector3Int.up).type == CUBE_TYPE.Water)
//                    temp.SetColor(TILE_TYPE.Passable);
//                break;
//            }               
//        }
//        if (!areaViewDic.ContainsKey(target))
//        {
//            AreaView temp = AreaViewManager.Instance.CallAreaView(new Vector3Int(target.x, field.size.y + 1, target.y) + new Vector3(0, 0.1f, 0), transform);
//            areaViewDic.Add(target,temp);
//        }       
//    }


//    public void AreaActivate(AreaView checkArea)
//    {
//        Vector2Int target = new Vector2Int((int)checkArea.transform.position.x, (int)checkArea.transform.position.z);
//        for(int i=-1;i<=1;i++)
//            for(int j=-1;j<=1;j++)
//            {
//                if (i == 0 & j == 0) continue;
//                if (i != 0 & j != 0) continue;
//                if (areaViewDic.TryGetValue(target + new Vector2Int(i, j), out AreaView targetView))
//                    if (!areaViewList.Contains(targetView))
//                        if(targetView.curType!=TILE_TYPE.Disable)
//                        {
//                            float height = targetView.transform.position.y - checkArea.transform.position.y;
//                            if (height <= jumpHeight & height >= -jumpHeight)
//                            {
//                                if (targetView.curType == TILE_TYPE.Passable)
//                                {
//                                    if (!AreaCheck(targetView.transform.position, new Vector2Int((int)targetView.transform.position.x, (int)targetView.transform.position.z)))
//                                        continue;
//                                }
//                                targetView.SetColor(TILE_TYPE.Enable);
//                                curAreaViewList.Add(targetView);
//                            }
//                        }
//            }
//    }

//    public bool AreaCheck(Vector3 origin,Vector2Int exception)
//    {
//        Vector2Int coord = new Vector2Int((int)origin.x, (int)origin.z);
//        for (int i = -1; i <= 1; i++)
//            for (int j = -1; j <= 1; j++)
//            {
//                if (i == 0 & j == 0) continue;
//                if (i != 0 & j != 0) continue;
//                Vector2Int target = coord + new Vector2Int(i, j);
//                if (target == exception) continue;
//                if (areaViewDic.TryGetValue(target, out AreaView targetView))
//                    if (!areaViewList.Contains(targetView))
//                        if (targetView.curType == TILE_TYPE.Enable)
//                        {
//                            float height = targetView.transform.position.y - origin.y;
//                            if (height <= jumpHeight & height >= -jumpHeight)
//                                return true;
//                        }
//            }
//        return false;
//    }


//    public void AreaDeActivate()
//    {
//        foreach (AreaView view in curAreaViewList)
//            view.SetColor(TILE_TYPE.Default);
//        curAreaViewList.Clear();
//    }


//    public void OnMouse()
//    {
//        if(Input.GetMouseButtonDown(0))
//            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, mask))
//                if (hit.transform.TryGetComponent(out AreaView view))
//                    if (view.curType == TILE_TYPE.Enable)
//                    {
//                        AreaDeActivate();
//                        view.SetColor(TILE_TYPE.Selected);
//                        areaViewList.Add(view);
//                        AreaActivate(view);
//                    }

//    }

//    public void FirstActive()
//    {
//        Vector2Int target = new Vector2Int((int)player.position.x, (int)player.position.z);
//        for (int i = -1; i <= 1; i++)
//            for (int j = -1; j <= 1; j++)
//            {
//                if (i == 0 & j == 0) continue;
//                if (i != 0 & j != 0) continue;
//                if (areaViewDic.TryGetValue(target + new Vector2Int(i, j), out AreaView targetView))
//                    if (!areaViewList.Contains(targetView))
//                        if (targetView.curType != TILE_TYPE.Disable)
//                        {
//                            float height = targetView.transform.position.y - player.transform.position.y;
//                            if (height <= jumpHeight & height >= -jumpHeight)
//                            {
//                                if (targetView.curType == TILE_TYPE.Passable)
//                                {
//                                    if (!AreaCheck(targetView.transform.position, new Vector2Int((int)targetView.transform.position.x, (int)targetView.transform.position.z)))
//                                        continue;
//                                }
//                                targetView.SetColor(TILE_TYPE.Enable);
//                                curAreaViewList.Add(targetView);
//                            }
//                        }
//            }
//    }



//    public IEnumerator MoveR()
//    {
//        AreaDeActivate();
//        Vector3 prevPos = player.position;
//        while(areaViewList.Count > 0)
//        {
//            AreaView target = areaViewList[0];
//            areaViewList.RemoveAt(0);       
//            bool isComplete = false;
//            Vector3 targetPos = target.transform.position - new Vector3(0, 0.1f, 0);
//            float height = prevPos.y - targetPos.y;
//            if(height>0)
//            {

//            }
//            if(height< 0)
//            {

//            }
//            player.transform.DOMove(targetPos, 1).OnComplete(() => isComplete = true);
//            yield return new WaitUntil(() => isComplete);
//        }
//    }

//}
