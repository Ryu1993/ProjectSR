using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG;

public class Loading : Singleton<Loading>
{

    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private Image loadingBar;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        //gameObject.SetActive(false);
    }

    public void CallLoadingUI()
    {
        gameObject.SetActive(true);
        loadingBar.fillAmount = 0;
    }

    public void ProgressLoadingUI(float progress)
    {
        loadingBar.fillAmount = progress;
    }

    public void ReturnLoadingUI()
    {
        gameObject.SetActive(false);
    }




}
