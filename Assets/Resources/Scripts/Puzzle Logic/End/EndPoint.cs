using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class EndPoint : MonoBehaviour
{
    [HideInInspector]
    public bool isActivate = false;
    public WaterColor color;
    public string objectPath { get; set; }

    abstract public string endPointName
    {
        get;
    }

    abstract public bool[] directionAllowedSpawn
    {
        get;
    }

    virtual public void Activate()
    {
        if (isActivate)
        {
            return;
        }
        isActivate = true;
    }

    virtual public void Deactivate()
    {
        if (!isActivate)
        {
            return;
        }
        isActivate = false;
    }

    abstract public void UpdateVisual(int num, int code);

}
