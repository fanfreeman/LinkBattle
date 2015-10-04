using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class Unit : MonoBehaviour {

    [HideInInspector]
    public int boardX;
    [HideInInspector]
    public int boardY;
    [HideInInspector]
    public bool isAtBottom;
    [HideInInspector]
    public bool isActivated = false;

    protected static int outOfTopEdgeY = 12;
    protected static int outOfBottomEdgeY = -12;
    static Color healthBarMinColor = Color.red;
    static Color healthBarMaxColor = Color.green;

    public abstract string GetTypeString();

    ShaderSetUp shaderSetUpScript;
    RawImage healthBarImage;
    UnitAnimationController animController;

    public float healthMax = 100f;
    float healthCurrent;

    protected List<Unit> attackBuddies;
    ParticleSystem particleHit;
    ParticleSystem particleActivation;
    ParticleSystem particleCountDown;
    ParticleSystem particleDeath;

    void Awake()
    {
        shaderSetUpScript = GetComponent<ShaderSetUp>();
        healthBarImage = GetComponentInChildren<RawImage>();
        animController = GetComponent<UnitAnimationController>();
        SetHealth(healthMax);
        attackBuddies = new List<Unit>(2);

        // 初始化particles
        Transform particleObject = transform.Find("ParticleExplosion");
        particleHit = particleObject.GetComponent<ParticleSystem>();

        particleObject = transform.Find("ParticleActivation");
        particleActivation = particleObject.GetComponent<ParticleSystem>();

        particleObject = transform.Find("ParticleBurst");
        particleCountDown = particleObject.GetComponent<ParticleSystem>();

        particleObject = transform.Find("ParticleBoom");
        particleDeath = particleObject.GetComponent<ParticleSystem>();
    }

    // 设置此单位的位置
    public void SetPositionValues(int x, int y)
    {
        boardX = x;
        boardY = y;
    }

    // 设置此单位在棋盘上半部分或下半部分
    public void SetIsAtBottom(bool val)
    {
        isAtBottom = val;
    }

    // 将此单位移动至新的位置
    public void MoveToPosition(int newX, int newY)
    {
        if (isAtBottom)
        {
            BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);
            BoardManager.instance.BottomHalf_SetUnitAtPosition(this, newX, newY);
            Vector3 moveToPos = BoardManager.instance.BottomHalf_GetCoordinatesAtPosition(newX, newY);
            iTween.MoveTo(this.gameObject, moveToPos, 1f);
        }
        else
        {
            BoardManager.instance.TopHalf_SetUnitAtPosition(null, boardX, boardY);
            BoardManager.instance.TopHalf_SetUnitAtPosition(this, newX, newY);
            Vector3 moveToPos = BoardManager.instance.TopHalf_GetCoordinatesAtPosition(newX, newY);
            iTween.MoveTo(this.gameObject, moveToPos, 1f);
        }

        boardX = newX;
        boardY = newY;
    }

    // 移动column高亮光柱
    void OnMouseEnter()
    {
        if (!GameManager.instance.canMove) return;
        ColumnCollider.MoveSelectorToColumn(boardX);
    }

    // 点击此单位时将其捡起
    void OnMouseDown()
    {
        if (!GameManager.instance.canMove) return;
        if (isAtBottom)
        {
            if (GameManager.instance.playersTurn)
            {
                // 如此单位在column底部，将其移出该column（捡起）
                if (BoardManager.instance.unitBeingPickedUp == null && !isActivated && BoardManager.instance.BottomHalf_CheckIfUnitIsAtTail(this))
                {
                    Vector3 moveToPos = BoardManager.instance.BottomHalf_GetCoordinatesAtPosition(boardX, boardY);
                    moveToPos.y = outOfBottomEdgeY;
                    iTween.MoveTo(gameObject, moveToPos, 1f);
                    BoardManager.instance.unitBeingPickedUp = this;
                    BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);

                    GameManager.instance.SetColumnHighlightEnabled(true, moveToPos.x);
                }
            }
            else Debug.Log("It's the top player's turn");
        }
        else
        {
            if (!GameManager.instance.playersTurn)
            {
                // 如此单位在column底部，将其移出该column（捡起）
                if (BoardManager.instance.unitBeingPickedUp == null && !isActivated && BoardManager.instance.TopHalf_CheckIfUnitIsAtTail(this))
                {
                    Vector3 moveToPos = BoardManager.instance.TopHalf_GetCoordinatesAtPosition(boardX, boardY);
                    moveToPos.y = outOfTopEdgeY;
                    iTween.MoveTo(gameObject, moveToPos, 1f);
                    BoardManager.instance.unitBeingPickedUp = this;
                    BoardManager.instance.TopHalf_SetUnitAtPosition(null, boardX, boardY);

                    GameManager.instance.SetColumnHighlightEnabled(true, moveToPos.x);
                }
            }
            else Debug.Log("It's the bottom player's turn");
        }
    }

    // 此单位开始蓄力
    public void Activate()
    {
        isActivated = true;
        particleActivation.Play();
        shaderSetUpScript.isMouseOverEffectEnabled = false;
    }

    void Deactivate()
    {
        isActivated = false;
    }

    void SetHealth(float val)
    {
        if (val < 0) val = 0;
        healthCurrent = val;
        float scaleFactor = healthCurrent / healthMax;
        Vector3 scale = healthBarImage.transform.localScale;
        scale.x = scaleFactor;
        healthBarImage.transform.localScale = scale;
        healthBarImage.color = Color.Lerp(healthBarMinColor, healthBarMaxColor, scaleFactor);

        if (healthCurrent <= 0) StartCoroutine(Die());
    }

    public void AddAttackBuddy(Unit unit)
    {
        attackBuddies.Add(unit);
    }

    protected void ResetAttackBuddies()
    {
        attackBuddies.Clear();
    }

    public virtual IEnumerator Attack()
    {
        particleCountDown.Play();

        // 攻击动画
        foreach (Unit unit in attackBuddies)
        {
            unit.animController.Attack();
            unit.Deactivate();
        }
        animController.Attack();
        this.Deactivate();

        // 从棋盘上清楚这些单位
        foreach (Unit unit in attackBuddies)
        {
            BoardManager.instance.BottomHalf_SetUnitAtPosition(null, unit.boardX, unit.boardY);
        }
        BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);

        yield return null;
    }

    protected IEnumerator Remove()
    {
        particleHit.Play();
        yield return new WaitForSeconds(0.3f);

        //BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);
        Destroy(gameObject);
    }

    // 单位接收伤害；返回实际造成的伤害值
    public float TakeDamage(float damage)
    {
        // 计算实际造成的伤害值
        float actualDamage = 0;
        if (healthCurrent >= damage) actualDamage = damage;
        else actualDamage = healthCurrent;

        particleHit.Play();
        SetHealth(healthCurrent - damage);
        return actualDamage;
    }

    IEnumerator Die()
    {
        if (isAtBottom) BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);
        else BoardManager.instance.TopHalf_SetUnitAtPosition(null, boardX, boardY);

        particleDeath.Play();
        CameraEffects.instance.Shake();
        yield return new WaitForSeconds(0.3f);
        
        Destroy(gameObject);
    }
}
