using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUIManager : Singleton<CharacterUIManager>
{
    private List<CharacterUI> charUIs = new List<CharacterUI>();


    public void UIAdd(CharacterUI target)
    {
        charUIs.Add(target);
    }
    public void UIinteractionSwitch(bool onoff)
    {
        foreach (CharacterUI target in charUIs)
            target.button.interactable = onoff;
    }
}
