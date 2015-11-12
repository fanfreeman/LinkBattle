using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    
    public float moveSpeed = 5f;

    [HideInInspector]
    public bool belongsToBottomPlayer { get; set; }

    [HideInInspector]
    public int column;

    // 以下两种伤害模式只能二选一
    [HideInInspector]
    public float remainingDamage = 0f; // 抛射物的总伤害值
    float perUnitDamage = 0f; // 抛射物对此列每一单位的伤害值

    Rigidbody2D rb2D;
    Vector3 moveTarget;

    protected static int outOfTopEdgeY = 20;
    protected static int outOfBottomEdgeY = -20;

    protected virtual void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        enabled = false;
    }

    public virtual void Init(bool belongsToBottom, int col, float totalAttackPower)
    {
        remainingDamage = totalAttackPower;
        if (remainingDamage > 0 && perUnitDamage > 0) throw new System.Exception("Total Damage and Per Unit Damage cannot both be set");

        belongsToBottomPlayer = belongsToBottom;

        column = col;
        moveTarget = BoardManager.instance.TopHalf_GetCoordinatesAtPosition(col, 0);
        if (belongsToBottomPlayer) moveTarget.y = outOfTopEdgeY;
        else moveTarget.y = outOfBottomEdgeY;
        enabled = true;
    }

    void Update()
    {
        float sqrRemainingDistance = (transform.position - moveTarget).sqrMagnitude;
        Vector3 newPosition = Vector3.MoveTowards(rb2D.position, moveTarget, moveSpeed * Time.deltaTime);
        rb2D.MovePosition(newPosition);

        if (sqrRemainingDistance < 1f)
        {
            Debug.Log("consolidate: projectile out of screen");
            DestroySelf(); // 移出屏幕后销毁此projectile
        }
    }

    protected void DestroySelf()
    {
        // projectile耗尽时，consolidate其所在的敌方column
        if (belongsToBottomPlayer) BoardManager.instance.TopHalf_DoConsolidateColumn(column);
        else BoardManager.instance.BottomHalf_DoConsolidateColumn(column);

        Destroy(gameObject);
    }
    
    // projectile碰撞检测
    void OnTriggerEnter2D(Collider2D other)
    {
        Unit targetUnit = other.GetComponent<Unit>();
        //检测是否撞线
        LineController hittedLine = other.GetComponent<LineController>();
        if(targetUnit == null && hittedLine != null){
            int damage = Mathf.CeilToInt(remainingDamage);
            if (other.tag == "EnemyLine") {
                BattleLoader.instance.ChangeTopHp(-damage);
            } else if (other.tag == "PlayerLine") {
                BattleLoader.instance.ChangeBottomHp(-damage);
            }
            hittedLine.PlayHitParticle(transform.position.x);

            CameraEffects.instance.CreateDamagePopup(damage, transform.position);
            CameraEffects.instance.Shake();
            DestroySelf();
            return;
        }

        // 检测是否撞到单位
        if ((belongsToBottomPlayer && !targetUnit.isAtBottom) || (!belongsToBottomPlayer && targetUnit.isAtBottom))
        {
            //Debug.Log("shot " + other.gameObject.name);
            remainingDamage -= (targetUnit.TakeDamage(remainingDamage));
            if (remainingDamage <= 0) // 此projectile耗尽后销毁此projectile
            {
                Debug.Log("consolidate: projectile used up");

                DestroySelf();
            }
        }
    }
}
