using UnityEngine;
using System.Collections;

//帮助playmaker存store array
public class PlaymakerGameObjectArray : MonoBehaviour
{

    private GameObject[] store
    {
     	get { return store; }
    }
	public PlaymakerGameObjectArray(GameObject[] store)
    {
        store = store;
    }
}
