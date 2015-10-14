using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileArcher : Projectile {
    public override void Init(bool belongsToBottom, int col, float totalAttackPower, List<Unit> attackBuddies, Unit leader)
    {
        base.Init(belongsToBottom, col, totalAttackPower, attackBuddies, leader);

        if (!belongsToBottom)
        {
            transform.Rotate(Vector3.forward * 180f);
        }
    }

//    void OnTriggerEnter2D(Collider2D other){
//        Unit targetUnit = other.GetComponent<Unit>();
//
//        // 检测是否撞线
//        if (targetUnit == null) {
//            int damage = Mathf.CeilToInt(remainingDamage);
//            if (other.tag == "EnemyLine") {
//                BattleLoader.instance.ChangeEnemyHp(-damage);
//            } else if (other.tag == "PlayerLine") {
//                BattleLoader.instance.ChangePlayerHp(-damage);
//            }
//
//            CameraEffects.instance.CreateDamagePopup(damage, transform.position);
//            CameraEffects.instance.Shake();
//            DestroySelf();
//        }
//    }

}
