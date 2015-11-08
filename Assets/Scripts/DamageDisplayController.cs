using UnityEngine;
using System.Collections;

public class DamageDisplayController : MonoBehaviour {

	void RemoveSelf()
    {
        //Debug.Log("destroying self");
        Destroy(transform.parent.gameObject);
    }
}
