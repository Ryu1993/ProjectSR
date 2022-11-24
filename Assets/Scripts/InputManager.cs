using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class InputManager : Singleton<InputManager>
{
    private Stack<Action> cancleBehaviour = new Stack<Action>();
    public Stack<Action> CancleBehaviour { get { return cancleBehaviour; } }

    public Action leftClickEvent;


    public void Update()
    {
        if (Input.GetMouseButtonDown(1))
            if(cancleBehaviour.Count > 0)
                cancleBehaviour.Pop().Invoke();
        if (Input.GetMouseButtonDown(0))
            leftClickEvent?.Invoke();
    }

    public void InputReset(bool isPlayer = true)
    {
        cancleBehaviour.Clear();
        CameraManager.Instance.CharacterFocusOut();
        CharacterUIManager.Instance.UIinteractionSwitch(isPlayer);
        ActionSelectUI.Instance.SelectBoxCancle();
        TurnManager.Instance.ButtonSwitch(true);
    }




}
