using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Character> party;
    public List<Character> enemys;

    public void Start()
    {
        party[0].transform.position = FieldGenerator.Instance.groundList[Random.Range(0, FieldGenerator.Instance.groundList.Count)] + Vector3Int.up;

    }

}
