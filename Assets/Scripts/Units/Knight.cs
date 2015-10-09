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
        if (!isAtBottom) projectileObj.transform.position = buddyTwoInFront.transform.position;
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        projectile.Init(isAtBottom, boardX, currentAttackPower);

        foreach (Knight unit in attackBuddies)
        {
            unit.RemoveImmediately();
        }
        ResetAttackBuddies();
        RemoveImmediately();
    }
}
