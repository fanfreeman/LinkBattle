using UnityEngine;
using System.Collections;

public class ProjectileKnight : Projectile {

    UnitAnimationController[] animControllers;

	override protected void Awake()
    {
        base.Awake();

        animControllers = GetComponentsInChildren<UnitAnimationController>();
        foreach (UnitAnimationController animController in animControllers)
        {
            animController.Walk();
        }
    }
}
