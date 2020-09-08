using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PureGrassSeed : StartPoint
{
    public SwtichGroup switches;
    public override void Activate()
    {
        ChangeModel(true);
        switches.UnsealGrids();
    }

    public override void Deactivate()
    {
        ChangeModel(false);
        switches.SealGrids();
    }

    private void Update()
    {
        UpdateNewVisual();
    }


    private void Start()
    {
        seedType = SeedType.PUREGRASS;
        InitializeGrid();
    }
}
