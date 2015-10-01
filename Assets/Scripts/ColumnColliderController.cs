using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColumnColliderController : MonoBehaviour {

    BoxCollider2D[] arrColliders;

	// Use this for initialization
	void Start () {
        arrColliders = GetComponentsInChildren<BoxCollider2D>();
        //DisableAllColumnColliders();
	}
	
	void EnableAllColumnColliders()
    {
        foreach (BoxCollider2D columnCollider in arrColliders)
        {
            columnCollider.enabled = true;
        }
    }

    void DisableAllColumnColliders()
    {
        foreach (BoxCollider2D columnCollider in arrColliders)
        {
            columnCollider.enabled = false;
        }
    }
}
