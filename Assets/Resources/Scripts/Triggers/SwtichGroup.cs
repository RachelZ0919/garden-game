using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwtichGroup : MonoBehaviour
{
    private Switch[] switches;
    public bool initialState = true;

    private void Awake()
    {
        switches = GetComponentsInChildren<Switch>();
        foreach(Switch sw in switches)
        {
            sw.initialState = initialState;
        }
    }

    public void SealGrids()
    {
        foreach(Switch sw in switches)
        {
            sw.SealGrid();
        }
    }

    public void UnsealGrids()
    {
        foreach(Switch sw in switches)
        {
            sw.UnsealGrid();
        }
    }
}
