using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void StartScan();
public delegate void UpdateGridVisual();

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    static StartScan startScan;
    static UpdateGridVisual updateGridVisual;
    static public Shader lawnShader;
    static public Texture2D brushTexture;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    public static void AddScanCube(GridSplitter splitter)
    {
        startScan += splitter.ResetScanGrid;
        updateGridVisual += splitter.VisualUpdateRequest;
    }

    public static void AddStartPoint(StartPoint start)
    {
        updateGridVisual += start.VisualUpdateRequest;
    }

    public static void ResetScanGrid()
    {
        startScan();
    }

    public static void NextScene()
    {

    }

    public static void RestartScene()
    {
        startScan();
    }

    public static void VisualUpdate()
    {
        updateGridVisual();
    }
}
