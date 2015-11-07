using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ShaderSetUp : MonoBehaviour {

    public Shader shader; //mousehover effect
    public bool isMouseOverEffectEnabled = true;

    private SpriteRenderer[] sRens;
    private List<Shader> initShader = new List<Shader>();

	void Start () {
        sRens = gameObject.GetComponentsInChildren<SpriteRenderer>();
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
