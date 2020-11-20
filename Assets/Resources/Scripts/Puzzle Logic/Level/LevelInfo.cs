using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

[Serializable]
public class GridInfo
{
    public int status;
    public WaterColor color;
}

[Serializable]
public class BlockInfo
{
    public List<GridInfo> gridInfo;
}

public class LevelInfo
{
    public List<BlockInfo> blockInfo;
}
