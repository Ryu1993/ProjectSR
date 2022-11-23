using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CharacterUI : MonoBehaviour
{
    public UnityAction baseClickAction;
    public Transform curSelectCharacter;
    public CharacterMove move;
    public CharacterAttack attack;
    public Character curCharacter;
   

    public void AttackClick()
    {
        baseClickAction?.Invoke();

    }

    public void MoveClick()
    {
        baseClickAction?.Invoke();
        move.Move(curCharacter);
        InputManager.Instance.CancleBehaviour.Push(() => { move.ReMove();gameObject.SetActive(true); });
        gameObject.SetActive(false);
    }

    public void RestClick()
    {
        baseClickAction?.Invoke();

    }

    public void Set(Character target)
    {
        curCharacter = target;
        transform.position = curCharacter.transform.position;


    }
    


   
}
