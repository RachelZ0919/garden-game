using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[Serializable]
public class LevelObjectSet
{
    public string objType;
    public string objPath;
    public string basePath;

    public LevelObjectSet()
    {
        objType = "none";
        objPath = "none";
        basePath = "none";
    }
}

[Serializable]
public class LevelSet
{
    public int ID;
    public string setName;
    public string notSprayablePath;
    public string framePath;
    public string planePath;
    public List<LevelObjectSet> startSets;
    public List<LevelObjectSet> endSets;

    public LevelSet()
    {
        startSets = new List<LevelObjectSet>();
        endSets = new List<LevelObjectSet>();
        setName = "none";
        notSprayablePath = "none";
        framePath = "none";
        planePath = "none";
    }
}

public class LevelSets
{
    public List<LevelSet> levelSets;
    public int nextLevelSetID;
    public LevelSets()
    {
        levelSets = new List<LevelSet>();
    }
}
