using UnityEngine;
using System.Collections;

public class ProjectileArcher : Projectile {
    public override void Init(bool belongsToBottom, int col, float totalAttackPower)
    {
        base.Init(belongsToBottom, col, totalAttackPower);

        if (!belongsToBottom)
        {
            transform.Rotate(Vector3.forward * 180f);
        }
    }
}
