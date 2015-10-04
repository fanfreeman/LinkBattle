using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

    // 以下两种伤害模式只能二选一
    public float totalDamage = 250f; // 抛射物的总伤害值
    public float perUnitDamage = 0f; // 抛射物对此列每一单位的伤害值

    public float moveSpeed = 5f;
    
    [HideInInspector]
    public bool belongsToBottomPlayer { get; set; }

    [HideInInspector]
    public int column;

    Rigidbody2D rb2D;
    Vector3 moveTarget;

    protected static int outOfTopEdgeY = 20;
    protected static int outOfBottomEdgeY = -20;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        belongsToBottomPlayer = true;
        enabled = false;
    }

    public void Init(int col)
    {
        if (totalDamage > 0 && perUnitDamage > 0) throw new System.Exception("Total Damage and Per Unit Damage cannot both be set");

        column = col;
        moveTarget = BoardManager.instance.TopHalf_GetCoordinatesAtPosition(col, 0);
        moveTarget.y = outOfTopEdgeY;
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
        BoardManager.instance.TopHalf_DoConsolidateColumn(column); // projectile耗尽时，consolidate其所在的敌方column
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Unit targetUnit = other.gameObject.GetComponent<Unit>();
        if (belongsToBottomPlayer)
        {
            if (!targetUnit.isAtBottom)
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
}
