using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPFlowingWater : StartPoint
{
    public override string startPointName {
        get
        {
            return "FlowingWater";
        }
    }

    public override int waterCanSupply
    {
        get
        {
            return -1;
        }
    }

    public override bool[] directionAllowedSpawn
    {
        get
        {
            return new[] { false, false, false, false, true, false };
        }
    }

    public override void UpdateVisual(int num, int code)
    {
        Transform childTransform = transform.Find($"{num}");
        if (childTransform.childCount > 0)
        {
            DestroyImmediate(childTransform.GetChild(0).gameObject);
        }

        if (code != -1)
        {
            GameObject sob = ItemLoader.LoadObject(objectPath, code);
            if (sob != null)
            {
                Instantiate(sob, childTransform);
            }
        }
    }
}
