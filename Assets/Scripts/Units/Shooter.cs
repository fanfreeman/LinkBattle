using UnityEngine;
using System.Collections;
using System;

public class Shooter : Unit {

    public GameObject projectilePrefab;

    protected override void Awake() {
        base.Awake();
    }

    protected override IEnumerator Attack()
    {
        // 播放攻击动画以及腾出棋盘上的位置
        StartCoroutine(base.Attack());

        yield return new WaitForSeconds(1f);

        // 发射projectile
        LaunchProjectile();
        
        yield return new WaitForSeconds(0.5f);
        
        // 清理此单位以及其队友
        foreach (Shooter unit in attackBuddies)
        {
            StartCoroutine(unit.RemoveWithAnimation());
        }
        //ResetAttackBuddies();
        StartCoroutine(RemoveWithAnimation());
    }

    //施法动画
    IEnumerator Cast()
    {
        foreach (Unit unit in attackBuddies)
        {
            unit.animController.Cast1();
        }
        animController.Cast1();
        yield return new WaitForSeconds(0.3f);;
    }

    // 远攻单位释放箭矢
    void LaunchProjectile()
    {
        GameObject projectileObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity) as GameObject;
        //if (!isAtBottom) projectileObj.transform.position = buddyTwoInFront.transform.position; // 如属于上半场，释放地点往尾部挪两个格位，因为attack leader在队首
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        projectile.Init(isAtBottom, boardX, currentAttackPower);
    }
}
