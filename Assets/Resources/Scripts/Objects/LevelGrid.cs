using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NearGridDirection
{
    RIGHT = 1,
    LEFT = 2,
    FORWARD = 3,
    BACK = 4,
}

public enum WaterColor
{
    RED = 1,
    BLUE = 2,
    DEFAULT = 3
}

public enum GridType
{
    SOIL = 1,
    SEED = 2,
    WATER = 3,
    GLASS = 4,
    GROUND = 5
}


public class LevelGrid
{
    //这是描述每个地块信息的类，它不参与游戏事件循环，所以这里尽量只存储信息。
    //位置属性
    public Vector3 position; //位置
    public Vector3 direction; //朝向
    private LevelGrid[] nearGrids = new LevelGrid[4]; //邻接格子

    //地块属性
    public int luminance = 1;//亮度,0暗，1适中，2亮
    public int state = 0; // 0是没有被染色，1是被染色，2是染色且与未激活起点连接，3是染色且与激活起点连接
    public GridType type; //地面类型，包括地面、玻璃、起点、终点
    public WaterColor groundColor = WaterColor.BLUE; //地面颜色，包括蓝色、红色

    //其他
    public StartPoint seed;
    public static int GRASSTYPE = 5;
    public int[] grassStates = new int[GRASSTYPE];
    //public List<int> grassTypes = new List<int>();//当前地块上生长的植物类型，1表示光草，2表示光菇，3表示弹力菇，4表示方块树

    //记录和哪个起点相连

    //记录遍历
    public bool haveScanned = true;

    //Debug
    public Vector3[] raycastPos = new Vector3[4];
    public Vector3[] hitPos = new Vector3[4];

    //显示用
    public bool colorOn = false;

    public LevelGrid(Vector3 p, Vector3 d, GridType t, int l = 1)
    {
        this.position = p;
        this.direction = d;
        this.type = t;
        this.luminance = l;
        for (int i = 0; i < nearGrids.Length; i++)
        {
            nearGrids[i] = null;
        }
    }

    //设置对应方向的LevelGrid
    public void SetNearGrid(LevelGrid near, NearGridDirection dir)
    {
        nearGrids[(int)dir - 1] = near;
    }

    public void setDebugInfo(Vector3 pos, Vector3 hit, NearGridDirection dir)
    {
        raycastPos[(int)dir - 1] = pos;
        hitPos[(int)dir - 1] = hit;
    }

    //获取对应方向的LevelGrid
    public LevelGrid GetNearGrid(NearGridDirection dir)
    {
        return nearGrids[(int)dir - 1];
    }

    //给方块染色
    public void SprayWater(WaterColor waterColor)
    {
        //不是可染色的地面或已染色
        if (type != GridType.SOIL || (state > 0 && groundColor == waterColor)) return;

        GameManager.VisualUpdate();
        colorOn = false;

        //更新原连通区域
        ClearGrid();

        //更新地块颜色
        groundColor = waterColor;
        state = 1;
        int[] change_state = new int[GRASSTYPE];
        //判断现连通区域
        int count = 0;
        bool is_start = false, is_active = false;
        LevelGrid tmp;
        for (int i = 1; i <= 4; i++)
        {
            tmp = GetNearGrid((NearGridDirection)i);
            if (tmp == null) continue;
            if (tmp.state > 0 && (tmp.groundColor == groundColor || tmp.groundColor == WaterColor.DEFAULT))
            {
                count++;
                if (tmp.state == 1) continue;
                for (int g = 0; g < GRASSTYPE; g++)
                {
                    if (tmp.grassStates[g] == 1) change_state[g] = 1;
                }
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
                change_state.CopyTo(grassStates, 0);
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
                ChangeState(3, change_state);
            else
            {
                state = 1;
            }
        }
    }

    public void ClearGrid()
    {
        //不是可更改染色状态的地面或原本无色
        if (type != GridType.SOIL || state == 0) return;
        GameManager.VisualUpdate();


        state = 0;  //更新地块状态
        for (int i = 0; i < GRASSTYPE; i++)
        {
            grassStates[i] = 0;
        }

        GameManager.ResetScanGrid();
        haveScanned = true;
        for (int i = 1; i <= 4; i++)
        {
            LevelGrid tmp = GetNearGrid((NearGridDirection)i);
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
        int[] s_count = new int[GRASSTYPE];
        Count_s_e(ref s_count, ref s_num, ref e_num, false);

        if (s_num <= 0) //没有与起点联通
            ChangeState(1, s_count);
        else if (s_num <= e_num)    //联通的起点少于终点--激活
            ChangeState(3, s_count);
        else
            ChangeState(2, s_count);     //联通的终点少于起点--未激活
    }

    //遍历并获取当前连通区域内的起点和终点数量
    private void Count_s_e(ref int[] s_count, ref int s_num, ref int e_num, bool last_is_start)
    {
        bool is_start = false;
        if (type == GridType.SEED)
        {
            if (last_is_start == true) return;    //起点之间不联通
            is_start = true;
            s_num++;
            s_count[(int)seed.seedType - 1] = 1;
        }
        else if (type == GridType.WATER) e_num++;
        haveScanned = true;    //已经遍历过的标记
        for (int i = 1; i <= 4; i++)
        {
            LevelGrid tmp = GetNearGrid((NearGridDirection)i);
            if (tmp == null) continue;
            if (tmp.state > 0 && tmp.haveScanned == false && (tmp.groundColor == groundColor || tmp.groundColor == WaterColor.DEFAULT))
            {
                tmp.Count_s_e(ref s_count, ref s_num, ref e_num, is_start);
            }
        }
    }

    //遍历并改变地块状态
    private void ChangeState(int new_state, int[] s_count)
    {
        bool is_finish = true;
        if (state != new_state)
        {
            colorOn = false;
            state = new_state;
            is_finish = false;
        }

        bool is_start = false;
        if (type == GridType.SEED)
        {
            is_start = true;
            if (new_state <= 2)
            {
                state = 2;  //起点状态只能是2or3
                seed.Deactivate();
            }
            else //new_state == 3
            {
                seed.Activate();
            }
        }
        else
        {
            for (int g = 0; g < GRASSTYPE; g++)
            {
                if (grassStates[g] != s_count[g])
                {
                    is_finish = false;
                    grassStates[g] = s_count[g];
                }
            }
            s_count.CopyTo(grassStates, 0);
            if (new_state < 1 && type == GridType.WATER) state = 1;    //终点状态只能是1or2or3
        }
        if (is_finish) return;

        LevelGrid tmp;
        for (int i = 1; i <= 4; i++)
        {
            tmp = GetNearGrid((NearGridDirection)i);
            if (tmp == null) continue;
            if (tmp.state > 0 && (tmp.groundColor == groundColor || tmp.groundColor == WaterColor.DEFAULT))
            {
                if (is_start == true && tmp.type == GridType.SEED) continue;    //起点之间不联通
                tmp.ChangeState(new_state, s_count);
            }
        }
    }
}
