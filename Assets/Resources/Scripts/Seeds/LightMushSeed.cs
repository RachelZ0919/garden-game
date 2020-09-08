using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMushSeed : StartPoint
{
    private List<Door> doors = new List<Door>();

    // Start is called before the first frame update
    void Start()
    {
        InitializeGrid();
        InitializeDoor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    override public void Activate()
    {
        ChangeModel(true);
        SeasameDoor();
    }

    override public void Deactivate()
    {
        ChangeModel(false);
        SeasameDoor();
    }

    private void SeasameDoor()
    {
        if (isActivate)
        {
            foreach(Door door in doors)
            {
                door.openLock();
            }
        }
        else
        {
            foreach (Door door in doors)
            {
                door.addLock();
            }
        }
    }

    private void InitializeDoor()
    {
        //TODO:加门（或许直接手动）
        foreach (Door door in doors)
        {
            door.addLock();
        }
    }
}
