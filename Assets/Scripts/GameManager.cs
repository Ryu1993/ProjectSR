using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;

public class GameManager : Singleton<GameManager>
{
    [HideInInspector] public List<Character> party;
    [HideInInspector] public List<Character> enemy;
    [HideInInspector] public bool isReady;
    public bool isConfirmIgnore;
    public CharacterInfo[] playerCharacter;
    public CharacterInfo[] enemies;
    private int progressCount; // 프로퍼티로 로딩 화면 구성
    private FieldGenerator field;
    public CharacterUI charUI;
    public Transform charUICanvas;

    public IEnumerator Start()
    {
        field = FieldGenerator.Instance;
        party.Clear();
        enemy.Clear();
        int maxProgressCount = playerCharacter.Length + enemies.Length;
        progressCount = 0;
        CharacterInstantiate(playerCharacter,party);
        CharacterInstantiate(enemies, enemy);
        yield return new WaitUntil(() => progressCount == maxProgressCount);
        field.PointSet(out Vector2Int partySpot, out Vector2Int enemySpot);
        Span<Vector3Int> spotAround = stackalloc Vector3Int[9];
        CharacterPositionSet(partySpot, ref spotAround, party);
        CharacterPositionSet(enemySpot,ref spotAround, enemy);
        for(int i= 0; i<party.Count;i++)
        {

            CharacterUI ui = Instantiate(charUI, charUICanvas);
            ui.CharacterMatch(party[i], playerCharacter[i].iconSprite);
            CharacterUIManager.Instance.UIAdd(ui);
        }
        PlayerMotionManager.Instance.SetAttackMotion();
        isReady = true;
    }

    private void CharacterInstantiate(CharacterInfo[] infos,List<Character> targetList)
    {
        foreach (CharacterInfo info in infos)
        {
            Addressables.InstantiateAsync(info.characterPrefab).Completed += (handle) =>
            {
                Transform charTransform = handle.Result.transform;
                Character character = charTransform.GetComponent<Character>();
                character.CharacterSetting(info);
                targetList.Add(character);
                progressCount++;
            };
        }
    }

    private void CharacterPositionSet(Vector2Int origin,ref Span<Vector3Int> spotAround,List<Character> list)
    {
        int count = 0;
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                field.Surface(origin + new Vector2Int(i, j), out Vector3Int target);
                spotAround[count] = target;
                count++;
            }
        Shuffle.ShuffleSpan(ref spotAround);
        for (int i = 0; i < list.Count; i++)
        {
            Vector3Int charPos = spotAround[i];
            list[i].transform.position = charPos;
            field.CubeDataCall(charPos).onChracter = list[i];
            field.Cube(charPos).type = CUBE_TYPE.OnCharacter;
        }
    }

    public IEnumerator EnemyTurn()
    {
        foreach(var e in enemy)
        {
            Monster mon = e as Monster;
            yield return StartCoroutine(mon.MonsterAction());
        }
    }

    bool turnSwith = false;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)&!turnSwith)
            StartCoroutine(EnemyTurn());
        if(Input.GetKeyDown(KeyCode.W)&turnSwith)
        {        
            foreach (var c in party)
                c.TurnReset();
            CharacterUIManager.Instance.UIinteractionSwitch(true);
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
            turnSwith = !turnSwith;
        }

    }

}
