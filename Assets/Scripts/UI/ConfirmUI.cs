using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ConfirmUI : MonoBehaviour
{
    public UnityAction yesClickAction;
    public UnityAction noClickAction;


    public void YesClick()
    {
        yesClickAction?.Invoke();
        gameObject.SetActive(false);          
    }
    public void NoClick()
    {
        noClickAction?.Invoke();
        noClickAction = null;
        yesClickAction = null;
        gameObject.SetActive(false);
    }

}
