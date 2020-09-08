using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    //枪支属性
    public float waterGunRange = 100f;
    public WaterColor waterColor { get; private set; }
    private List<WaterColor> enabledWaterColors;
    private int currentColor;

    //控制相关
    private LevelGrid selectingGrid;
    private LevelCube selectingCube;
    private Vector3 targetPoint;
    private int mouseButtonDown = 0;
    //public ParticleSystem waterParticle;

    //UI
    public GameObject highlight;
    public GameObject fs_small;
    public GameObject fs_big;

    //Debug
    public Text debugText;
    public bool gizmoEnabled = false;
    public bool debugTextEnabled = false;

    //游戏逻辑
    public bool hasGun;
    private EventTrigger trigger;
    private MeshRenderer renderer;

    //public ParticleSystem[] particles = new ParticleSystem[2];



    void Awake()
    {
        enabledWaterColors = new List<WaterColor>();
        currentColor = 0;
        renderer = transform.GetComponentInChildren<MeshRenderer>();
    }


    public void SetGun(bool b)
    {
        if(hasGun != b)
        {
            hasGun = b;
            highlight.SetActive(b);
            selectingGrid = null;
            selectingCube = null;
            renderer.enabled = b;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //鼠标输入判断
        switch (mouseButtonDown)
        {
            case 0://没有按下任何键
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        mouseButtonDown = 1;
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        if (mouseButtonDown == 0)
                        {
                            mouseButtonDown = 2;
                        }
                        else
                        {
                            mouseButtonDown = 0;
                        }
                    }

                    if (mouseButtonDown == 0)
                    {
                        if (hasGun)
                        {
                            if (Input.GetAxis("Mouse ScrollWheel") < 0) //鼠标后滚
                            {
                                //如果此时已经是最后一个颜色，那么就从第一个开始重新后滚（形成循环）
                                if (currentColor == enabledWaterColors.Count - 1)
                                {
                                    currentColor = 0;
                                }
                                else
                                {
                                    currentColor += 1;
                                }

                                waterColor = enabledWaterColors[currentColor];
                            }
                            else if (Input.GetAxis("Mouse ScrollWheel") > 0) //鼠标前滚
                            {
                                //Done:Change Water color
                                if (currentColor == 0)
                                {
                                    currentColor = enabledWaterColors.Count - 1;
                                }
                                else
                                {
                                    currentColor -= 1;
                                }

                                waterColor = enabledWaterColors[currentColor];
                            }
                        }//有枪的时候切枪
                    }
                    break;
                }
            case 1://按下左键
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        mouseButtonDown = 0;
                        if (hasGun)
                        {
                            fs_big.SetActive(true);
                            fs_small.SetActive(false);
                        }

                    }
                    else
                    {
                        if (hasGun)
                        {
                            if (selectingGrid != null)
                            {
                                selectingGrid.SprayWater(waterColor);
                                selectingCube.SprayWaterAtPosition(targetPoint, waterColor);
                            }
                            fs_big.SetActive(false);
                            fs_small.SetActive(true);
                        }
                        else
                        {

                        }
                    }
                    break;
                }
            case 2://按下右键
                {
                    if (Input.GetMouseButtonUp(1))
                    {
                        mouseButtonDown = 0;
                    }
                    else
                    {
                        if (hasGun && selectingGrid != null)
                        {
                            selectingGrid.ClearGrid();
                        }
                    }
                    break;
                }
        }

        //E键输入判断
        if (trigger != null && Input.GetKeyDown(KeyCode.E))
        {
            trigger.Interact();
        }

        //如果有枪需要找选中格子
        if (hasGun)
        {
            RaycastGrid();
            HighlightGrid();
        }

        //找选中trigger
        RaycastTrigger();

        //Debug
        if (debugTextEnabled)
        {
            if (selectingGrid != null)
            {
                debugText.text = $"Position:{selectingGrid.position}   State:{selectingGrid.state}   Luminance:{selectingGrid.luminance}   Type:{selectingGrid.type}";
                for (int i = 0; i < selectingGrid.grassStates.Length; i++)
                {
                    debugText.text += $"\ngrassState[{i}] = {selectingGrid.grassStates[i]}";
                }
                LevelGrid grid = selectingGrid.GetNearGrid(NearGridDirection.LEFT);
                if (grid != null)
                {
                    debugText.text += $"\nLEFT:{grid.position} - {grid.groundColor}";
                }
                else
                {
                    debugText.text += "\nLEFT:NULL";
                }
                grid = selectingGrid.GetNearGrid(NearGridDirection.RIGHT);
                if (grid != null)
                {
                    debugText.text += $"\nRIGHT:{grid.position} - {grid.groundColor}";
                }
                else
                {
                    debugText.text += "\nRIGHT:NULL";
                }
                grid = selectingGrid.GetNearGrid(NearGridDirection.FORWARD);
                if (grid != null)
                {
                    debugText.text += $"\nFORWARD:{grid.position} - {grid.groundColor}";
                }
                else
                {
                    debugText.text += "\nFORWARD:NULL";
                }
                grid = selectingGrid.GetNearGrid(NearGridDirection.BACK);
                if (grid != null)
                {
                    debugText.text += $"\nBACK:{grid.position} - {grid.groundColor}";
                }
                else
                {
                    debugText.text += "\nBACK:NULL";
                }

            }
            else
            {
                debugText.text = "NULL";
            }


        }

    }

    private void RaycastTrigger()
    {
        int layerMask = 1 << 11;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            EventTrigger tmp = hit.collider.transform.parent.GetComponent<EventTrigger>();
            if (tmp != null)
            {
                if (trigger != null && tmp != trigger)
                {
                    trigger.isLookedAt = false;
                }
                trigger = tmp;
                trigger.isLookedAt = true;
            }
        }
        else
        {
            if (trigger != null)
            {
                trigger.isLookedAt = false;
                trigger = null;
            }
        }
    }

    private void RaycastGrid()
    {
        int layerMask = 1 << 9 | 1 << 10;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, waterGunRange, layerMask))
        {
            targetPoint = hit.point;
            selectingCube = hit.collider.GetComponent<LevelCube>();
            if (selectingCube != null)
            {
                selectingGrid = selectingCube.GetGridAtPosition(hit.point);
            }
            else
            {
                selectingGrid = null;
            }
        }
        else
        {
            targetPoint = transform.position + transform.TransformDirection(Vector3.forward) * waterGunRange;
            selectingGrid = null;
            selectingCube = null;
        }
    }

    //Debug用
    private void OnDrawGizmos()
    {
        if (gizmoEnabled)
        {
            if (selectingGrid != null)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(selectingGrid.position, .05f);
                //Gizmos.DrawIcon(selectingGrid.position, $"{selectingGrid.position}");

                //back
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(selectingGrid.GetNearGrid(NearGridDirection.BACK).position, .05f);
                Gizmos.DrawLine(selectingGrid.raycastPos[3], selectingGrid.hitPos[3]);

                //forward
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(selectingGrid.GetNearGrid(NearGridDirection.FORWARD).position, .05f);
                Gizmos.DrawLine(selectingGrid.raycastPos[2], selectingGrid.hitPos[2]);

                //left
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(selectingGrid.GetNearGrid(NearGridDirection.LEFT).position, .05f);
                Gizmos.DrawLine(selectingGrid.raycastPos[1], selectingGrid.hitPos[1]);

                //right
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(selectingGrid.GetNearGrid(NearGridDirection.RIGHT).position, .05f);
                Gizmos.DrawLine(selectingGrid.raycastPos[0], selectingGrid.hitPos[0]);
            }
        }
    }

    private void WaterAnimation()
    {
        //waterParticle.Play();
    }

    private void HighlightGrid()
    {
        if (selectingGrid != null)
        {
            highlight.SetActive(true);
            if (selectingGrid.direction == Vector3.up)
            {
                highlight.transform.eulerAngles = new Vector3(0, 0, 0);
            }
            else if (selectingGrid.direction == Vector3.down)
            {
                highlight.transform.eulerAngles = new Vector3(180, 0, 0);
            }
            else if (selectingGrid.direction == Vector3.forward)
            {
                highlight.transform.eulerAngles = new Vector3(90, 0, 0);
            }
            else if (selectingGrid.direction == Vector3.back)
            {
                highlight.transform.eulerAngles = new Vector3(-90, 0, 0);
            }
            else if (selectingGrid.direction == Vector3.left)
            {
                highlight.transform.eulerAngles = new Vector3(0, 0, -90);
            }
            else if (selectingGrid.direction == Vector3.right)
            {
                highlight.transform.eulerAngles = new Vector3(90, 0, 90);
            }
            highlight.transform.position = selectingGrid.position;
            if (selectingGrid.type == GridType.SOIL || selectingGrid.type == GridType.GROUND)
            {
                if (waterColor == WaterColor.BLUE)
                    highlight.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/SceneMats/Water/BlueWaterMat");
                else
                    highlight.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/SceneMats/Water/RedWaterMat");
            }
            else if (selectingGrid.type == GridType.GLASS)
            {
                highlight.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/SceneMats/Water/GrayMat");
            }
        }
        else highlight.SetActive(false);

    }
    public WaterColor getWaterColor()
    {
        return waterColor;
    }

    public void AddColor(WaterColor color)
    {
        enabledWaterColors.Add(color);
        waterColor = enabledWaterColors[currentColor];
    }
}

