using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ShaderSetUp : MonoBehaviour {

    public Shader shader; //mousehover effect
    public bool isMouseOverEffectEnabled = true;

    private SpriteRenderer[] sRens;
    private List<Shader> initShader = new List<Shader>();

	void Awake () {
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

    //设置alpha
    public void SetUnitArtAlpha(float a)
    {
        foreach(SpriteRenderer sRen in sRens) {
            Color c = sRen.color;
            c.a = a;
            sRen.color = c;
        }
    }

    public static void SetUnitArtAlpha(GameObject obj,float a)
    {
        SpriteRenderer[] sRensObj;
        sRensObj = obj.GetComponentsInChildren<SpriteRenderer>();

        foreach(SpriteRenderer sRen in sRensObj) {
            Color c = sRen.color;
            c.a = a;
            sRen.color = c;
        }
    }
}
