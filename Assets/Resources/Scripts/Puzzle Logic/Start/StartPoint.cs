using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StartPoint : MonoBehaviour
{
    abstract public int waterCanSupply 
    {
        get;
    }

    abstract public string startPointName
    {
        get;
    }

    abstract public bool[] directionAllowedSpawn
    {
        get;
    }

    public string objectPath { get; set; }

    abstract public void UpdateVisual(int num, int code);

}
