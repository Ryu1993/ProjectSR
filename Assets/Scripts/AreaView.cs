using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TILE_TYPE { Default =0, Enable = 1, Disable = 2,Selected = 3}


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
    public Transform player;
    public TestPlayerMove playerMove;
    public TILE_TYPE curType;

    public ObjectPool home { get; set; }

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        SetColor(TILE_TYPE.Default);
    }


    public void SetColor(TILE_TYPE type)
    {
        curType = type;
        Color[] colors = new Color[meshFilter.mesh.vertexCount];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = this.colors[(int)type];
        meshFilter.mesh.SetColors(colors);
    }

    public void Return()
    {
        player = null;
        home.Return(this.gameObject);
    }

    public void Move()
    {
        //playerMove.transform.position = transform.position + new Vector3(0,-0.1f, 0);
        //playerMove.MoveablePoint(playerMove.transform.position.ConvertInt());
        //playerMove.CreateAreaView();

    }



}
