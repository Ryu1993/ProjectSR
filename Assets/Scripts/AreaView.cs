using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using Color = UnityEngine.Color;

public enum TILE_TYPE { Default =0, Enable = 1, Disable = 2,Selected = 3,Passable = 4,Invisible =5}


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
    private MeshRenderer meshRenderer;
    public Transform player;
    public PlayerMove playerMove;
    public TILE_TYPE curType;
    public TILE_TYPE curState;

    public ObjectPool home { get; set; }

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

    }
    private void OnEnable()
    {
        SetColor(TILE_TYPE.Default, ref curType);
        meshRenderer.enabled = true;
    }

    public void Invisible()
    {
        meshRenderer.enabled = false;
        transform.position = transform.position + new Vector3(0, 0.1f, 0);
    }

    public void SetColor(TILE_TYPE type,ref TILE_TYPE cur)
    {
        cur = type;
        Color[] colors = new Color[meshFilter.mesh.vertexCount];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = this.colors[(int)type];
        meshFilter.mesh.SetColors(colors);
    }




    public void Return()
    {
        home.Return(this.gameObject);
    }

    public void Move()
    {
        //playerMove.transform.position = transform.position + new Vector3(0,-0.1f, 0);
        //playerMove.MoveablePoint(playerMove.transform.position.ConvertInt());
        //playerMove.CreateAreaView();

    }

    


}
