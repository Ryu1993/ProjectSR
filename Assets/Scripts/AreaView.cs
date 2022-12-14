using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Transactions;
using UnityEngine;
using UnityEngine.Events;
using Color = UnityEngine.Color;

public enum TILE_TYPE { Default =0, Enable = 1, Disable = 2,Selected = 3,Passable = 4, Active = 5,Invisible =6}



public class AreaView : MonoBehaviour,IPoolingable
{
    [SerializeField] private Color[] colors;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshCollider meshCollider;
    public TILE_TYPE curType;
    public TILE_TYPE curState;

    public ObjectPool home { get; set; }

    private void OnEnable()
    {
        SetColor(TILE_TYPE.Default, ref curType);
        curState = TILE_TYPE.Default;
        meshRenderer.enabled = true;
        gameObject.layer = 3;
    }

    public void Invisible()
    {
        meshRenderer.enabled = false;
        curState = TILE_TYPE.Disable;
    }

    private void SetColor(TILE_TYPE type,ref TILE_TYPE cur)
    {
        cur = type;
        Color[] colors = new Color[meshFilter.mesh.vertexCount];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = this.colors[(int)type];
        meshFilter.mesh.SetColors(colors);
        meshCollider.enabled = type == TILE_TYPE.Enable;
    }


    public void SetType(TILE_TYPE type) => SetColor(type,ref curType);
    public void SetState(TILE_TYPE type) => SetColor(type, ref curState);
    public void Return()=> home.Return(gameObject);






}
