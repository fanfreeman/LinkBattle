using UnityEngine;
using System.Collections;
using System;

public class Archer : Unit {

    public GameObject projectile;

    public override string GetTypeString()
    {
        return "Archer";
    }

    public override IEnumerator Attack()
    {
        StartCoroutine(base.Attack());

        yield return new WaitForSeconds(1f);

        // 发射projectile
        GameObject arrowObj = Instantiate(projectile, transform.position, Quaternion.identity) as GameObject;
        Projectile arrow = arrowObj.GetComponent<Projectile>();
        arrow.Init(boardX);
        
        yield return new WaitForSeconds(0.5f);
        
        foreach (Archer unit in attackBuddies)
        {
            StartCoroutine(unit.Remove());
        }
        ResetAttackBuddies();
        StartCoroutine(Remove());
    }
}
