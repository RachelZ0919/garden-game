//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//abstract public class SprayableObject : MonoBehaviour
//{
//    protected LevelGrid[] levelGrids;
//    protected bool needVisualUpdate = false;
//    public bool gizmoEnabled = false;

//    protected void FindNearGrids()
//    {
//        if(levelGrids != null)
//        {
//            foreach (LevelGrid grid in levelGrids)
//            {
//                FindNearGridOnDirection(grid, NearGridDirection.RIGHT);
//                FindNearGridOnDirection(grid, NearGridDirection.LEFT);
//                FindNearGridOnDirection(grid, NearGridDirection.FORWARD);
//                FindNearGridOnDirection(grid, NearGridDirection.BACK);
//            }
//        }

//    }
//    abstract protected void SplitGrid();
//    abstract public LevelGrid GetGridAtPosition(Vector3 position);
//    abstract public void SprayAtPosition(Vector3 pos, WaterColor color); //每种sprayable object可以根据player的染色输入产生不同反馈
//    private void OnDrawGizmos()
//    {
//        if (gizmoEnabled)
//        {
//            foreach (LevelGrid grid in levelGrids)
//            {
//                if (grid.state != 0)
//                {
//                    if (grid.groundColor == WaterColor.BLUE)
//                    {
//                        Gizmos.color = Color.blue;
//                    }
//                    else
//                    {
//                        Gizmos.color = Color.red;
//                    }
//                }
//                if (grid.state == 1)
//                {
//                    Gizmos.color = Color.Lerp(Gizmos.color, Color.white, 0.8f);
//                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, grid.direction);
//                    Gizmos.DrawCube(grid.position + grid.direction * 0.03f, rotation * new Vector3(0.3f, 0.05f, 0.3f));
//                }
//                else if (grid.state == 2)
//                {
//                    Gizmos.color = Color.Lerp(Gizmos.color, Color.white, 0.8f);
//                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, grid.direction);
//                    Gizmos.DrawCube(grid.position + grid.direction * 0.06f, rotation * new Vector3(1, 0.1f, 1));
//                }
//                else if (grid.state == 3)
//                {
//                    Quaternion rotation = Quaternion.FromToRotation(Vector3.up, grid.direction);
//                    Gizmos.DrawCube(grid.position + grid.direction * 0.22f, rotation * new Vector3(1, 0.4f, 1));
//                }
//            }
//        }
//    }
//    private void FindNearGridOnDirection(LevelGrid grid, NearGridDirection dir)
//    {
//        int layerMask = 1 << 9 | 1 << 10;
//        RaycastHit hit;
//        Vector3 raycastPos = grid.position + 0.5f * grid.direction;
//        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, grid.direction);
//        Vector3 direction = Vector3.back;
//        switch (dir)
//        {
//            case NearGridDirection.BACK:
//                {
//                    direction = Vector3.back;
//                    break;
//                }
//            case NearGridDirection.FORWARD:
//                {
//                    direction = Vector3.forward;
//                    break;
//                }
//            case NearGridDirection.LEFT:
//                {
//                    direction = Vector3.left;
//                    break;
//                }
//            case NearGridDirection.RIGHT:
//                {
//                    direction = Vector3.right;
//                    break;
//                }
//        }

//        if (Physics.Raycast(raycastPos, rotation * direction, out hit, 1f, layerMask))
//        {
//            SprayableObject cube = hit.collider.GetComponent<SprayableObject>();
//            grid.SetNearGrid(cube.GetGridAtPosition(hit.point), dir);
//            //grid.setDebugInfo(raycastPos, hit.point, dir);
//        }
//        else if (Physics.Raycast(raycastPos, grid.position + rotation * direction - raycastPos, out hit, 2f, layerMask))
//        {
//            SprayableObject cube = hit.collider.GetComponent<SprayableObject>();
//            grid.SetNearGrid(cube.GetGridAtPosition(hit.point), dir);
//            //grid.setDebugInfo(raycastPos, hit.point, dir);
//        }
//        else if (Physics.Raycast(raycastPos - grid.direction + rotation * direction, rotation * ((-1) * direction), out hit, 1f, layerMask))
//        {
//            SprayableObject cube = hit.collider.GetComponent<SprayableObject>();
//            grid.SetNearGrid(cube.GetGridAtPosition(hit.point), dir);
//            //grid.setDebugInfo(raycastPos - grid.direction + rotation * direction, hit.point, dir);
//        }
//    }
//    public void ResetScanGrid()
//    {
//        if (levelGrids != null) {
//            foreach (LevelGrid grid in levelGrids)
//            {
//                grid.haveScanned = false;
//            }
//        }
//    }
//    //protected void LightDetect()
//    //{
//    //    //TODO:光照检测，确定每个LevelGrid的luminance
//    //    Quaternion lightDir = GameObject.Find("Directional Light").GetComponent<Transform>().rotation;
//    //    Vector3 direction = lightDir * Vector3.back;
        
//    //    foreach (LevelGrid grid in levelGrids)
//    //    {
//    //        //判断是否光线是否被遮挡
//    //        int layerMask = 1 << 9;
//    //        RaycastHit hit;
//    //        Vector3 raycastPos = grid.position;
//    //        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, grid.direction);

//    //        //中心点和四个顶点向光源做raycast
//    //        if (Physics.Raycast(raycastPos, direction, out hit, 100f, layerMask))
//    //        {
//    //            grid.luminance = 0;
//    //        }
//    //        else if (Physics.Raycast(raycastPos + 0.4f * (rotation * Vector3.back + rotation * Vector3.left), direction, out hit, 100f, layerMask))
//    //        {
//    //            grid.luminance = 0;
//    //        }
//    //        else if (Physics.Raycast(raycastPos + 0.4f * (rotation * Vector3.back + rotation * Vector3.right), direction, out hit, 100f, layerMask))
//    //        {
//    //            grid.luminance = 0;
//    //        }
//    //        else if (Physics.Raycast(raycastPos + 0.4f * (rotation * Vector3.forward + rotation * Vector3.left), direction, out hit, 100f, layerMask))
//    //        {
//    //            grid.luminance = 0;
//    //        }
//    //        else if (Physics.Raycast(raycastPos + 0.4f * (rotation * Vector3.forward + rotation * Vector3.right), direction, out hit, 100f, layerMask))
//    //        {
//    //            grid.luminance = 0;
//    //        }

//    //        //检测面在同一个平面上不视为遮挡
//    //        if (grid.luminance == 0) {
//    //            SprayableObject cube = hit.collider.GetComponent<SprayableObject>();
//    //            LevelGrid tmpGrid = cube.GetGridAtPosition(hit.point);
//    //            if (tmpGrid != null)
//    //            {
//    //                if (tmpGrid.direction == grid.direction && Vector3.Dot(hit.point, grid.direction) == Vector3.Dot(raycastPos, grid.direction))
//    //                {
//    //                    grid.luminance = 1;
//    //                }
//    //            }
//    //        }

//    //        //点积法线和光照方向计算光照强度
//    //        if (grid.luminance == 1) {
//    //            double intensity = (Vector3.Dot(grid.direction, (lightDir * Vector3.back)) * 0.5f) + 0.5f;
//    //            // Debug.Log(intensity);
//    //            if(intensity > 0.5){
//    //                grid.luminance = 2;
//    //            }
//    //        }
//    //    }
//    //}
//    public void VisualUpdateRequest()
//    {
//        needVisualUpdate = true;
//    }
//}
