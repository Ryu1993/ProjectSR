using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField]private CinemachineVirtualCamera characterVC;
    [SerializeField]private CinemachineVirtualCamera staticCharacterVC;
    [SerializeField]private CinemachineVirtualCamera fieldVC;
    private Camera mainCam;

    private FieldGenerator field { get { return FieldGenerator.Instance; } }



    private void Start()
    {
        Vector3 target = field.size;

        mainCam = Camera.main;
    }

    public void VCMove(bool on)
    {
        if (on)
        {
            staticCharacterVC.transform.position = characterVC.transform.position - Vector3.up * 5 - Vector3.back * 2;
            staticCharacterVC.LookAt = characterVC.LookAt;
            staticCharacterVC.Follow = characterVC.LookAt;
            staticCharacterVC.gameObject.SetActive(true);        
        }
        else
            staticCharacterVC.gameObject.SetActive(false);
    }

    public void CharacterFocus(Character selectCharacter)
    {
        characterVC.Follow = selectCharacter.transform;
        characterVC.LookAt = selectCharacter.transform;
        fieldVC.gameObject.SetActive(false);
    }
    public void CharacterFocusOut()
    {
        fieldVC.gameObject.SetActive(true);
    }




}
