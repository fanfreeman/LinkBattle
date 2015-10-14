using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class ButtonGiveMeAttacker : MonoBehaviour {
	public Text numText;
    public Button clickTrigger;
	// Use this for initialization
    public Action OnClick;

    [Serializable]
    public class MyEventType : UnityEvent { }
    public MyEventType OnEvent;

    public void SetNumberOfReserveUnits(int num){
        numText.text = num.ToString();

    }

    private void TriggerClick( ){
        numText.text ="0";
        OnEvent.Invoke();
    }

    private void test( ){

    }


	void Start () {
        clickTrigger.onClick.AddListener(delegate() {
            this.TriggerClick();
        });
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
