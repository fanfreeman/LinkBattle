// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour {
				
	public CharController knight;
	public CharController barbarian;
	public CharController ninja;

	private bool restartNecessary = false;

	void Start() {
		StartCoroutine( StartBattleScenario() );
		StartCoroutine( CheckHeroesDeath() );
	}

	private IEnumerator StartBattleScenario() {
		yield return new WaitForSeconds(.1f);

		knight.Command_WalkTo(new Vector3(-4f, -2.5f, 0f));
		barbarian.Command_WalkTo(new Vector3(-6.5f, -1f, 0f));
		ninja.Command_WalkTo(new Vector3(-5f, -4f, 0f));

		yield return new WaitForSeconds(3f);
		AI_Controller.Instance.SendWave_1();

		do {
			yield return new WaitForSeconds(1f);
		} while (!AI_Controller.Instance.AreAllMinionsDead());

		yield return new WaitForSeconds(3f);
		AI_Controller.Instance.SendWave_2();

		yield return new WaitForSeconds(15f);
		AI_Controller.Instance.SendWave_3();

		do {
			yield return new WaitForSeconds(1f);
		} while (!AI_Controller.Instance.AreAllMinionsDead());

		yield return new WaitForSeconds(3f);
		AI_Controller.Instance.SendWave_4();

		do {
			yield return new WaitForSeconds(1f);
		} while (!AI_Controller.Instance.AreAllMinionsDead());

		yield return new WaitForSeconds(3f);
		AI_Controller.Instance.SendWave_5();

	}

	private IEnumerator CheckHeroesDeath() {
		while (!AreAllHeroesDead()) {
			yield return new WaitForSeconds(3f);
		}

		restartNecessary = true;
	}

	private bool AreAllHeroesDead() {
		Hero[] heroes = FindObjectsOfType<Hero>();
		foreach(Hero hero in heroes) {
			if ( (hero != null) && !hero.IsDead() ) {
				return false;
			}
		}
		return true;
	}


	void OnGUI() {
		if (restartNecessary) {
			if (GUI.Button(new Rect(10, 10, 100, 30), "Restart")) {
				Application.LoadLevel("Demo3");
			}
		}
	}
}

