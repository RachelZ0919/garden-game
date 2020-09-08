using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeedSeed : StartPoint
{
    public override void Activate()
    {
        //什么也不做，只占空间
    }

    public override void Deactivate()
    {
    }

    private void Start()
    {
        seedType = SeedType.WEED;
        InitializeGrid();
    }

    private void Update()
    {
        UpdateNewVisual();
    }
}
