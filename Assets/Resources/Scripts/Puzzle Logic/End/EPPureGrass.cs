using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EPPureGrass : EndPoint
{
    public SwtichGroup switches;

    public override string endPointName {
        get
        {
            return "PureGrass";
        }
    }

    public override bool[] directionAllowedSpawn => throw new System.NotImplementedException();

    public override void Activate()
    {
        switches.UnsealGrids();
    }

    public override void Deactivate()
    {
        switches.SealGrids();
    }

    public override void UpdateVisual(int num, int code)
    {
        
    }
}
