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

    private List<Unit> bottomReserveUnitsQueue;
    private List<Unit> topReserveUnitsQueue;

    private int bottomNumberOfReserveUnits = 0;
    private int topNumberOfReserveUnits = 0;

    // Use this for initialization
	public void SetUpBattleLoader ()
    {
        if (instance == null) {
            this.enemyHpHubController = enemyHpHub.GetComponent<StatusBarController>();
            this.playerHpHubController = playerHpHub.GetComponent<StatusBarController>();
            this.playerHpHubController.InitStatus(bottomHp, bottomHp);
            this.enemyHpHubController.InitStatus(topHp, topHp);
            this.bottomReserveUnitsQueue = new List<Unit>();
            this.topReserveUnitsQueue = new List<Unit>();
            bottomCallReserveUnitsController.SetNumberOfReserveUnits(0);
            topCallReserveUnitsController.SetNumberOfReserveUnits(0);

            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);
    }

    public void EnableBottomCallReserveButton()
    {
        topCallReserveUnitsController.clickTrigger.interactable = false;
        if (bottomNumberOfReserveUnits > 0) bottomCallReserveUnitsController.clickTrigger.interactable = true;
    }

    public void EnableTopCallReserveButton()
    {
        bottomCallReserveUnitsController.clickTrigger.interactable = false;
        if (topNumberOfReserveUnits > 0) topCallReserveUnitsController.clickTrigger.interactable = true;
    }

    // 补兵
    public void EmptyBottomHalfReserveUnits()
    {
        if (bottomNumberOfReserveUnits == 0) return;
        BoardManager.instance.CallReserveUnits(bottomReserveUnitsQueue, true);
        bottomReserveUnitsQueue.Clear();
        bottomNumberOfReserveUnits = 0;
        GameManager.instance.UseOneMove();
    }

    // 补兵
    public void EmptyTopHalfReserveUnits()
    {
        if (topNumberOfReserveUnits == 0) return;
        BoardManager.instance.CallReserveUnits(topReserveUnitsQueue, false);
        topReserveUnitsQueue.Clear();
        topNumberOfReserveUnits = 0;
        GameManager.instance.UseOneMove();
    }

    public void MoveToBottomHalfReserveQueue(Unit recycledUnit)
    {
        recycledUnit.ResetUnitStatusForRecycling();
        PutRecycledUnitOutOfStage(recycledUnit);
        bottomReserveUnitsQueue.Add(recycledUnit);
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
        topReserveUnitsQueue.Add(recycledUnit);
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
