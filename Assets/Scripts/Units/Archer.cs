using UnityEngine;
using System.Collections;
using System;

public class Archer : Unit {

    public GameObject projectile;

    public override string GetTypeString()
    {
        return "Archer";
    }

    protected override IEnumerator Attack()
    {
        StartCoroutine(base.Attack());

        yield return new WaitForSeconds(1f);

        // 发射projectile
        GameObject arrowObj = Instantiate(projectile, transform.position, Quaternion.identity) as GameObject;
        if (!isAtBottom) arrowObj.transform.position = buddyTwoInFront.transform.position;
        Projectile arrow = arrowObj.GetComponent<Projectile>();
        arrow.Init(isAtBottom, boardX, currentAttackPower);
        
        yield return new WaitForSeconds(0.5f);
        
        foreach (Archer unit in attackBuddies)
        {
            StartCoroutine(unit.Remove());
        }
        ResetAttackBuddies();
        StartCoroutine(Remove());
    }
}
