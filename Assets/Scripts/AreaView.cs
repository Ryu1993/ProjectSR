using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TILE_TYPE { Default =0, Enable = 1, Disable = 2}


[RequireComponent(typeof(MeshRenderer))]
public class AreaView : MonoBehaviour,IPoolingable
{
    /// <summary>
    /// 0=default,1=able,2=disable
    /// </summary>
    [SerializeField] private Color[] colors;
    //private Material _material;
    //private Material material
    //{
    //    get
    //    {
    //        if(_material == null)
    //            _material = GetComponent<MeshRenderer>().material;
    //        return _material;
    //    }
    //}
    private MeshFilter meshFilter;

    public ObjectPool home { get; set; }

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        SetColor(TILE_TYPE.Default);
    }


    public void SetColor(TILE_TYPE type)
    {
        Color[] colors = new Color[meshFilter.mesh.vertexCount];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = this.colors[(int)type];
        meshFilter.mesh.SetColors(colors);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SetColor(TILE_TYPE.Enable);
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            SetColor(TILE_TYPE.Disable);
        }
    }





}
