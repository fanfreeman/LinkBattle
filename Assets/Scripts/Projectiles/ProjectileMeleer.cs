using UnityEngine;
using System.Collections;

public class ProjectileMeleer : Projectile {

    UnitAnimationController[] animControllers;

    /// <summary>
    ///  create projecttile by  custom meleer   not only knight
    /// </summary>
    /// <param name="meleer">the custom gameObject</param>
    public void SetMeleerObject(GameObject meleer)
    {
        // these objects will become my children
        GameObject meleerObject1 = Instantiate(meleer) as GameObject;
        GameObject meleerObject2 = Instantiate(meleer) as GameObject;
        GameObject meleerObject3 = Instantiate(meleer) as GameObject;

        SpriteRenderer[] meleerObject1Sprites = meleerObject1.GetComponentsInChildren<SpriteRenderer>();
        SpriteRenderer[] meleerObject2Sprites = meleerObject2.GetComponentsInChildren<SpriteRenderer>();
        SpriteRenderer[] meleerObject3Sprites = meleerObject3.GetComponentsInChildren<SpriteRenderer>();

        SetProjectileSortingLayer(meleerObject1Sprites);
        SetProjectileSortingLayer(meleerObject2Sprites);
        SetProjectileSortingLayer(meleerObject3Sprites);

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
    private void SetProjectileSortingLayer(SpriteRenderer[] sprites)
    {
        foreach (SpriteRenderer sprite in sprites) {
            sprite.sortingLayerName = "projectiles";
        }
    }
}
