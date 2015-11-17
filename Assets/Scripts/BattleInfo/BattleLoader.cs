using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLoader : MonoBehaviour {
    public static BattleLoader instance = null;

    public int topHp = 1000;
    public int bottomHp = 1000;

    public ButtonGiveMeReserveUnits bottomCallReserveUnitsController;
    public ButtonGiveMeReserveUnits topCallReserveUnitsController;

    public GameObject enemyHpHub;
    public GameObject playerHpHub;

    private StatusBarController enemyHpHubController;
    private StatusBarController playerHpHubController;

    [HideInInspector]
    public Unit[] bottomReserveUnitsQueue;

    [HideInInspector]
    public Unit[] topReserveUnitsQueue;

    [HideInInspector]
    public int bottomNumberOfReserveUnits = 0;

    [HideInInspector]
    public int topNumberOfReserveUnits = 0;

    // Use this for initialization
	public void SetUpBattleLoader ()
    {
        if (instance == null) {
            this.enemyHpHubController = enemyHpHub.GetComponent<StatusBarController>();
            this.playerHpHubController = playerHpHub.GetComponent<StatusBarController>();
            this.playerHpHubController.InitStatus(bottomHp, bottomHp);
            this.enemyHpHubController.InitStatus(topHp, topHp);
            this.bottomReserveUnitsQueue = new Unit[20];
            this.topReserveUnitsQueue = new Unit[20];
            bottomCallReserveUnitsController.SetNumberOfReserveUnits(0);
            topCallReserveUnitsController.SetNumberOfReserveUnits(0);

            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);
    }

    public void DisableCallReserveButtons()
    {
        topCallReserveUnitsController.clickTrigger.interactable = false;
        bottomCallReserveUnitsController.clickTrigger.interactable = false;
    }

    public void EnableBottomCallReserveButton()
    {
        if (bottomNumberOfReserveUnits > 0) bottomCallReserveUnitsController.clickTrigger.interactable = true;
    }

    public void EnableTopCallReserveButton()
    {
        if (topNumberOfReserveUnits > 0) topCallReserveUnitsController.clickTrigger.interactable = true;
    }

    // 补兵
    public void EmptyBottomHalfReserveUnits()
    {
        if (bottomNumberOfReserveUnits == 0) return;
        BoardManager.instance.BottomHalf_CallReserveUnits();
    }

    // 清空补兵queue，并进入下一回合
    public void BottomHalf_ClearReserveUnitsQueueAndUseOneMove()
    {
        ClearReserveUnitsQueue(bottomReserveUnitsQueue);
        bottomNumberOfReserveUnits = 0;
        GameManager.instance.UseOneMove();
    }

    void ClearReserveUnitsQueue(Unit[] queue)
    {
        for (int i = 0; i < queue.Length; i++)
        {
            queue[i] = null;
        }
    }

    // 补兵
    //public void EmptyTopHalfReserveUnits()
    //{
    //    if (topNumberOfReserveUnits == 0) return;
    //    StartCoroutine(BoardManager.instance.CallReserveUnits(false));
    //    TopHalf_ClearReserveUnitsQueueAndUseOneMove();
    //}

    // 清空补兵queue，并进入下一回合
    public void TopHalf_ClearReserveUnitsQueueAndUseOneMove()
    {
        ClearReserveUnitsQueue(topReserveUnitsQueue);
        topNumberOfReserveUnits = 0;
        GameManager.instance.UseOneMove();
    }

    public void MoveToBottomHalfReserveQueue(Unit recycledUnit)
    {
        recycledUnit.ResetUnitStatusForRecycling();
        PutRecycledUnitOutOfStage(recycledUnit);
        bottomReserveUnitsQueue[bottomNumberOfReserveUnits] = recycledUnit;
        bottomNumberOfReserveUnits++;

        // 更新数字显示
        bottomCallReserveUnitsController.SetNumberOfReserveUnits(bottomNumberOfReserveUnits);
    }

    //public void AddToBottomHalfReserveQueue(List<Unit> recycledUnits)
    //{
    //    foreach (Unit t in recycledUnits) {
    //        t.ResetUnitStatusValue();
    //        PutRecycledUnitOutOfStage(t);
    //        bottomReserveUnitsQueue.Add(t);
    //        bottomNumberOfReserveUnits++;
    //        bottomCallReserveUnitsController.SetNumberOfReserveUnits(bottomNumberOfReserveUnits);
    //    }
    //}

    public void MoveToTopHalfReserveQueue(Unit recycledUnit)
    {
        recycledUnit.ResetUnitStatusForRecycling();
        PutRecycledUnitOutOfStage(recycledUnit, false);
        topReserveUnitsQueue[topNumberOfReserveUnits] = recycledUnit;
        topNumberOfReserveUnits++;

        // 更新数字显示
        topCallReserveUnitsController.SetNumberOfReserveUnits(topNumberOfReserveUnits);
    }

    //public void AddToTopHalfReserveQueue(List<Unit> recycledUnits)
    //{
    //    foreach (Unit t in recycledUnits) {
    //        t.ResetUnitStatusValue();
    //        PutRecycledUnitOutOfStage(t, false);
    //        topReserveUnitsQueue.Add(t);
    //        topNumberOfReserveUnits++;
    //        topCallReserveUnitsController.SetNumberOfReserveUnits(topNumberOfReserveUnits);
    //    }
    //}

    private void PutRecycledUnitOutOfStage(Unit recycledUnit, bool isplayer = true)
    {
        Vector3 pos = recycledUnit.transform.position;
        pos.y = isplayer ? -13f : 13f;
        recycledUnit.transform.position = pos;
    }

    public void ChangeTopHp(int delta)
    {
        topHp += delta;
        enemyHpHubController.ChangeHealth(delta);
    }

    public void ChangeBottomHp(int delta)
    {
        bottomHp += delta;
        playerHpHubController.ChangeHealth(delta);
    }
}
