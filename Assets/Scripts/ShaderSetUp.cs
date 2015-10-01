using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ShaderSetUp : MonoBehaviour {

    public Shader shader;
    public GameObject body;
    public bool isMouseOverEffectEnabled = true;

    private SpriteRenderer[] sRens;
    private List<Shader> initShader = new List<Shader>();

	void Start () {
        sRens = body.GetComponentsInChildren<SpriteRenderer>();
        getInitMaterial();
    }

    void  OnMouseEnter()
    {
        if (isMouseOverEffectEnabled) SetUpMaterial();
    }

    void  OnMouseExit()
    {
        if (isMouseOverEffectEnabled) SetToInitMaterial();
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
