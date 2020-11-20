using System.Collections;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor.Rendering;
using UnityEditor.Experimental.TerrainAPI;

public static class SaveLoadManager
{
    public static void SaveLevel()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/Level.sav", FileMode.Create); //打开文件

        LevelData data = new LevelData();
        bf.Serialize(stream, data);
        stream.Close();
    }

    public static void LoadLevel()
    {

    }
}

[Serializable]
public class LevelData
{
    //用数组会存起来快

    public LevelData()
    {
        //构造
    }


}