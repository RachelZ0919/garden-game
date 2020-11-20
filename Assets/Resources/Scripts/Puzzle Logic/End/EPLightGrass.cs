using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EPLightGrass : EndPoint
{
    public Trigger triggerToActivate;

    private void Awake()
    {
        if(triggerToActivate != null)
        {
            triggerToActivate.Register();
        }
    }

    public override string endPointName
    {
        get
        {
            return "LightGrass";
        }
    }

    public override bool[] directionAllowedSpawn
    {
        get
        {
            return new[] { true, true, true, true, true, true };
        }
    }

    override public void Activate()
    {
        base.Activate();
        triggerToActivate.ActivateTrigger();
    }

    override public void Deactivate()
    {
        base.Deactivate();
        triggerToActivate.DeactivateTrigger();
    }

    public override void UpdateVisual(int num, int code)
    {
        //do nothing tee hee
    }
}
