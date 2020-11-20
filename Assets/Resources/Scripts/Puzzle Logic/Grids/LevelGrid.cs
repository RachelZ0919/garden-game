using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public enum Direction
{
    FORWARD = 1,
    LEFT = 2,
    BACK = 3,
    RIGHT = 4,
    UP = 5,
    DOWN = 6
}

public enum WaterColor
{
    RED = 1,
    BLUE = 2,
    DEFAULT = 3
}

public enum GridType
{
    NORMAL = 1,
    START = 2,
    END = 3,
    NOTSPRAYABLE = 4
}

public class LevelGrid : MonoBehaviour
{
    #region Variables

    #region Location Info
    public Vector3 position { private set { position = value; } get { return transform.position; } } //位置
    [SerializeField]
    private LevelGrid[] nearGrids = new LevelGrid[4]; //邻接格子
    public LevelGrid coveringGrid;
    public PuzzleBlock block; //所在地块
    public Direction direction;//新朝向
    public int gridStatus = 0x0000;//前2位空，一位表示Sprayable,五位表示是否连接，后8位表示高度
    public int objectStatus = 0x00;//前3位空，五位表示是否连接
    #endregion

    #region Gameplay Data
    //地块属性
    public int state { get { if (state < 0 || state > 2) return 0; else return state; } set { state = value; } } // 0是没有被染色，1是被染色，2是染色且激活
    public GridType type; //地面类型，包括地面、玻璃、起点、终点
    public WaterColor groundColor; //地面颜色，包括蓝色、红色
    //其他
    public string objectName
    {
        get
        {
            if(type == GridType.START)
            {
                return start.startPointName;
            }else if(type == GridType.END)
            {
                return end.endPointName;
            }
            else
            {
                return "none";
            }
        }
    }
    public StartPoint start;
    public EndPoint end;
    
    //显示用
    public bool colorOn { get; set; }
    //记录遍历
    public bool haveScanned { get; set; }
    #endregion

    #region Visual Setting
    public string frameFilePath;
    public string planeFilePath;
    public string objectFilePath;
    public string objectBaseFilePath;
    public string defaultModelPath;
    [HideInInspector]
    public int updateStatus = 0x000;//前8位表示是否双向联通，3表示不care，0表示不通，1表示通，后四位表示是否要更新
    [HideInInspector]
    public int updateObjectStatus = 0x000;//同上
    #endregion

    #region Fake
    private struct FakeConnectionInfo
    {
        public Direction dir;
        public LevelGrid grid;
        public float eventAngle;
        public FakeConnectionInfo(Direction d, LevelGrid g, float a)
        {
            dir = d;
            grid = g;
            eventAngle = a;
        }
    }

    private int fakeGridStatus = 0x00;
    private List<FakeConnectionInfo> fakeConnections = new List<FakeConnectionInfo>();

    #endregion

    #region Debug
    public Vector3[] raycastPos = new Vector3[4];
    public Vector3[] hitPos = new Vector3[4];
    #endregion

    #endregion

    #region Location Setting Functions
    public void SetCoveringGrid(LevelGrid grid)
    {
        Debug.Log("Set Grid");
        coveringGrid = grid;
        updateStatus |= 0x00F;//all need to update
    }

    public void SetNearGrid(LevelGrid near, Direction dir)
    {
        nearGrids[(int)dir - 1] = near;
        //SetUpdateDir(dir);
    }

    public LevelGrid GetNearGrid(Direction dir)//获取对应方向的LevelGrid
    {
        return nearGrids[(int)dir - 1];
    }

    public void SetSprayable(bool b)
    {
        if(b != isSprayable())
        {
            updateStatus |= 0x00F;// all need to update
            if (b)
            {
                type = GridType.NORMAL;
                gridStatus |= 0x2000;
            }
            else
            {
                type = GridType.NOTSPRAYABLE;
                gridStatus &= 0x1FFF;
            }
        }
    }

    public bool isSprayable()
    {
        if ((gridStatus >> 13 & 1) == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void SetConnection(Direction dir, bool b)
    {
        if(b != IsConnecting(dir))
        {
            SetUpdateDir(dir);
            
            if(type == GridType.START || type == GridType.END)
            {
                SetObjectUpdateDir(dir);
            }

            if (b)
            {
                gridStatus |= (1 << (8 + 4 - (int)dir));
                if (dir == Direction.RIGHT)
                {
                    gridStatus |= 0x1000;
                }
            }
            else
            {
                gridStatus &= ~(1 << (8 + 4 - (int)dir));
                if (dir == Direction.RIGHT)
                {
                    gridStatus &= 0x2FFF;
                }
            }
        }
    }

    public void SetHeight(Direction dir,int height)
    {
        if(height != GetHeight(dir))
        {
            SetUpdateDir(dir);
            int pos = (4 - (int)dir) * 2;
            gridStatus &= ~(3 << pos);
            gridStatus |= height << pos;
        }
    }

    public int AddHeight(Direction dir, int dh)
    {
        //int debug = gridStatus;
        SetUpdateDir(dir);
        int d = Mathf.Abs(dh);
        if(dh > 0)
        {
            if(GetHeight(dir) + d <= 2)
            {
                gridStatus += d << ((4 - (int)dir) * 2);
            }
        }
        else
        {
            if(GetHeight(dir) - d >= 0)
            {
                gridStatus -= d << ((4 - (int)dir) * 2);
            }
        }
        return GetHeight(dir);
    }

    public int GetHeight(Direction dir)
    {
        int ret = gridStatus >> ((4 - (int)dir) * 2) & 0x3;
        return ret;
    }

    public bool IsConnecting(Direction dir)
    {
        int connectionStatus = gridStatus >> (8 + 4 - (int)dir) & 0x1;
        if(connectionStatus == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void SetUpdateDir(Direction dir)
    {
        if(dir != Direction.RIGHT)
        {
            updateStatus |= 0x3 << (3 - (int)dir);
        }
        else
        {
            updateStatus |= 0x8;
            updateStatus |= 0x1;
        }
    }

    private void SetObjectUpdateDir(Direction dir)
    {
        if (dir != Direction.RIGHT)
        {
            updateObjectStatus |= 0x3 << (3 - (int)dir);
        }
        else
        {
            updateObjectStatus |= 0x8;
            updateObjectStatus |= 0x1;
        }
    }
    #endregion

    #region Gameplay Setting Function

    public void SetType(GridType t)
    {
        if(type == GridType.NOTSPRAYABLE)
        {
            block.SetSprayable(false, direction);
        }

        type = t;
    }

    public void SetStartPoint(GameObject obj, string objPath, string basePath)
    {
        //Get Transform
        Transform objTransform = transform.Find("Object");

        //Instantiate and Initialize
        start = Instantiate(obj, objTransform).GetComponent<StartPoint>();
        SetType(GridType.START);
        start.objectPath = objPath;
        objectFilePath = objPath;
        objectBaseFilePath = basePath;

        //Set Update
        updateObjectStatus |= 0xF;
    }

    public void SetEndPoint(GameObject obj, string objPath, string basePath)
    {
        //Get Transform
        Transform objTransform = transform.Find("Object");

        //Instantiate and Initialize
        end = Instantiate(obj, objTransform).GetComponent<EndPoint>();
        SetType(GridType.END);
        end.objectPath = objPath;
        objectFilePath = objPath;
        objectBaseFilePath = basePath;

        //Set Update
        updateObjectStatus |= 0xF;
    }

    public void RemoveObject()
    {
        if(type == GridType.START || type == GridType.END)
        {
            //Delete the Object
            Transform objectTransform = transform.Find("Object");
            if (objectTransform.childCount > 0)
            {
                DestroyImmediate(objectTransform.GetChild(0).gameObject);
            }

            //set all
            start = null;
            end = null;
            objectBaseFilePath = "";
            objectFilePath = "";

            //set update
            updateObjectStatus |= 0x00F;
        }
    }

    public string GetObjectType()
    {
        if(type == GridType.START)
        {
            return start.startPointName;
        }else if(type == GridType.END)
        {
            return end.endPointName;
        }
        else
        {
            return "none";
        }
    }

    public void SetObjectConnection(Direction dir, bool b)
    {
        if(b != GetObjectConnection(dir))
        {
            int i = (int)dir;
            //Set update status
            SetObjectUpdateDir(dir);
            //Set connection
            if (b)
            {
                objectStatus |= 1 << (4 - i);
                if (i == 4)
                {
                    objectStatus |= 0x10;
                }
            }
            else
            {
                objectStatus &= ~(1 << (4 - i));
                if (i == 4)
                {
                    objectStatus &= 0xEF;
                }
            }
        }
    }

    private bool GetObjectConnection(Direction dir)
    {
        int con = objectStatus >> (4 - (int)dir) & 0x1;
        if(con == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool AddFakeConnection(Direction dir, LevelGrid grid,float eventAngle)
    {
        //check if can add fake connection
        //only when the grid is not connected to the lower(=0) and higher(=2) grid, it will be reasonable to set fake connection
        int height = gridStatus >> (8 - (int)dir * 2) & 3;
        int isConnecting = gridStatus >> (12 - (int)dir) & 1;
        if(isConnecting == 1)
        {
            if(height != 1)
            {
                return false;
            }
        }

        //Change fakegridstatus and updatestatus
        FakeConnectionInfo connectionInfo = new FakeConnectionInfo(dir, grid, eventAngle);
        fakeGridStatus |= 1 << (4 - (int)dir);
        if(dir == Direction.RIGHT)
        {
            fakeGridStatus |= 0x10;
        }
        SetUpdateDir(dir);

        //Add fake connection
        fakeConnections.Add(connectionInfo);
        return true;
    }

    public void DeleteFakeConnection(int index)
    {
        //Check if the status need to change
        Direction dir = fakeConnections[index].dir;
        bool needChangeStatus = true;
        for(int i = 0; i < fakeConnections.Count; i++)
        {
            if(dir == fakeConnections[i].dir)
            {
                if(i == index)
                {
                    continue;
                }
                needChangeStatus = false;
            }
   
        }

        //Change status
        if (needChangeStatus)
        {
            fakeGridStatus &= ~(1 << (4 - (int)dir));
            if(dir == Direction.RIGHT)
            {
                fakeGridStatus &= 0xEF;
            }
            SetUpdateDir(dir);
        }

        //Remove the fake connection
        fakeConnections.RemoveAt(index);
    }

    public void ClearFakeConnection()
    {
        fakeConnections.Clear();
        fakeGridStatus = 0x00;
        updateObjectStatus |= 0x00F;
    }

    #endregion

    #region Visual Update Function

    public void UpdateAllVisual(int count)
    {
        UpdateVisual();
        if (count > 0) 
        {
            count -= 1;
            for (int i = 1; i <= 4; i++)
            {
                LevelGrid tmp = GetNearGrid((Direction)i); 
                if(tmp != null) tmp.UpdateAllVisual(count);
            }
        }
    }

    public void UpdateVisual()
    {
        Transform frameTransform = transform.Find("Frame");
        Transform planeTransform = transform.Find("Plane");
        Transform objectTransform = transform.Find("Object");

        bool debugInfoShow = true;

        for (int i = 0; i < 4; i++) 
        {
            //Get Info
            int hasChanged = (updateStatus >> (3 - i)) & 1;
            int objectHasChanged = (updateObjectStatus >> (3 - i)) & 1;
            int connectionStatus = gridStatus >> (8 + 3 - i) & 3 | fakeGridStatus >> (3 - i) & 3;
            int objectConnectionStatus = objectStatus >> (3 - i) & 3;
            int probeStatus = GetProbeCode(i);
            int objectProbeStatus = GetObjectProbeCode(i);

            //Set bool
            bool needUpdateFrame = false;
            bool needUpdatePlane = false;
            bool needUpdateObject = false;

            //Check Need Change
            if (hasChanged == 0)
            {
                if (connectionStatus == 3)
                {
                    int oldProbeStatus = (updateStatus >> (2 + 8 - i * 2)) & 0x3;
                    if (probeStatus != oldProbeStatus)
                    {
                        needUpdateFrame = true;
                        needUpdatePlane = true;
                        needUpdateObject = true;
                    }
                }
            }
            else
            {
                needUpdatePlane = true;
                needUpdateFrame = true;
            }

            if(objectHasChanged == 0)
            {
                if(objectConnectionStatus == 3)
                {
                    int oldProbeStatus = (updateObjectStatus >> (2 + 8 - i * 2) & 3);
                    if(objectProbeStatus != oldProbeStatus)
                    {
                        needUpdatePlane = true;
                        needUpdateObject = true;
                    }
                }
            }
            else
            {
                needUpdatePlane = true;
                needUpdateObject = true;
            }

            updateStatus &= ~(3 << (2 + 8 - i * 2));
            updateStatus |= probeStatus << (2 + 8 - i * 2);
            updateObjectStatus &= ~(3 << (2 + 8 - i * 2));
            updateObjectStatus |= objectProbeStatus << (2 + 8 - i * 2);

            if (!needUpdateFrame && !needUpdatePlane && !needUpdateObject)
            {
                continue;
            }
            else
            {
                if (debugInfoShow)
                {
                    //Debug.Log($"Update Visual {transform.parent.name} - {direction} - " +
                        //$"{Convert.ToString(updateStatus, 16).PadLeft(3, '0')} - {Convert.ToString(updateObjectStatus, 16).PadLeft(3, '0')}");
                    debugInfoShow = false;
                }
            }

            //Get Codes
            int visualCode = GetVisualCode(i, connectionStatus, probeStatus);
            int extraCode = GetExtraCode(i, objectConnectionStatus, objectProbeStatus);
            int code = (extraCode << 8) | visualCode;

            if(visualCode == -1)
            {
                Debug.Log($"--- {i}, code is none");
            }
            else
            {
                Debug.Log($"--- {i}, code is " +
                    $"{Convert.ToString(extraCode, 16)} + {Convert.ToString(visualCode, 16).PadLeft(2, '0')} = {Convert.ToString(code, 16).PadLeft(3, '0')}");
            }

            if (needUpdateFrame)
            {
                //Get Transform And Delete Old
                Transform frameChildTransform = frameTransform.Find($"{i}");
                if (frameChildTransform.childCount > 0)
                {
                    DestroyImmediate(frameChildTransform.GetChild(0).gameObject);
                }

                //If Covered, End
                if (code != -1)
                {
                    GameObject fob = GetFrameObject(visualCode);
                    if (fob != null)
                    {
                        Instantiate(fob, frameChildTransform);
                    }
                }
            }

            if (needUpdatePlane)
            {
                //Get Transform and Delete Old
                Transform planeChildTransform = planeTransform.Find($"{i}");
                if (planeChildTransform.childCount > 0)
                {
                    DestroyImmediate(planeChildTransform.GetChild(0).gameObject);
                }

                //If Covered, End
                if (code != -1)
                {
                    GameObject pob = GetPlaneObject(code);
                    if (pob != null)
                    {
                        Instantiate(pob, planeChildTransform);
                    }
                }
            }

            if (needUpdateObject)
            {
                if(type == GridType.START)
                {
                    start.UpdateVisual(i, code);
                }else if(type == GridType.END)
                {
                    end.UpdateVisual(i, code);
                }
            }         
        }

        //Clear Update
        updateStatus &= 0xFF0;
        updateObjectStatus &= 0xFF0;
    }

    private int GetVisualCode(int num, int connectionStatus, int probeStatus)
    {
        //Down Three are special conditions
        if (coveringGrid != null)//it is covered
        {
            return -1;
        }

        if (!isSprayable())//it is not sprayable
        {
            return 0x00;
        }

        if (connectionStatus == 0)//it is connecting to nothing at all(probably removed in the future because it is actually not a special condition)
        {
            return 0xCF;
        }

        int code, height;
        code = (probeStatus << 2) | connectionStatus;
        if (num != 0)//Get the Height Code
        {
            height = gridStatus >> ((3 - num) * 2) & 0xF;
        }
        else
        {
            height = (gridStatus << 2 & 0xC) | (gridStatus >> 6 & 0x3);
        }

        return (code << 4) | height;
    }

    private int GetExtraCode(int num, int connectionStatus, int probeCode)
    {
        if (type == GridType.START)
        {
            return probeCode << 2 | connectionStatus;
        }
        else
        {
            return 0;
        }
    }

    private int GetProbeCode(int num)
    {
        int connectionStatus = gridStatus >> (8 + 3 - num) & 3;
        if(connectionStatus == 3)
        {
            Direction d1, d2;
            if (num == 0)
            {
                d1 = Direction.RIGHT;
                d2 = Direction.FORWARD;
            }
            else
            {
                d1 = (Direction)num;
                d2 = (Direction)(num + 1);
            }

            if (block.Probe(direction, d1, d2))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 3;
        }
    }

    private int GetObjectProbeCode(int num)
    {
        string objectType = GetObjectType();
        if(objectType != "none")
        {
            int connectionStatus = objectStatus >> (3 - num) & 3;
            if (connectionStatus == 3)
            {
                Direction d1, d2;
                if (num == 0)
                {
                    d1 = Direction.RIGHT;
                    d2 = Direction.FORWARD;
                }
                else
                {
                    d1 = (Direction)num;
                    d2 = (Direction)(num + 1);
                }

                if (block.ProbeForObject(direction, d1, d2, GetObjectType()))
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 3;
            }
        }
        else
        {
            return 0;
        }
    }

    private GameObject GetPlaneObject(int code)
    {
        GameObject obj = null;
        switch(type)
        {
            case GridType.NOTSPRAYABLE:
                {
                    obj = Resources.Load<GameObject>(defaultModelPath + "00");
                    if(obj == null)
                    {
                        Debug.LogError($"Load Default Failed:{defaultModelPath + "00"}");
                    }
                    break;
                }
            case GridType.NORMAL:
                {
                    obj = ItemLoader.LoadObject(planeFilePath, code);
                    if (obj == null)
                    {
                        Debug.LogError($"Load Normal Failed:{planeFilePath}{code.ToString("X")}");
                    }
                    break;
                }
            case GridType.START:
                {
                    obj = ItemLoader.LoadObject(objectBaseFilePath, code);
                    if (obj == null)
                    {
                        Debug.LogError($"Load Start Failed:{objectBaseFilePath}{code.ToString("X")}");
                    }
                    break;
                }
            case GridType.END:
                {
                    obj = ItemLoader.LoadObject(objectBaseFilePath, code);
                    if (obj == null)
                    {
                        Debug.LogError($"Load End Failed:{objectBaseFilePath}{code.ToString("X")}");
                    }
                    break;
                }
        }
        return obj;
    }

    private GameObject GetFrameObject(int connectionStatus)
    {
        return ItemLoader.LoadObject(frameFilePath, connectionStatus);
    }

    #endregion

    #region Gameplay Logic Function
    //给方块染色
    public void SprayWater(WaterColor waterColor)
    {
        //不是可染色的地面或已染色
        if (type != GridType.NORMAL || (state > 0 && groundColor == waterColor)) return;

        GameManager.VisualUpdate();
        colorOn = false;

        //更新原连通区域
        ClearGrid();

        //更新地块颜色
        groundColor = waterColor;
        state = 1;
        //判断现连通区域
        int count = 0;
        bool is_start = false, is_active = false;
        LevelGrid tmp;
        for (int i = 1; i <= 4; i++)
        {
            tmp = GetNearGrid((Direction)i);
            if (tmp == null) continue;
            if (tmp.state > 0 && (tmp.groundColor == groundColor || tmp.groundColor == WaterColor.DEFAULT))
            {
                count++;
                if (tmp.state == 1) continue;
                if (tmp.state == 2) is_start = true;
                else if (tmp.state == 3) is_active = true;
            }
        }

        if (count <= 0) //周围无同色地块
        {
            state = 1;
            return;
        }
        if (is_start == true)
        {
            if (count == 1) //只有一个与起点联通的同色地块
            {
                state = 2;
            }
            else
            {
                GameManager.ResetScanGrid();
                ScanGrids();
            }
        }
        else
        {   //四周地块的状态只有1或3
            if (is_active == true)
                ChangeState(3);
            else
            {
                state = 1;
            }
        }
    }

    public void ClearGrid()
    {
        //不是可更改染色状态的地面或原本无色
        if (type != GridType.NORMAL || state == 0) return;
        GameManager.VisualUpdate();


        state = 0;  //更新地块状态

        GameManager.ResetScanGrid();
        haveScanned = true;
        for (int i = 1; i <= 4; i++)
        {
            LevelGrid tmp = GetNearGrid((Direction)i);
            if (tmp == null) continue;
            if (tmp.state > 0 && (tmp.groundColor == groundColor || tmp.groundColor == WaterColor.DEFAULT))
            {
                tmp.ScanGrids();//更新连通区域
            }
        }
    }

    //遍历场景
    private void ScanGrids()
    {
        if (haveScanned == true) return;//如果当前点已经判断过了就直接return（针对cleargrid）
        int s_num = 0, e_num = 0;
        Count_s_e(ref s_num, ref e_num, false);

        if (s_num <= 0) //没有与起点联通
            ChangeState(1);
        else if (s_num <= e_num)    //联通的起点少于终点--激活
            ChangeState(3);
        else
            ChangeState(2);     //联通的终点少于起点--未激活
    }

    //遍历并获取当前连通区域内的起点和终点数量
    private void Count_s_e(ref int s_num, ref int e_num, bool last_is_start)
    {
        bool is_start = false;
        if (type == GridType.END)
        {
            if (last_is_start == true) return;    //起点之间不联通
            is_start = true;
            s_num++;
        }
        else if (type == GridType.START) e_num++;
        haveScanned = true;    //已经遍历过的标记
        for (int i = 1; i <= 4; i++)
        {
            LevelGrid tmp = GetNearGrid((Direction)i);
            if (tmp == null) continue;
            if (tmp.state > 0 && tmp.haveScanned == false && (tmp.groundColor == groundColor || tmp.groundColor == WaterColor.DEFAULT))
            {
                tmp.Count_s_e(ref s_num, ref e_num, is_start);
            }
        }
    }

    //遍历并改变地块状态
    private void ChangeState(int new_state)
    {
        if (state != new_state)
        {
            colorOn = false;
            state = new_state;
        }

        bool is_start = false;
        if (type == GridType.END)
        {
            is_start = true;
            if (new_state <= 2)
            {
                state = 2;  //起点状态只能是2or3
                //seed.Deactivate();
            }
            else //new_state == 3
            {
                //seed.Activate();
            }
        }
        else
        {
            if (new_state < 1 && type == GridType.START) state = 1;    //终点状态只能是1or2or3
            LevelGrid tmp;
            for (int i = 1; i <= 4; i++)
            {
                tmp = GetNearGrid((Direction)i);
                if (tmp == null) continue;
                if (tmp.state > 0 && (tmp.groundColor == groundColor || tmp.groundColor == WaterColor.DEFAULT))
                {
                    if (is_start == true && tmp.type == GridType.END) continue;    //起点之间不联通
                    tmp.ChangeState(new_state);
                }
            }
        }


    }

    #endregion

    #region Debug
    public void setDebugInfo(Vector3 pos, Vector3 hit, Direction dir)
    {
        raycastPos[(int)dir - 1] = pos;
        hitPos[(int)dir - 1] = hit;
    }

    #endregion
}
