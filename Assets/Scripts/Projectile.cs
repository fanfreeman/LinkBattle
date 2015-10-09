using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {
    
    public float moveSpeed = 5f;

    [HideInInspector]
    public bool belongsToBottomPlayer { get; set; }

    [HideInInspector]
    public int column;

    // 以下两种伤害模式只能二选一
    float totalDamage = 0f; // 抛射物的总伤害值
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

    public void Init(bool belongsToBottom, int col, float totalAttackPower)
    {
        totalDamage = totalAttackPower;
        if (totalDamage > 0 && perUnitDamage > 0) throw new System.Exception("Total Damage and Per Unit Damage cannot both be set");

        belongsToBottomPlayer = belongsToBottom;

        if(!belongsToBottom)
        {
            transform.Rotate(Vector3.forward * 180f);
        }

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

    void DestroySelf()
    {
        // projectile耗尽时，consolidate其所在的敌方column
        if (belongsToBottomPlayer) BoardManager.instance.TopHalf_DoConsolidateColumn(column);
        else BoardManager.instance.BottomHalf_DoConsolidateColumn(column);

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Unit targetUnit = other.GetComponent<Unit>();
        if ((belongsToBottomPlayer && !targetUnit.isAtBottom) || (!belongsToBottomPlayer && targetUnit.isAtBottom))
        {
            //Debug.Log("shot " + other.gameObject.name);
            this.totalDamage -= (targetUnit.TakeDamage(totalDamage));
            if (this.totalDamage <= 0) // 此projectile耗尽后销毁此projectile
            {
                Debug.Log("consolidate: projectile used up");
                DestroySelf();
            }
        }
    }
}
