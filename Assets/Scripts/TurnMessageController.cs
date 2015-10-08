using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TurnMessageController : MonoBehaviour {

    Text messageText;

	// Use this for initialization
	void Start () {
        messageText = GetComponentInChildren<Text>();
    }

    public void Show()
    {
        StartCoroutine(ShowMessage());
    }
	
	IEnumerator ShowMessage()
    {
        if (GameManager.instance.playersTurn) messageText.text = "Player Turn";
        else messageText.text = "Enemy Turn";

        // 起始位置
        Vector3 position = gameObject.transform.position;
        position.x = -15f;
        gameObject.transform.position = position;


        position.x = 0f;
        iTween.MoveTo(gameObject, position, 1f);
        yield return new WaitForSeconds(2f);

        position.x = 15f;
        iTween.Stop(gameObject);
        iTween.MoveTo(gameObject, position, 1f);

        GameManager.instance.TurnStartStep_ChargingUnitsTickDown();
    }
}
