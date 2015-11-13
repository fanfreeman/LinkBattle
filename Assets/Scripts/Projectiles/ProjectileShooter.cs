using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileShooter : Projectile {
    public override void Init(bool belongsToBottom, int col, float totalAttackPower)
    {
        base.Init(belongsToBottom, col, totalAttackPower);

        if (!belongsToBottom)
        {
            transform.Rotate(Vector3.forward * 180f);
        }
    }
}
