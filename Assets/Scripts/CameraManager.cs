using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager> , IInputEventable
{
    [SerializeField]private CinemachineVirtualCamera characterVC;
    [SerializeField]private CinemachineVirtualCamera staticCharacterVC;
    [SerializeField]private CinemachineVirtualCamera fieldVC;
    [SerializeField]private CharacterUI characterUI;
    private Camera mainCamera;
    private RaycastHit hit;
    private Character selectCharacter;
    private Coroutine selectInput;
    private FieldGenerator field { get { return FieldGenerator.Instance; } }

    private List<AreaView> selectableView = new List<AreaView>();
    private bool inputSwitch;


    private void Awake()
    {
        Vector3 target = field.size;
        fieldVC.transform.position = new Vector3(target.x / 2, 20, target.z / 2);
        mainCamera = Camera.main;
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameManager.Instance.isReady);
        InputStart();
    }


    public void InputBreak()
    {
        StopCoroutine(selectInput);
    }
    public void InputStart()
    {
        SelectTileSetting();
        selectInput = StartCoroutine(CharacterSelect());
    }


    private void SelectTileSetting()
    {
        characterUI.gameObject.SetActive(false);
        fieldVC.gameObject.SetActive(true);
        foreach (Character player in GameManager.Instance.party)
            selectableView.Add(AreaViewManager.Instance.CallAreaView(player.transform.position + new Vector3(0, 0.1f, 0), null));
        foreach (AreaView selecteable in selectableView)
            selecteable.SetState(TILE_TYPE.Enable);
        inputSwitch = false;
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

    private void CharacterFocus()
    {
        inputSwitch = true;
        characterVC.Follow = selectCharacter.transform;
        characterVC.LookAt = selectCharacter.transform;
        fieldVC.gameObject.SetActive(false);
        characterUI.transform.position = selectCharacter.transform.position;
        characterUI.transform.rotation = characterVC.transform.rotation;
        characterUI.curSelectCharacter = selectCharacter.transform;
        characterUI.curCharacter = selectCharacter;
        characterUI.baseClickAction = InputBreak;
        characterUI.gameObject.SetActive(true);
    }


    private IEnumerator CharacterSelect()
    {
        while(true)
        {
            if (Input.GetMouseButtonDown(0)&!inputSwitch)
            {
                if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
                    if(hit.transform.TryGetComponent(out AreaView selectview))
                    {
                        selectCharacter = field.Cube(selectview.transform.position.ToInt()).data.onChracter;
                        foreach (var view in selectableView)
                            view.Return();
                        CharacterFocus();
                    }
            }
            yield return null;
        }
    }



}
