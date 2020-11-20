using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeedSeed : EndPoint
{
    public override string endPointName
    {
        get
        {
            return "Weed";//I deal drugs in my code.
        }
    }

    public override bool[] directionAllowedSpawn => throw new System.NotImplementedException();

    public override void Activate()
    {
        base.Activate();
    }

    public override void Deactivate()
    {
        base.Deactivate();
    }

    public override void UpdateVisual(int num, int code)
    {
        
    }
}
