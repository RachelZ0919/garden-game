using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : GridSplitter
{
    public WaterColor color;
    private Vector3 startPoint;
    private Vector3 cubeSize;

    public override LevelGrid GetGridAtPosition(Vector3 position)
    {
        Vector3 offset = position - startPoint;
        offset.x = Mathf.Floor(offset.x);
        offset.z = Mathf.Floor(offset.z);
        float index = offset.x * cubeSize.z + offset.z;
        if(index < 0 || index >= levelGrids.Length)
        {
            return null;
        }
        return levelGrids[(int)index];
    }

    protected override void SplitGrid()
    {
        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        cubeSize = vertices[1] - vertices[0] + vertices[2] - vertices[0] + vertices[1] - vertices[5];
        cubeSize.y = 1;
        startPoint = transform.TransformPoint(vertices[9]);
        int gridCounts = (int)cubeSize.x * (int)cubeSize.z;
        levelGrids = new LevelGrid[gridCounts];
        int gridIndex = 0;
        GridType type = GridType.WATER;

        for (int x = 0; x < (int)cubeSize.x; x++)
        {
            for (int z = 0; z < (int)cubeSize.z; z++, gridIndex++)
            {
                Vector3 position = startPoint + new Vector3(x, 1, z) + new Vector3(0.5f, 0, 0.5f);
                levelGrids[gridIndex] = new LevelGrid(position, Vector3.up, type);
                levelGrids[gridIndex].state = 1;
                levelGrids[gridIndex].groundColor = color;
            }
        }

    }

    void Start()
    {
        FindNearGrids();
        GameManager.AddScanCube(this);
        //LightDetect();
    }

    private void Awake()
    {
        SplitGrid();
        gameObject.layer = 9;
        SetCollideBox();
    }

    private void SetCollideBox()
    {
        GetComponent<BoxCollider>().size = cubeSize;
        GetComponent<BoxCollider>().center = transform.InverseTransformPoint(startPoint + new Vector3(cubeSize.x / 2, 0.5f, cubeSize.z / 2));
    }
}
