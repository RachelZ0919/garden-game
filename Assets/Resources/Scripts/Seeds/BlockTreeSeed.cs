using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTreeSeed : StartPoint
{
    private List<LevelGrid> gridsUnderTree = new List<LevelGrid>();
    private List<int> gridsLuminance = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        seedType = SeedType.BLOCKTREE;
        InitializeGrid();
    }

    private void Update()
    {
        UpdateNewVisual();
    }

    override public void Activate()
    {
        if (!isActivate)
        {
            ChangeModel(true);
            ShadeGrid();
        }
    }

    override public void Deactivate()
    {
        if (isActivate)
        {
            ChangeModel(false);
            ShadeGrid();
        }
    }

    private void ShadeGrid()
    {
        int luminance;
        if (isActivate)
        {
            foreach (LevelGrid grid in gridsUnderTree)
            {
                grid.luminance = 0;
            }
        }
        else
        {
            for(int i = 0;i < gridsUnderTree.Count; i++)
            {
                gridsUnderTree[i].luminance = gridsLuminance[i];
            }
        }
    }
}
