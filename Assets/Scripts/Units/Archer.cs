using UnityEngine;
using System.Collections;
using System;

public class Archer : Unit {

    public GameObject projectile;

    public override string GetTypeString()
    {
        return "Archer";
    }
}
