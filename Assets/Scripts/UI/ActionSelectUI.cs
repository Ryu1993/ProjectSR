using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionSelectUI : Singleton<ActionSelectUI>
{

    public Character curCharacter;
    public RectTransform selectBox;
    /// <summary>
    /// 0:Attack 1: Move 2: Rest
    /// </summary>
    public Button[] buttons;


    private void SelectBoxActive() => SelectBoxActive(selectBox.position, curCharacter);
    public void SelectBoxActive(Vector3 position ,Character target)
    {
        curCharacter = target;
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].interactable = curCharacter.actionable[i];
        selectBox.position = position;
        selectBox.gameObject.SetActive(true);
    }
    public void SelectBoxCancle()
    {
        curCharacter = null;
        foreach (Button button in buttons)
            button.interactable = true;
        selectBox.gameObject.SetActive(false);
    }


    public void MoveAction()
    {
        CharacterMove.Instance.Move(curCharacter);
        InputManager.Instance.CancleBehaviour.Push(() => { CharacterMove.Instance.ReMove();SelectBoxActive();});
        selectBox.gameObject.SetActive(false);
    }

    public void AttackAction()
    {
        if (curCharacter.attackList.Count == 0) return;
        CharacterAttack.Instance.AttackAreaCreate(curCharacter, curCharacter.attackList[0]);
        InputManager.Instance.CancleBehaviour.Push(() => { CharacterAttack.Instance.AttackAreaRemove();SelectBoxActive();});
        selectBox.gameObject.SetActive(false);
    }


    public void RestAction()
    {


    }
}
