using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    private Character curCharacter;
    public Image hpBar;
    public Image icon;
    public Button button;

    public void CharacterSelect()
    {
        CharacterUIManager.Instance.UIinteractionSwitch(false);
        CameraManager.Instance.CharacterFocus(curCharacter);
        ActionSelectUI.Instance.SelectBoxActive(curCharacter);
        TurnManager.Instance.ButtonSwitch(false);
        InputManager.Instance.CancleBehaviour.Push(()=> 
        {
            ActionSelectUI.Instance.SelectBoxCancle();
            CameraManager.Instance.CharacterFocusOut();
            CharacterUIManager.Instance.UIinteractionSwitch(true);
            TurnManager.Instance.ButtonSwitch(true);
        });
    }
    
    public void CharacterMatch(Character character,Sprite sprite)
    {
        curCharacter = character;
        icon.sprite = sprite;
        character.hpChangeAction += (value) => hpBar.fillAmount = value;
    }

  
}
