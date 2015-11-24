using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//∞Ô÷˙playmaker¥ÊList array
public class PlaymakerUnitArray : MonoBehaviour
{
    private List<Unit> store;
    public List<Unit> GetUnits()
    {
        return store;
    }

    public void SetUnits(List<Unit> input)
    {
        store = new List<Unit>();
        foreach (Unit unit in input)
        {
            if(unit != null) store.Add(unit);
        }
    }
}