using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileShooter : Projectile {
    public override void Init(bool belongsToBottom, int col, string unitTypeString, float totalAttackPower)
    {
        base.Init(belongsToBottom, col, unitTypeString, totalAttackPower);

        if (!belongsToBottom)
        {
            transform.Rotate(Vector3.forward * 180f);
        }
    }
}
