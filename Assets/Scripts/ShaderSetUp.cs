using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ShaderSetUp : MonoBehaviour {
    public Shader shader;
    public GameObject body;
    private SpriteRenderer[] sRens;
    private List<Shader> initShader = new List<Shader>();
    // Use this for initialization
	void Start () {
        sRens = body.GetComponentsInChildren<SpriteRenderer>();
        getInitMaterial();
        //for test
        //SetUpMaterial();
    }

    void  OnMouseEnter()
    {
        //print ("OnMouseOver："+gameObject.tag);
        foreach(ShaderSetUp ssu in BoardManager.instance._shaderSetUpArchers)
            ssu.SetUpMaterial();
    }

    void  OnMouseExit()
    {
        //print ("OnMouseOver："+gameObject.tag);
        foreach(ShaderSetUp ssu in BoardManager.instance._shaderSetUpArchers)
            ssu.SetToInitMaterial();
    }

    private void getInitMaterial() {
        foreach(SpriteRenderer sRen in sRens) {
            initShader.Add(sRen.material.shader);
        }
    }

    private void SetUpMaterial() {
        foreach(SpriteRenderer sRen in sRens) {
            sRen.material.shader = shader;
        }
    }

    private void SetToInitMaterial() {
        int pointer = 0;
        foreach(SpriteRenderer sRen in sRens) {
            sRen.material.shader = initShader[pointer];
            pointer++;
        }
    }


}
