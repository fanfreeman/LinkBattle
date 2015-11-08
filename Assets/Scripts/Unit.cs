﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class Unit : MonoBehaviour {
    
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
    bool enableParticles = true;

    GameObject unitStatusCanvas;
    UnitStatusController unitStatusController = null;
    Transform unitFeaturesTransform;
    Transform damageTransform;

    private float originalHealth; // 记录单位初始health值，因为heathMax在蓄力时会变

    void Awake()
    {
        unitFeaturesTransform = transform.Find("UnitFeatures");
        damageTransform = unitFeaturesTransform.Find("DamageTransform");

        shaderSetUpScript = GetComponent<ShaderSetUp>();
        animController = unitFeaturesTransform.GetComponent<UnitAnimationController>();
        SetHealth(healthMax);
        originalHealth = healthMax;
        attackBuddies = new List<Unit>(2);

        // 初始化particles
        if (enableParticles)
        {
            Transform particleObject = unitFeaturesTransform.Find("ParticleExplosion");
            particleHit = particleObject.GetComponent<ParticleSystem>();

            particleObject = unitFeaturesTransform.Find("ParticleActivation");
            particleActivation = particleObject.GetComponent<ParticleSystem>();

            particleObject = unitFeaturesTransform.Find("ParticleBurst");
            particleCountDown = particleObject.GetComponent<ParticleSystem>();

            particleObject = unitFeaturesTransform.Find("ParticleBoom");
            particleDeath = particleObject.GetComponent<ParticleSystem>();
        }
        
        unitStatusCanvas = unitFeaturesTransform.Find("UnitStatusCanvas").gameObject;
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
    public void MoveToPosition(int newX, int newY, bool alsoRemoveFromOldPosition = false)
    {
        if (isAtBottom)
        {
            if (alsoRemoveFromOldPosition) BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);
            BoardManager.instance.BottomHalf_SetUnitAtPosition(this, newX, newY);
            Vector3 moveToPos = BoardManager.instance.BottomHalf_GetCoordinatesAtPosition(newX, newY);
            iTween.MoveTo(gameObject, moveToPos, 1f);
        }
        else
        {
            if (alsoRemoveFromOldPosition) BoardManager.instance.TopHalf_SetUnitAtPosition(null, boardX, boardY);
            BoardManager.instance.TopHalf_SetUnitAtPosition(this, newX, newY);
            Vector3 moveToPos = BoardManager.instance.TopHalf_GetCoordinatesAtPosition(newX, newY);
            iTween.MoveTo(gameObject, moveToPos, 1f);
            
        }

        boardX = newX;
        boardY = newY;
        
        SetSortingLayer();
    }

    // 将此单位放在其row所对应的sorting layer
    public void SetSortingLayer()
    {
        if (isAtBottom)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.sortingLayerName = "row" + boardY.ToString();
            }
        }
        else
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.sortingLayerName = "row" + (BoardManager.instance.numRowsPerSide - boardY - 1).ToString();
            }
        }
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
        if (enableParticles)
        {
            particleActivation.gameObject.SetActive(true);
            particleActivation.Play();
        }
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
    public int ChargeUpTickDown()
    {
        if (!isActivated || !isChargeUpLeader) return -1;

        if (enableParticles)
        {
            particleCountDown.gameObject.SetActive(true);
            particleCountDown.Play();
        }

        numTurnsToChargeUpLeft--;
        unitStatusController.SetCountDown(numTurnsToChargeUpLeft);

        // 计算新的attack power
        float healthScaleFactor = healthCurrent / healthMax;
        currentMaxAttackPower += ((attackPower * 0.5f) / numTurnsToChargeUp);
        currentAttackPower = currentMaxAttackPower * healthScaleFactor;
        unitStatusController.SetAttackPower(currentAttackPower);

        if (numTurnsToChargeUpLeft == 0) StartCoroutine(Attack());

        return numTurnsToChargeUpLeft;
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

        // 腾出这些单位在棋盘上的位置
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

    // 播放动画，然后从棋盘上清除此单位
    protected IEnumerator RemoveWithAnimation()
    {
        if (enableParticles)
        {
            particleHit.gameObject.SetActive(true);
            particleHit.Play();
        }
        yield return new WaitForSeconds(0.3f);

        //BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);

        // 回收再利用
        if (isAtBottom)
            BattleLoader.instance.MoveToBottomHalfReserveQueue(this);
        else
            BattleLoader.instance.MoveToTopHalfReserveQueue(this);
        //Destroy(gameObject);
    }

    // 立刻从棋盘上清除此单位
    protected void RemoveImmediately()
    {
        // 回收再利用
        if (isAtBottom)
            BattleLoader.instance.MoveToBottomHalfReserveQueue(this);
        else
            BattleLoader.instance.MoveToTopHalfReserveQueue(this);
        //Destroy(gameObject);
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
        CameraEffects.instance.CreateDamagePopup(actualDamage, damageTransform.position);
        CameraEffects.instance.Shake();

        if (enableParticles)
        {
            particleHit.gameObject.SetActive(true);
            particleHit.Play();
        }
        SetHealth(healthCurrent - damage);
        return actualDamage;
    }

    IEnumerator Die(bool isDeathLeader)
    {
        // 检查是否是正在蓄力的单位组，如是则播放大爆炸粒子效果
        if (isDeathLeader && attackBuddies.Count > 0)
        {
            foreach (Unit buddy in attackBuddies)
            {
                StartCoroutine(buddy.Die(false));
            }

            if (enableParticles)
            {
                particleDeath.gameObject.SetActive(true);
                particleDeath.Play();
            }
        }

        // 腾出棋盘格位
        if (isAtBottom) BoardManager.instance.BottomHalf_SetUnitAtPosition(null, boardX, boardY);
        else BoardManager.instance.TopHalf_SetUnitAtPosition(null, boardX, boardY);
        
        // 等待片刻
        yield return new WaitForSeconds(0.3f);

        // 回收此单位
        if (isAtBottom)
            BattleLoader.instance.MoveToBottomHalfReserveQueue(this);
        else
            BattleLoader.instance.MoveToTopHalfReserveQueue(this);
        //Destroy(gameObject);
    }

    // 回收单位时重置属性
    public void ResetUnitStatusForRecycling()
    {
        healthMax = originalHealth;
        SetHealth(healthMax);

        ResetAttackBuddies();
        isChargeUpLeader = false;

        numTurnsToChargeUpLeft = 0;

        //if (unitStatusController != null)
        //{
            unitStatusController = null;
            unitStatusCanvas.SetActive(false);
        //}

        particleActivation.gameObject.SetActive(false);
        particleCountDown.gameObject.SetActive(false);
        particleHit.gameObject.SetActive(false);
        particleDeath.gameObject.SetActive(false);

        //重新开启mouseover高亮
        shaderSetUpScript.isMouseOverEffectEnabled = true;
    }
}
