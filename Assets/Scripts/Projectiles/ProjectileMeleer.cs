using UnityEngine;
using System.Collections;

public class ProjectileMeleer : Projectile {

    UnitAnimationController[] animControllers;

    /// <summary>
    ///  create projecttile by  custom meleer   not only knight
    /// </summary>
    /// <param name="meleer">the custom gameObject</param>
    public void SetMeleerObject(GameObject meleer){

        GameObject meleerObject1 = Instantiate(meleer) as GameObject;
        GameObject meleerObject2 = Instantiate(meleer) as GameObject;
        GameObject meleerObject3 = Instantiate(meleer) as GameObject;

        SpriteRenderer[] meleerObject1Sprites1 = meleerObject1.GetComponentsInChildren<SpriteRenderer>();
        SpriteRenderer[] meleerObject1Sprites2 = meleerObject2.GetComponentsInChildren<SpriteRenderer>();
        SpriteRenderer[] meleerObject1Sprites3 = meleerObject3.GetComponentsInChildren<SpriteRenderer>();

        setLayerToProjectiles(meleerObject1Sprites1);
        setLayerToProjectiles(meleerObject1Sprites2);
        setLayerToProjectiles(meleerObject1Sprites3);

        meleerObject1.transform.parent = gameObject.transform;
        meleerObject2.transform.parent = gameObject.transform;
        meleerObject3.transform.parent = gameObject.transform;
        meleerObject1.transform.localPosition = new Vector3(0,0,0);
        meleerObject2.transform.localPosition = new Vector3(0,1,0);
        meleerObject3.transform.localPosition = new Vector3(0,2,0);

        animControllers = GetComponentsInChildren<UnitAnimationController>();
        foreach (UnitAnimationController animController in animControllers)
        {
            animController.Walk();
        }

    }


    //设置layer防覆盖
    private void setLayerToProjectiles(SpriteRenderer[] sprites){
        foreach(SpriteRenderer sprite in sprites){
            sprite.sortingLayerName = "projectiles";
        }
    }

}
