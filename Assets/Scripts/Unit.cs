﻿using UnityEngine;
using System.Collections;

public abstract class Unit : MonoBehaviour {

    public abstract string GetTypeString();

    void OnMouseDown()
    {
        Debug.Log("clicked me!");
    }
}
