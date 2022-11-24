using Sirenix.OdinInspector.Editor.GettingStarted;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ActionSelectUI : Singleton<ActionSelectUI>
{
    [SerializeField]
    private GameObject buttonGroup;
    public Character curCharacter;
    /// <summary>
    /// 0:Attack 1: Move 2: Rest
    /// </summary>
    public Button[] buttons;
    public Button[] attackButtons;
    public Action moveAction;

    private void SelectBoxActive() => SelectBoxActive(curCharacter);
    public void SelectBoxActive(Character target)
    {
        buttonGroup.gameObject.SetActive(true);
        curCharacter = target;
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].interactable = curCharacter.actionable[i];
        if (buttons[0].interactable)
            for (int i = 0; i < attackButtons.Length; i++)
                attackButtons[i].interactable = true;
        moveAction = DefaultMoveAction;

    }
    public void SelectBoxCancle()
    {
        curCharacter = null;
        foreach (Button button in buttons)
            button.interactable = true;
        buttonGroup.gameObject.SetActive(false);
    }

    public void ButtonOff()
    {
        foreach (var button in buttons)
            button.interactable = false;
        foreach (var button in attackButtons)
            button.interactable = false;
    }


    public void MoveAction()
    {
        moveAction?.Invoke();
    }

    public void DefaultMoveAction()
    {
        ButtonOff();
        CharacterMove.Instance.Move(curCharacter);
        InputManager.Instance.CancleBehaviour.Push(() => { CharacterMove.Instance.ReMove(); SelectBoxActive(); });
    }

    public void AttackAction()
    {
        if (curCharacter.attackList.Count == 0) return;
        ButtonOff();
        moveAction = null;
        CharacterAttack.Instance.AttackAreaCreate(curCharacter, curCharacter.attackList[0]);
        InputManager.Instance.CancleBehaviour.Push(() => { CharacterAttack.Instance.AttackAreaRemove();SelectBoxActive();});
    }


    public void RestAction()
    {


    }
    public void SkiiAction(int index)
    {
        if (curCharacter.attackList.Count < index-1) return;
        ButtonOff();
        CharacterAttack.Instance.AttackAreaCreate(curCharacter, curCharacter.attackList[index]);
        InputManager.Instance.CancleBehaviour.Push(() => { CharacterAttack.Instance.AttackAreaRemove(); SelectBoxActive(); });
    }

}
