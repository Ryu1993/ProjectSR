using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    private List<Character> enemies;
    private List<Character> players;
    [SerializeField] private Button turnBotton;
    [SerializeField] private GameObject monsterTurnPannel;
    public static TurnManager Instance { get; private set; }

    public void Setting(List<Character> enemys, List<Character> players)
    {
        this.enemies = enemys;
        this.players = players;
        Instance = this;
    }

    public void ButtonSwitch(bool isOnOff)
    {
        turnBotton.gameObject.SetActive(isOnOff);
    }

    public void NextTurnProgress()
    {
        turnBotton.interactable = false;
        StartCoroutine(TurnProgress());
    }

    private IEnumerator TurnProgress()
    {
        CharacterUIManager.Instance.UIinteractionSwitch(false);
        Time.timeScale = 2f;
        monsterTurnPannel.SetActive(true);
        foreach (var enemy in enemies)
        {
            Monster monster = enemy as Monster;
            yield return monster?.StartCoroutine(monster.MonsterAction());
        }
        foreach (var character in players)
            character.TurnReset();
        monsterTurnPannel.SetActive(false);
        Time.timeScale = 1f;
        CharacterUIManager.Instance.UIinteractionSwitch(true);
        turnBotton.interactable = true;
    }

}
