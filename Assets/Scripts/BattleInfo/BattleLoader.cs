using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLoader : MonoBehaviour {
    public static BattleLoader instance = null;

    public int enemyHP = 1000;
    public int playerHP = 1000;

    public ButtonGiveMeAttacker playerButtonGiveMeAttacker;
    public ButtonGiveMeAttacker enemyButtonGiveMeAttacker;

    public GameObject enemyHpHub;
    public GameObject playerHpHub;

    private StatusBarController enemyHpHubController;
    private StatusBarController playerHpHubController;

    private List<Unit> playerUsedAttackerQueue;
    private List<Unit> enemyUsedAttackerQueue;

    private int playerNumberOfReserveUnits = 0;
    private int enemyNumberOfReserveUnits = 0;

    // Use this for initialization
	public void SetUpBattleLoader () {
        if (instance == null) {
            this.enemyHpHubController = enemyHpHub.GetComponent<StatusBarController>();
            this.playerHpHubController = playerHpHub.GetComponent<StatusBarController>();
            this.playerHpHubController.InitStatus(playerHP, playerHP);
            this.enemyHpHubController.InitStatus(enemyHP, enemyHP);
            this.playerUsedAttackerQueue = new List<Unit>();
            this.enemyUsedAttackerQueue = new List<Unit>();
            playerButtonGiveMeAttacker.SetNumberOfReserveUnits(0);
            enemyButtonGiveMeAttacker.SetNumberOfReserveUnits(0);
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);
    }

    public void FillUpPlayerAttacker(){
        if(playerNumberOfReserveUnits == 0)return;
        BoardManager.instance.FillUpAttacker(playerUsedAttackerQueue,true);
        playerUsedAttackerQueue.Clear();
        playerNumberOfReserveUnits = 0;
        GameManager.instance.UseOneMove();
    }

    public void FillUpEnemyAttacker(){
        if(enemyNumberOfReserveUnits == 0)return;
        BoardManager.instance.FillUpAttacker(enemyUsedAttackerQueue,false);
        enemyUsedAttackerQueue.Clear();
        enemyNumberOfReserveUnits = 0;
        GameManager.instance.UseOneMove();
    }

    public void AddToPlayerUsedAttackerQueue(Unit attacker){
        attacker.ResetUnitStatusValue();
        putAttackerOutOfStage(attacker);
        playerUsedAttackerQueue.Add(attacker);
        playerNumberOfReserveUnits++;
        playerButtonGiveMeAttacker.SetNumberOfReserveUnits(playerNumberOfReserveUnits);
    }

    public void AddToPlayerUsedAttackerQueue(List<Unit> attackers){
        foreach (Unit t in attackers) {
            t.ResetUnitStatusValue();
            putAttackerOutOfStage(t);
            playerUsedAttackerQueue.Add(t);
            playerNumberOfReserveUnits++;
            playerButtonGiveMeAttacker.SetNumberOfReserveUnits(playerNumberOfReserveUnits);
        }
    }

    public void AddToEnemyUsedAttackerQueue(Unit attacker){
        attacker.ResetUnitStatusValue();
        putAttackerOutOfStage(attacker, false);
        enemyUsedAttackerQueue.Add(attacker);
        enemyNumberOfReserveUnits++;
        enemyButtonGiveMeAttacker.SetNumberOfReserveUnits(enemyNumberOfReserveUnits);
    }

    public void AddToEnemyUsedAttackerQueue(List<Unit> attackers){
        foreach (Unit t in attackers) {
            t.ResetUnitStatusValue();
            putAttackerOutOfStage(t, false);
            enemyUsedAttackerQueue.Add(t);
            enemyNumberOfReserveUnits++;
            enemyButtonGiveMeAttacker.SetNumberOfReserveUnits(enemyNumberOfReserveUnits);
        }
    }

    private void putAttackerOutOfStage(Unit attacker, bool isplayer = true){
        Vector3 pos = attacker.transform.position;
        pos.y = isplayer?-13f:13f;
        attacker.transform.position = pos;
    }

    public void ChangeEnemyHp(int delta) {
        enemyHP += delta;
        enemyHpHubController.ChangeHealth(delta);
    }

    public void ChangePlayerHp(int delta) {
        playerHP += delta;
        playerHpHubController.ChangeHealth(delta);
    }
}
