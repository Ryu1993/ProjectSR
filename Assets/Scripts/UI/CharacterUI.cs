using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CharacterUI : MonoBehaviour
{
    public UnityAction baseClickAction;
    public Transform curSelectCharacter;
    public PlayerMove move;
    public CharacterAttack attack;
   

    public void AttackClick()
    {
        baseClickAction?.Invoke();

    }

    public void MoveClick()
    {
        baseClickAction?.Invoke();
        move.order = curSelectCharacter;
        move.InputStart();
        gameObject.SetActive(false);
    }

    public void RestClick()
    {
        baseClickAction?.Invoke();

    }

    


   
}
