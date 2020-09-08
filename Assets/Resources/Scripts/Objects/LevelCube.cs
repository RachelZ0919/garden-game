using UnityEngine;
using System.Collections;



public class LevelCube : GridSplitter
{
    /*
        这个类附在每个场景物体中，它会在场景加载时分割场景数据，并完成初始化的工作。
        它也负责返回raycast击中位置的grid
    */

    public GridType type;

    private Vector3 startPoint;
    private Vector3 cubeSize;
    private Lawn[] lawns;
    //private GridType type;
    private GameObject fencePrefab;

    private void Awake()
    {
        fencePrefab = Resources.Load<GameObject>("Prefabs/FencePrefab");
        //InitializeType();
        SplitGrid();
        SetupLawn();
    }
    void Start()
    {
        FindNearGrids();
        GameManager.AddScanCube(this);
        //LightDetect();
        //if(type == GridType.SOIL) AddFence();
    }
    private void Update()
    {
        if (needVisualUpdate)
        {
            needVisualUpdate = false;
            UpdateGridVisual();
        }
    }
    public void UpdateGridVisual()
    {
        //visual
        if(levelGrids != null)
        {
            foreach (LevelGrid grid in levelGrids)
            {
                if (grid.type == GridType.SOIL || grid.type == GridType.GROUND)
                {
                    UpdateLawn(grid);
                }
            }
        }

    }
    //输入世界坐标，返回对应位置的LevelGrid对象
    public override LevelGrid GetGridAtPosition(Vector3 position)
    {
        Vector3 offset = position - startPoint;
        int index;
        if (Mathf.Abs(offset.x - 0) < 0.01f || Mathf.Abs(offset.x - cubeSize.x) < 0.01f)
        {
            offset.y = Mathf.Floor(offset.y);
            offset.z = Mathf.Floor(offset.z);
            if (Mathf.Abs(offset.x - cubeSize.x) < 0.01f)
            {
                float indexoffset = cubeSize.x * cubeSize.y * 2 + cubeSize.x * cubeSize.z * 2;
                index = (int)indexoffset + (int)offset.y * (int)cubeSize.z * 2 + (int)offset.z * 2;
            }
            else
            {
                float indexoffset = cubeSize.x * cubeSize.y * 2 + cubeSize.x * cubeSize.z * 2;
                index = (int)indexoffset + (int)offset.y * (int)cubeSize.z * 2 + (int)offset.z * 2 + 1;
            }
        }
        else if (Mathf.Abs(offset.y - 0) < 0.01f || Mathf.Abs(offset.y - cubeSize.y) < 0.01f) 
        {
            offset.x = Mathf.Floor(offset.x);
            offset.z = Mathf.Floor(offset.z);
            if (Mathf.Abs(offset.y - cubeSize.y) < 0.01f)
            {
                float indexoffset = cubeSize.x * cubeSize.y * 2;
                index = (int)indexoffset + (int)offset.x * (int)cubeSize.z * 2 + (int)offset.z * 2;
            }
            else
            {
                float indexoffset = cubeSize.x * cubeSize.y * 2;
                index = (int)indexoffset + (int)offset.x * (int)cubeSize.z * 2 + (int)offset.z * 2 + 1;
            }
        }
        else
        {
            offset.y = Mathf.Floor(offset.y);
            offset.x = Mathf.Floor(offset.x);
            if (Mathf.Abs(offset.z - cubeSize.z) < 0.01f)
            {
                float indexoffset = 0;
                index = (int)indexoffset + (int)offset.x * (int)cubeSize.y * 2 + (int)offset.y * 2;
            }
            else
            {
                float indexoffset = 0;
                index = (int)indexoffset + (int)offset.x * (int)cubeSize.y * 2 + (int)offset.y * 2 + 1;
            }
        }

        if(levelGrids == null || index < 0 || index >= levelGrids.Length)
        {
            return null;
        }
        else
        {
            return levelGrids[index];
        }
    }
    protected override void SplitGrid()
    {
        if(type == GridType.SOIL)
        {
            Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
            cubeSize = vertices[1] - vertices[0] + vertices[2] - vertices[0] + vertices[1] - vertices[5];

            startPoint = transform.TransformPoint(vertices[9]);
            int gridCounts = (int)cubeSize.x * (int)cubeSize.y * 2 + (int)cubeSize.x * (int)cubeSize.z * 2 + (int)cubeSize.y * (int)cubeSize.z * 2;
            levelGrids = new LevelGrid[gridCounts];
            int gridIndex = 0;

            for (int x = 0; x < cubeSize.x; x++)
            {
                for (int y = 0; y < cubeSize.y; y++, gridIndex += 2)
                {
                    Vector3 position = startPoint + new Vector3(x, y, cubeSize.z) + new Vector3(0.5f, 0.5f, 0);
                    levelGrids[gridIndex] = new LevelGrid(position, Vector3.forward, type);
                    position.z = startPoint.z;
                    levelGrids[gridIndex + 1] = new LevelGrid(position, Vector3.back, type);
                }
            }

            for (int x = 0; x < cubeSize.x; x++)
            {
                for (int z = 0; z < cubeSize.z; z++, gridIndex += 2)
                {
                    Vector3 position = startPoint + new Vector3(x, cubeSize.y, z) + new Vector3(0.5f, 0, 0.5f);
                    levelGrids[gridIndex] = new LevelGrid(position, Vector3.up, type);
                    position.y = startPoint.y;
                    levelGrids[gridIndex + 1] = new LevelGrid(position, Vector3.down, type);
                }
            }

            for (int y = 0; y < cubeSize.y; y++)
            {
                for (int z = 0; z < cubeSize.z; z++, gridIndex += 2)
                {
                    Vector3 position = startPoint + new Vector3(cubeSize.x, y, z) + new Vector3(0, 0.5f, 0.5f);
                    levelGrids[gridIndex] = new LevelGrid(position, Vector3.right, type);
                    position.x = startPoint.x;
                    levelGrids[gridIndex + 1] = new LevelGrid(position, Vector3.left, type);
                }
            }
        }
        
    }
    
    //Lawn 相关
    private void SetupLawn()
    {
        if (type == GridType.SOIL)
        {
            lawns = new Lawn[6];
            InstantiateLawn(startPoint, cubeSize.x, cubeSize.y, Vector3.back, 0);
            InstantiateLawn(startPoint + new Vector3(0, 0, cubeSize.z), cubeSize.x, cubeSize.y, Vector3.forward, 1);
            InstantiateLawn(startPoint, cubeSize.x, cubeSize.z, Vector3.down, 2);
            InstantiateLawn(startPoint + new Vector3(0, cubeSize.y, 0), cubeSize.x, cubeSize.z, Vector3.up, 3);
            InstantiateLawn(startPoint, cubeSize.y, cubeSize.z, Vector3.left, 4);
            InstantiateLawn(startPoint + new Vector3(cubeSize.x, 0, 0), cubeSize.y, cubeSize.z, Vector3.right, 5);
        }
    }
    private void InstantiateLawn(Vector3 pos, float width, float height, Vector3 normal, int index)
    {
        GameObject lawnInstance = new GameObject($"lawn{index}", typeof(MeshRenderer), typeof(MeshFilter), typeof(Lawn));
        lawnInstance.transform.parent = transform;
        lawns[index] = lawnInstance.GetComponent<Lawn>();
        lawns[index].CreateMesh(pos, (int)width, (int)height, normal);
    }
    public void SprayWaterAtPosition(Vector3 pos, WaterColor color)
    {
        LevelGrid grid = GetGridAtPosition(pos);
        if (grid.type == GridType.SOIL)
        {
            Vector2 uv = GetUVAtPosition(grid, pos);
            Lawn lawn = GetLawnAtPosition(grid);
            if (lawn != null)
            {
                lawn.DrawAtPosition(uv, color);
            }
        }
    }

    public void FillAtPosition(Vector3 pos,WaterColor color)
    {
        LevelGrid grid = GetGridAtPosition(pos);
        if (grid.type == GridType.SOIL)
        {
            Vector2 uv = GetUVAtPosition(grid, pos);
            Lawn lawn = GetLawnAtPosition(grid);
            if (lawn != null)
            {
                lawn.FillAtPosition(uv, color);
            }
        }
    }

    public void DisableLawnAtGrid(LevelGrid grid)
    {
        Vector2 uv = GetUVAtPosition(grid, grid.position);
        Lawn lawn = GetLawnAtPosition(grid);
        lawn.ClearAtPosition(uv);
    }

    private void AddFence()
    {
        foreach(LevelGrid grid in levelGrids)
        {
            for (int i = 1; i <= 4; i++)
            {
                LevelGrid tmp = grid.GetNearGrid((NearGridDirection)i);
                if (tmp == null) continue;
                if (tmp.type != GridType.SOIL || tmp.type != GridType.WATER)
                {
                    Instantiate(fencePrefab, (grid.position + tmp.position) / 2, Quaternion.identity);
                }
            }

        }
    }

    private void UpdateLawn(LevelGrid grid)
    {
        Lawn lawn = GetLawnAtPosition(grid);
        bool[] nearGridColor = new bool[4];
        if (grid.state > 0 && !grid.colorOn)
        {
            Vector2 uv = GetUVAtPosition(grid, grid.position);
            for (int i = 1; i <= 4; i++)
            {
                LevelGrid tmp = grid.GetNearGrid((NearGridDirection)i);
                if (tmp != null && tmp.state > 0 && grid.groundColor == tmp.groundColor)
                {
                    nearGridColor[i - 1] = true;
                }
                else
                {
                    nearGridColor[i - 1] = false;
                }
            }
            lawn.DrawGridAtPosition(uv, grid.groundColor, grid.state, nearGridColor[0], nearGridColor[1], nearGridColor[2], nearGridColor[3]);
            grid.colorOn = true;
        }
        else if (grid.state == 0 && grid.colorOn)
        {
            Vector2 uv = GetUVAtPosition(grid, grid.position);
            lawn.ClearAtPosition(uv);
            grid.colorOn = false;
        }
    }
    private Vector2 GetUVAtPosition(LevelGrid grid, Vector3 pos)
    {
        if (grid == null)
        {
            return new Vector2(0, 0);
        }

        Vector3 offset = pos - startPoint;
        offset = new Vector3(offset.x / cubeSize.x, offset.y / cubeSize.y, offset.z / cubeSize.z);
        if (grid.direction == Vector3.back || grid.direction == Vector3.forward)
        {
            return new Vector2(offset.x, offset.y);
        }
        else if (grid.direction == Vector3.down || grid.direction == Vector3.up)
        {
            return new Vector2(offset.x, offset.z);
        }
        else
        {
            return new Vector2(offset.y, offset.z);
        }
    }
    private Lawn GetLawnAtPosition(LevelGrid grid)
    {
        if (grid == null || lawns == null)
        {
            return null;
        }

        if (grid.direction == Vector3.back)
        {
            return lawns[0];
        }
        else if (grid.direction == Vector3.forward)
        {
            return lawns[1];
        }
        else if (grid.direction == Vector3.down)
        {
            return lawns[2];
        }
        else if (grid.direction == Vector3.up)
        {
            return lawns[3];
        }
        else if (grid.direction == Vector3.left)
        {
            return lawns[4];
        }
        else
        {
            return lawns[5];
        }
    }
    private void InitializeType()
    {
        switch (GetComponent<MeshRenderer>().material.name)
        {
            case "GroundMat (Instance)":
                {
                    type = GridType.GROUND;
                    gameObject.layer = 9;
                    break;
                }
            case "SoilMat (Instance)":
                {
                    type = GridType.SOIL;
                    gameObject.layer = 9;
                    break;
                }
            case "GlassMat (Instance)":
                {
                    type = GridType.GLASS;
                    gameObject.layer = 10;
                    break;
                }
        }
    }
}
