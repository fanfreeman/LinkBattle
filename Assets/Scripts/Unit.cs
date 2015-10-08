using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class Unit : MonoBehaviour {

    public Transform damageTransform;
    public GameObject damageDisplayPrefab;
    public float healthMax = 100f;
    public int numTurnsToChargeUp = 1;
    int numTurnsToChargeUpLeft;
    public float attackPower = 250f; // 整个小组蓄力满之后的攻击力

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

    public abstract string GetTypeString();

    ShaderSetUp shaderSetUpScript;
    UnitAnimationController animController;

    float healthCurrent { get; set; }
    bool isChargeUpLeader { get; set; }
    protected float currentAttackPower { get; set; } // 被health影响的已经蓄力好的攻击力
    protected float currentMaxAttackPower { get; set; } // 不被health影响的已经蓄力好的攻击力

    protected List<Unit> attackBuddies;
    protected Unit buddyTwoInFront;
    ParticleSystem particleHit;
    ParticleSystem particleActivation;
    ParticleSystem particleCountDown;
    ParticleSystem particleDeath;

    GameObject unitStatusCanvas;
    UnitStatusController unitStatusController = null;

    void Awake()
    {
        shaderSetUpScript = GetComponent<ShaderSetUp>();
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

        unitStatusCanvas = transform.Find("UnitStatusCanvas").gameObject;
    }

    void InitUnitStatusControllerIfNeeded()
    {
        if (unitStatusController == null)
        {
            unitStatusCanvas.SetActive(true);
            unitStatusController = unitStatusCanvas.GetComponent<UnitStatusController>();
        }
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
    public void ActivateChargeUp(Unit unitInFront, Unit unitTwoInFront, bool isLeader)
    {
        isActivated = true;
        isChargeUpLeader = isLeader;
        buddyTwoInFront = unitTwoInFront;

        // 显示效果
        particleActivation.gameObject.SetActive(true);
        particleActivation.Play();
        shaderSetUpScript.isMouseOverEffectEnabled = false;

        // 添加战友
        AddAttackBuddy(unitInFront);
        AddAttackBuddy(unitTwoInFront);
        
        // 如果不是队长，隐藏health bar
        if (unitStatusController != null && !isLeader) unitStatusController.HideHealth();

        // 如果是队长，则同时启动另外两个队友
        if (isLeader)
        {
            healthMax *= 3;
            healthCurrent = healthCurrent + unitInFront.healthCurrent + unitTwoInFront.healthCurrent;

            unitInFront.ActivateChargeUp(this, unitTwoInFront, false);
            unitTwoInFront.ActivateChargeUp(this, unitInFront, false);

            InitUnitStatusControllerIfNeeded();

            // 计算attack power
            currentMaxAttackPower = attackPower / 2;
            float healthScaleFactor = healthCurrent / healthMax;
            currentAttackPower = currentMaxAttackPower * healthScaleFactor;
            unitStatusController.SetAttackPower(currentAttackPower);

            // 显示剩余回合数
            numTurnsToChargeUpLeft = numTurnsToChargeUp;
            unitStatusController.SetCountDown(numTurnsToChargeUpLeft);
        }
        else
        {
            healthMax = unitInFront.healthMax;
            healthCurrent = unitInFront.healthCurrent;
        }
    }

    void Deactivate()
    {
        isActivated = false;
    }

    public void AddAttackBuddy(Unit unit)
    {
        attackBuddies.Add(unit);
    }

    protected void ResetAttackBuddies()
    {
        attackBuddies.Clear();
    }

    void RemoveAttackBuddy(Unit buddy)
    {
        attackBuddies.Remove(buddy);
    }

    // 蓄力tick down
    public bool ChargeUpTickDown()
    {
        if (!isActivated || !isChargeUpLeader) return false;

        particleCountDown.gameObject.SetActive(true);
        particleCountDown.Play();

        numTurnsToChargeUpLeft--;
        unitStatusController.SetCountDown(numTurnsToChargeUpLeft);

        // 计算新的attack power
        float healthScaleFactor = healthCurrent / healthMax;
        currentMaxAttackPower += ((attackPower * 0.5f) / numTurnsToChargeUp);
        currentAttackPower = currentMaxAttackPower * healthScaleFactor;
        unitStatusController.SetAttackPower(currentAttackPower);

        if (numTurnsToChargeUpLeft == 0) StartCoroutine(Attack());

        return true;
    }

    protected virtual IEnumerator Attack()
    {
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
            if (isAtBottom) BoardManager.instance.BottomHalf_SetUnitAtPosition(null, unit.boardX, unit.boardY);
            else BoardManager.instance.TopHalf_SetUnitAtPosition(null, unit.boardX, unit.boardY);
        }
        if (isAtBottom) BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);
        else BoardManager.instance.TopHalf_SetUnitAtPosition(null, boardX, boardY);

        yield return null;
    }

    void SetHealth(float val)
    {
        if (val < 0) val = 0;
        healthCurrent = val;
        float scaleFactor = healthCurrent / healthMax;
        if (unitStatusController != null) unitStatusController.SetHealthScale(scaleFactor);

        if (isActivated && isChargeUpLeader)
        {
            currentAttackPower = currentMaxAttackPower * scaleFactor;
            unitStatusController.SetAttackPower(currentAttackPower);
        }

        if (healthCurrent <= 0) StartCoroutine(Die(true));
    }

    protected IEnumerator Remove()
    {
        particleHit.gameObject.SetActive(true);
        particleHit.Play();
        yield return new WaitForSeconds(0.3f);

        //BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);
        Destroy(gameObject);
    }

    // 单位接收伤害；返回实际造成的伤害值
    public float TakeDamage(float damage)
    {
        // 受到伤害后，显示health bar
        InitUnitStatusControllerIfNeeded();

        // 计算实际造成的伤害值
        float actualDamage = 0;
        if (healthCurrent >= damage) actualDamage = damage;
        else actualDamage = healthCurrent;

        // 显示伤害数字
        CreateDamagePopup(actualDamage);

        particleHit.gameObject.SetActive(true);
        particleHit.Play();
        SetHealth(healthCurrent - damage);
        return actualDamage;
    }

    void CreateDamagePopup(float damage)
    {
        GameObject damageGameObject = (GameObject)Instantiate(damageDisplayPrefab, damageTransform.position, Quaternion.identity);
        //damageGameObject.transform.SetParent(damageTransform);
        damageGameObject.GetComponentInChildren<Text>().text = Mathf.Round(damage).ToString();
        Destroy(damageGameObject, 1f);
    }

    IEnumerator Die(bool isDeathLeader)
    {
        // 检查是否是正在蓄力的单位组
        if (isDeathLeader && attackBuddies.Count > 0)
        {
            foreach (Unit buddy in attackBuddies)
            {
                StartCoroutine(buddy.Die(false));
            }

            particleDeath.gameObject.SetActive(true);
            particleDeath.Play();
        }

        if (isAtBottom) BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);
        else BoardManager.instance.TopHalf_SetUnitAtPosition(null, boardX, boardY);
        
        particleHit.gameObject.SetActive(true);
        particleHit.Play();
        CameraEffects.instance.Shake();
        yield return new WaitForSeconds(0.3f);
        
        Destroy(gameObject);
    }
}
