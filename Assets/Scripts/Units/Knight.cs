using UnityEngine;
using System.Collections;

public class Knight : Unit
{
    public GameObject projectilePrefab;

    public override string GetTypeString()
    {
        return "Knight";
    }

    protected override IEnumerator Attack()
    {
        // 播放攻击动画以及腾出棋盘上的位置
        StartCoroutine(base.Attack());

        yield return new WaitForSeconds(1f);

        // 进攻
        LaunchSelf();
    }

    // 近攻单位自行出战
    void LaunchSelf()
    {
        GameObject projectileObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity) as GameObject;
        Projectile projectile = projectileObj.GetComponent<Projectile>();

        //给三人组传过去日后再用
        projectile.Init(isAtBottom, boardX, currentAttackPower, attackBuddies, this);

        // 立刻清理此单位以及其队友
//        foreach (Knight unit in attackBuddies)
//        {
//            unit.RemoveImmediately();
//        }
//        ResetAttackBuddies();
//        RemoveImmediately();
    }
}
