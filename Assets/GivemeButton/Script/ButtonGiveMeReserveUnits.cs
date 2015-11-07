using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class ButtonGiveMeReserveUnits : MonoBehaviour {
	public Text numText;
    public Button clickTrigger;

    [Serializable]
    public class MyEventType : UnityEvent { }
    public MyEventType OnEvent;

    public void SetNumberOfReserveUnits(int num) {
        numText.text = num.ToString();
    }

    private void TriggerClick( ){
        numText.text ="0";
        OnEvent.Invoke();
    }

	void Start () {
        clickTrigger.onClick.AddListener(delegate() {
            this.TriggerClick();
        });
	}
}
