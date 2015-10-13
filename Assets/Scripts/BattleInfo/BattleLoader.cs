using UnityEngine;
using System.Collections;

public class BattleLoader : MonoBehaviour {
    public static BattleLoader instance = null;

    public int enemyHP = 1000;
    public int playerHP = 1000;
    
    public GameObject enemyHpHub;
    public GameObject playerHpHub;

    private StatusBarController enemyHpHubController;
    private StatusBarController playerHpHubController;

	// Use this for initialization
	public void SetUpBattleLoader () {
        if (instance == null) {
            this.enemyHpHubController = enemyHpHub.GetComponent<StatusBarController>();
            this.playerHpHubController = playerHpHub.GetComponent<StatusBarController>();
            this.playerHpHubController.InitStatus(playerHP, playerHP);
            this.enemyHpHubController.InitStatus(enemyHP, enemyHP);
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);
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
