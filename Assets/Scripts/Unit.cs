using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class Unit : MonoBehaviour {

    public int id { get; set; }
    
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
    [HideInInspector]
    public bool isBarricade = false;

    protected static int outOfTopEdgeY = 12;
    protected static int outOfBottomEdgeY = -12;

    public abstract string GetTypeString();
    public abstract BoardManager.UnitTypes GetUnitType();

    ShaderSetUp shaderSetUpScript;
    UnitAnimationController animController;

    float healthCurrent { get; set; }
    protected float currentAttackPower { get; set; } // 被health影响的已经蓄力好的攻击力
    protected float currentFullHealthAttackPower { get; set; } // 不被health影响的已经蓄力好的攻击力

    protected List<Unit> attackBuddies;
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
    private bool isChargeUpFlagHolder = false; // 三连一组的蓄力组里显示status的那个单位
    private BoxCollider2D myCollider;

    private static int currentUnitIdToAssign = 0;

    GameObject barricadeObj;

    void Awake()
    {
        myCollider = GetComponent<BoxCollider2D>();
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
        
        // 初始化状态显示
        unitStatusCanvas = unitFeaturesTransform.Find("UnitStatusCanvas").gameObject;

        // 初始化路障
        Vector3 position = transform.position;
        position.y += 0.5f;
        barricadeObj = Instantiate(BoardManager.instance.barricadePrefab, position, Quaternion.identity) as GameObject;
        barricadeObj.transform.parent = this.transform;
        barricadeObj.SetActive(false);
    }

    void InitUnitStatusControllerIfNeeded()
    {
        if (isActivated)
        {
            if (unitStatusController == null && isChargeUpFlagHolder)
            {
                unitStatusCanvas.SetActive(true);
                unitStatusController = unitStatusCanvas.GetComponent<UnitStatusController>();
            }
        }
        else // not activated
        {
            if (unitStatusController == null)
            {
                unitStatusCanvas.SetActive(true);
                unitStatusController = unitStatusCanvas.GetComponent<UnitStatusController>();
            }
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

        SetSortingLayerForObjectAccordingToRow(null);
    }

    // 将对象位放在其row所对应的sorting layer
    public void SetSortingLayerForObjectAccordingToRow(GameObject obj)
    {
        if (obj == null) obj = this.gameObject;

        if (isAtBottom)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.sortingLayerName = "row" + boardY.ToString();
            }
        }
        else
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
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

                    // send network message
                    BoardManager.instance.SendPickUpUnit(boardX, boardY);
                }
            }
            else Debug.Log("It's the top player's turn");
        }
        //else
        //{
        //    if (!GameManager.instance.playersTurn)
        //    {
        //        // 如此单位在column底部，将其移出该column（捡起）
        //        if (BoardManager.instance.unitBeingPickedUp == null && !isActivated && BoardManager.instance.TopHalf_CheckIfUnitIsAtTail(this))
        //        {
        //            Vector3 moveToPos = BoardManager.instance.TopHalf_GetCoordinatesAtPosition(boardX, boardY);
        //            moveToPos.y = outOfTopEdgeY;
        //            iTween.MoveTo(gameObject, moveToPos, 1f);
        //            BoardManager.instance.unitBeingPickedUp = this;
        //            BoardManager.instance.TopHalf_SetUnitAtPosition(null, boardX, boardY);

        //            GameManager.instance.SetColumnHighlightEnabled(true, moveToPos.x);
        //        }
        //    }
        //    else Debug.Log("It's the bottom player's turn");
        //}
    }

    public void NetworkPickUpEnemyUnit()
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

    // 三个连在一起的单位开始蓄力
    public void ActivateChargeUp(Unit otherBuddy, Unit otherBuddy2, bool isFirst)
    {
        isActivated = true;

        // 显示效果
        if (enableParticles)
        {
            particleActivation.gameObject.SetActive(true);
            particleActivation.Play();
        }
        shaderSetUpScript.isMouseOverEffectEnabled = false;

        // 添加战友
        AddAttackBuddy(otherBuddy);
        AddAttackBuddy(otherBuddy2);
        
        // 如果不是队长，隐藏health bar
        if (unitStatusController != null) unitStatusController.HideHealth();

        // 剩余回合数
        numTurnsToChargeUpLeft = numTurnsToChargeUp;
        
        // 如果是三个里第一个activate的单位，则给另外两个队友赋值，并activate它们
        if (isFirst)
        {
            // 计算最大health
            healthMax *= 3;
            otherBuddy.healthMax = healthMax;
            otherBuddy2.healthMax = healthMax;

            // 计算实际health
            healthCurrent = healthCurrent + otherBuddy.healthCurrent + otherBuddy2.healthCurrent;
            otherBuddy.healthCurrent = healthCurrent;
            otherBuddy2.healthCurrent = healthCurrent;

            // 计算满血attack power
            currentFullHealthAttackPower = attackPower / 2;
            otherBuddy.currentFullHealthAttackPower = currentFullHealthAttackPower;
            otherBuddy2.currentFullHealthAttackPower = currentFullHealthAttackPower;

            // 计算实际attack power
            float healthScaleFactor = healthCurrent / healthMax;
            currentAttackPower = currentFullHealthAttackPower * healthScaleFactor;
            otherBuddy.currentAttackPower = currentAttackPower;
            otherBuddy2.currentAttackPower = currentAttackPower;

            otherBuddy.ActivateChargeUp(this, otherBuddy2, false);
            otherBuddy2.ActivateChargeUp(this, otherBuddy, false);
        }
    }

    public void EnableChargeUpStatusDisplay()
    {
        isChargeUpFlagHolder = true;
        InitUnitStatusControllerIfNeeded();
        unitStatusController.ShowHealth();
        unitStatusController.SetAttackPower(currentAttackPower);
        unitStatusController.SetCountDown(numTurnsToChargeUpLeft);
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
        if (!isActivated || !isChargeUpFlagHolder) return -1; // 只有显示status的单位可以让整组tick down

        if (enableParticles)
        {
            particleCountDown.gameObject.SetActive(true);
            particleCountDown.Play();
        }
        
        numTurnsToChargeUpLeft--;
        unitStatusController.SetCountDown(numTurnsToChargeUpLeft);

        // 计算新的attack power
        float healthScaleFactor = healthCurrent / healthMax;
        currentFullHealthAttackPower += ((attackPower * 0.5f) / numTurnsToChargeUp);
        currentAttackPower = currentFullHealthAttackPower * healthScaleFactor;
        unitStatusController.SetAttackPower(currentAttackPower);

        foreach (Unit buddy in attackBuddies)
        {
            buddy.currentFullHealthAttackPower = currentFullHealthAttackPower;
            buddy.currentAttackPower = currentAttackPower;
        }

        if (numTurnsToChargeUpLeft == 0) StartCoroutine(Attack());

        return numTurnsToChargeUpLeft;
    }

    protected virtual IEnumerator Attack()
    {
        // 攻击动画
        foreach (Unit unit in attackBuddies)
        {
            unit.animController.Attack();
            unit.isActivated = false;
        }
        animController.Attack();
        isActivated = false;

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

    // 给单位赋予新的health值
    void SetHealth(float val)
    {
        if (val < 0) val = 0;
        healthCurrent = val;
        float scaleFactor = healthCurrent / healthMax;
        
        if (isActivated)
        {
            currentAttackPower = currentFullHealthAttackPower * scaleFactor;

            if (isChargeUpFlagHolder)
            {
                unitStatusController.SetHealthScale(scaleFactor);
                unitStatusController.SetAttackPower(currentAttackPower);
            }

            // 还需要修改蓄力组里其它成员的数值
            foreach (Unit buddy in attackBuddies)
            {
                buddy.healthCurrent = healthCurrent;
                buddy.currentAttackPower = currentAttackPower;

                if (buddy.isChargeUpFlagHolder)
                {
                    buddy.unitStatusController.SetHealthScale(scaleFactor);
                    buddy.unitStatusController.SetAttackPower(currentAttackPower);
                }
            }
        }
        else // not activated
        {
            if (unitStatusController != null) unitStatusController.SetHealthScale(scaleFactor);
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
    }

    // 立刻从棋盘上清除此单位
    protected void RemoveImmediately()
    {
        // 回收再利用
        if (isAtBottom)
            BattleLoader.instance.MoveToBottomHalfReserveQueue(this);
        else
            BattleLoader.instance.MoveToTopHalfReserveQueue(this);
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
        myCollider.enabled = false; // 避免已经挂掉的单位据需和projectile碰撞

        // 检查是否是正在蓄力的单位组，如是则播放大爆炸粒子效果
        if (isActivated && isDeathLeader && attackBuddies.Count > 0)
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
    }

    // 三个连在一起的单位开始蓄力
    public void TurnIntoBarricade(float newHealthMax)
    {
        isBarricade = true;

        // 隐藏单位美术
        Transform unitArt = transform.Find("Hip");
        unitArt.gameObject.SetActive(false);

        // 显示路障
        barricadeObj.SetActive(true);

        // DEBUG: 设置路障sorting order
        SetSortingLayerForObjectAccordingToRow(barricadeObj);

        //shaderSetUpScript.isMouseOverEffectEnabled = false;

        // 如果不是队长，隐藏health bar
        //if (unitStatusController != null) unitStatusController.HideHealth();

        healthMax = newHealthMax;
        healthCurrent = healthMax;
    }

    // 回收单位时重置属性
    public void ResetUnitStatusForRecycling()
    {
        isActivated = false;

        if (isBarricade)
        {
            isBarricade = false;

            // TODO optimize the following
            Transform unitArt = transform.Find("Hip");
            unitArt.gameObject.SetActive(true);

            // TODO optimize the following
            barricadeObj.SetActive(false);
        }
        
        myCollider.enabled = true;
        healthMax = originalHealth;
        SetHealth(healthMax);

        ResetAttackBuddies();
        isChargeUpFlagHolder = false;

        if (unitStatusController != null) unitStatusController.Clear();
        unitStatusController = null;
        unitStatusCanvas.SetActive(false);

        particleActivation.gameObject.SetActive(false);
        particleCountDown.gameObject.SetActive(false);
        particleHit.gameObject.SetActive(false);
        particleDeath.gameObject.SetActive(false);

        //重新开启mouseover高亮
        shaderSetUpScript.isMouseOverEffectEnabled = true;
    }

    // 给此单位赋id值
    public void AssignNewId()
    {
        id = currentUnitIdToAssign;
        currentUnitIdToAssign++;
    }
}
