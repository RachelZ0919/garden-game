using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PuzzleBlock : MonoBehaviour
{
    static private Vector3[] DirectionVectors = { Vector3.forward, Vector3.left, Vector3.back, Vector3.right, Vector3.up, Vector3.down };
    public LevelGrid[] grids = new LevelGrid[6];

    public bool SetType(GridType type, Direction dir)
    {
        GetGrid(dir).SetType(type);
        return true;
    }

    public bool SetStartPoint(string objPath, string basePath, Direction dir)
    {
        LevelGrid grid = GetGrid(dir);

        //check allowed to spawn
        GameObject obj = Resources.Load<GameObject>(objPath + "obj");
        if (!obj.GetComponent<StartPoint>().directionAllowedSpawn[(int)dir - 1])
        {
            return false;
        }

        //first check if need to set sprayable or remove formal object
        if (!grid.isSprayable())
        {
            SetSprayable(true, dir);
        }
        else
        {
            if (grid.type == GridType.START)
            {
                if (grid.objectBaseFilePath == basePath && grid.objectFilePath == objPath)
                {
                    return true;
                }
                else
                {
                    RemoveObject(dir);
                }
            }
            else
            {
                RemoveObject(dir);
            }
        }


        //Set new one!
        grid.SetStartPoint(obj, objPath, basePath);
        for (int i = 1; i <= 4; i++)
        {
            if (grid.IsConnecting((Direction)i))
            {
                LevelGrid nearGrid = grid.GetNearGrid((Direction)i);
                if (nearGrid.GetObjectType() == grid.GetObjectType())
                {
                    Direction relativeDir = GetRelativeDirection(dir, (Direction)i, grid.GetHeight((Direction)i));
                    grid.SetObjectConnection((Direction)i, true);
                    nearGrid.SetObjectConnection(relativeDir, true);
                    nearGrid.UpdateAllVisual(0);
                }
            }
        }
        grid.UpdateAllVisual(2);

        return true;
    }

    public bool SetEndPoint(string objPath,string basePath, Direction dir)
    {
        LevelGrid grid = GetGrid(dir);

        //check allowed to spawn
        GameObject obj = Resources.Load<GameObject>(objPath + "obj");
        if (!obj.GetComponent<EndPoint>().directionAllowedSpawn[(int)dir - 1])
        {
            return false;
        }

        //first check if need to set sprayable or remove formal object
        if (!grid.isSprayable())
        {
            SetSprayable(true, dir);
        }
        else
        {
            if (grid.type == GridType.END)
            {
                if (grid.objectBaseFilePath == basePath && grid.objectFilePath == objPath)
                {
                    return true;
                }
                else
                {
                    RemoveObject(dir);
                }
            }
            else
            {
                RemoveObject(dir);
            }
        }

        //Set new one!
        grid.SetEndPoint(obj, objPath, basePath);
        for (int i = 1; i <= 4; i++) //maybe this is not necessary
        {
            if (grid.IsConnecting((Direction)i))
            {
                LevelGrid nearGrid = grid.GetNearGrid((Direction)i);
                if (nearGrid.GetObjectType() == grid.GetObjectType())
                {
                    Direction relativeDir = GetRelativeDirection(dir, (Direction)i, grid.GetHeight((Direction)i));
                    grid.SetObjectConnection((Direction)i, true);
                    nearGrid.SetObjectConnection(relativeDir, true);
                    nearGrid.UpdateAllVisual(0);
                }
            }
        }
        grid.UpdateAllVisual(2);

        return true;
    }

    public void RemoveObject(Direction dir)
    {
        LevelGrid grid = GetGrid(dir);
        if (grid.type == GridType.START || grid.type == GridType.END) 
        {
            //set all to false
            for (int i = 1; i <= 4; i++) 
            {
                if (grid.IsConnecting((Direction)i))
                {
                    LevelGrid nearGrid = grid.GetNearGrid((Direction)i);
                    if (nearGrid.GetObjectType() == grid.GetObjectType())
                    {
                        Direction relativeDir = GetRelativeDirection(dir, (Direction)i, grid.GetHeight((Direction)i));
                        grid.SetObjectConnection((Direction)i, false);
                        nearGrid.SetObjectConnection(relativeDir, false);
                        nearGrid.UpdateAllVisual(0);
                    }
                }
            }

            //remove it!
            grid.RemoveObject();
        }
    }

    public void SetSprayable(bool b, Direction dir) //Setup or Delete Grid
    {
        LevelGrid grid = GetGrid(dir);
        if (!b)
        {
            DisconnectAll(dir);
            RemoveObject(dir);
            grid.SetSprayable(b);
            grid.UpdateAllVisual(0);
        }
        else
        {
            grid.SetSprayable(b);
            grid.UpdateAllVisual(0);
        }

    }

    public void ConnectGrid(Direction dir, Direction connectDir) //Connect New Grid
    {
        //Get Grid
        LevelGrid myGrid = GetGrid(dir);
        LevelGrid nearGrid = myGrid.GetNearGrid(connectDir);
        //Get Relative Direction
        Direction relativeDirection = GetRelativeDirection(dir, connectDir, myGrid.GetHeight(connectDir));
        //Set Undo
        Undo.RecordObject(myGrid, "Connect Grid");
        Undo.RecordObject(nearGrid, "Connect Grid");
        //Set Connection
        myGrid.SetConnection(connectDir, true);
        nearGrid.SetConnection(relativeDirection, true);
        //if it is start/end,also connect
        if(myGrid.GetObjectType() == nearGrid.GetObjectType() && myGrid.GetObjectType() != "none")
        {
            myGrid.SetObjectConnection(connectDir, true);
            nearGrid.SetObjectConnection(relativeDirection, true);
        }
        //Update Visual
        myGrid.UpdateAllVisual(2);
        nearGrid.UpdateAllVisual(2);
    }

    public void DisconnectAll(Direction dir)//Disconnect all nearby grids
    {
        for(int i = 1;i <= 4; i++)
        {
            DisconnectGrid(dir, (Direction)i);
        }
    }

    public void DisconnectGrid(Direction dir, Direction connectDir)//disconect nearby grid in one direction
    {
        //Get Grid
        LevelGrid myGrid = GetGrid(dir);
        LevelGrid nearGrid = myGrid.GetNearGrid(connectDir);
        //Get Relative Direction
        Direction relativeDirection = GetRelativeDirection(dir, connectDir, myGrid.GetHeight(connectDir));
        if (nearGrid != null)
        {
            //Disconnect
            myGrid.SetConnection(connectDir, false);
            nearGrid.SetConnection(relativeDirection, false);
            //if it is start/end, also disconnect
            if(myGrid.GetObjectType() == nearGrid.GetObjectType() && myGrid.GetObjectType() != "none")
            {
                myGrid.SetObjectConnection(connectDir, false);
                nearGrid.SetObjectConnection(relativeDirection, false);
            }
            //Update Visual
            myGrid.UpdateAllVisual(1);
            nearGrid.UpdateAllVisual(1);
        }
    }

    public void AddBlockTo(Direction dir, PuzzleBlock newBlock)//add block to one direction
    {

        LevelGrid grid = GetGrid(dir);//Get grid
        LevelGrid coveringGrid = newBlock.GetGrid(GetOpposeDirection(dir));//also get covering grid
        SetSprayable(false, dir); //This Grid is Not Sprayable Any More!
        newBlock.SetSprayable(false, GetOpposeDirection(dir)); //set covering grid to not sprayable
        grid.SetCoveringGrid(coveringGrid);
        coveringGrid.SetCoveringGrid(grid);

        for (int i = 1; i <= 4; i++)   // Change All Nearby Grid And Status
        {
            //Get near Grid and new Grid
            LevelGrid nearGrid = grid.GetNearGrid((Direction)i);
            if (nearGrid == null)//this situation only happen when processing multiple covered grid
            {
                continue;
            }

            LevelGrid newGrid = newBlock.GetSelfRelativeDirectionGrid(dir, (Direction)i);

            //Get height status
            int height = grid.GetHeight((Direction)i);
            //Get Relative Direction
            Direction nearGridRelativeDirection = GetRelativeDirection(dir, (Direction)i, height);
            Direction newGridRelativeDirection;
            //Change nearby grids status
            if (height < 2)
            {
                int h = nearGrid.AddHeight(nearGridRelativeDirection, 1); // Change near grid height Status
                newGridRelativeDirection = GetRelativeDirection(nearGrid.direction, nearGridRelativeDirection, nearGrid.GetHeight(nearGridRelativeDirection));
                newGrid.SetHeight(newGridRelativeDirection, h);//Also Change new grid height status = near grid height status
                nearGrid.SetNearGrid(newGrid, nearGridRelativeDirection);//Set new Nearby Grid
                newGrid.SetNearGrid(nearGrid, newGridRelativeDirection);// Set new Nearby Grid
            }
            else//if height = 2, it means this grid is covered too.
            {
                newGridRelativeDirection = GetRelativeDirection(nearGrid.direction, nearGridRelativeDirection, nearGrid.GetHeight(nearGridRelativeDirection));
                nearGrid.block.SetSprayable(false, nearGrid.direction);// Change status to not sprayable
                nearGrid.SetCoveringGrid(newGrid);
                nearGrid.SetNearGrid(null, nearGridRelativeDirection);//set to null
                newGrid.SetCoveringGrid(nearGrid);
                newGrid.SetNearGrid(null, newGridRelativeDirection);
            }
            //clear grid and covering grid connection
            grid.SetConnection((Direction)i, false);
            grid.SetNearGrid(null, (Direction)i);
            coveringGrid.SetNearGrid(null, (Direction)i);
            //also delete the connection of near grid
            nearGrid.SetConnection(nearGridRelativeDirection, false);
            nearGrid.UpdateAllVisual(0);
            newGrid.UpdateAllVisual(0);
        }

        grid.UpdateAllVisual(0);
        coveringGrid.UpdateAllVisual(0);
    }

    public bool Probe(Direction ori, Direction dir1, Direction dir2)
    {
        LevelGrid grid = GetGrid(ori);
        bool ret = false;
        int probeAdd; 
        
        if (ori == Direction.DOWN)
        {
            probeAdd = -1;
        }
        else
        {
            probeAdd = 1;
        }
        //d1 = smaller one, d2 = bigger one
        Direction d1, d2;
        if (LoopAdd((int)dir1, probeAdd, 1, 4) == (int)dir2)  
        {
            d1 = dir1;
            d2 = dir2;
        }
        else
        {
            d1 = dir2;
            d2 = dir1;
        }

        LevelGrid lastGrid = grid;
        LevelGrid probeGrid = grid.GetNearGrid(d1);
        LevelGrid targetGrid = grid.GetNearGrid(d2);

        Direction lastDirection = ori;
        Direction probeDirection = d1;

        

        for(int i = 0; i < 4; i++)
        {
            probeDirection = GetRelativeDirection(lastDirection, probeDirection, lastGrid.GetHeight(probeDirection));

            if(probeGrid.direction == Direction.DOWN)
            {
                probeAdd = 1;
            }
            else
            {
                probeAdd = -1;
            }

            probeDirection = (Direction)LoopAdd((int)probeDirection, probeAdd, 1, 4);
            if (!probeGrid.IsConnecting(probeDirection))
            {
                ret = false;
                break;
            }
            lastGrid = probeGrid;
            lastDirection = lastGrid.direction;
            probeGrid = lastGrid.GetNearGrid(probeDirection);
            if(probeGrid == targetGrid)
            {
                ret = true;
                break;
            }
        }
        //Debug.Log($"[Function]{name}Probe({ori},{dir1},{dir2}) = {ret}");
        return ret;
    }

    public bool ProbeForObject(Direction ori, Direction dir1, Direction dir2, string type)
    {
        LevelGrid grid = GetGrid(ori);
        bool ret = false;
        //d1 = smaller one, d2 = bigger one
        Direction d1, d2;
        if (LoopAdd((int)dir1, 1, 1, 4) == (int)dir2)
        {
            d1 = dir1;
            d2 = dir2;
        }
        else
        {
            d1 = dir2;
            d2 = dir1;
        }

        LevelGrid lastGrid = grid;
        LevelGrid probeGrid = grid.GetNearGrid(d1);
        LevelGrid targetGrid = grid.GetNearGrid(d2);

        Direction lastDirection = ori;
        Direction probeDirection = d1;

        int probeAdd = -1;
        if (grid.direction == Direction.DOWN || lastGrid.direction == Direction.DOWN)
        {
            probeAdd = 1;
        }

        for (int i = 0; i < 3; i++)
        {
            Debug.Log(i);
            probeDirection = GetRelativeDirection(lastDirection, probeDirection, lastGrid.GetHeight(probeDirection));
            probeDirection = (Direction)LoopAdd((int)probeDirection, probeAdd, 1, 4);
            if (!probeGrid.IsConnecting(probeDirection) || probeGrid.GetObjectType() != type) 
            {
                ret = false;
                break;
            }
            lastGrid = probeGrid;
            lastDirection = lastGrid.direction;
            probeGrid = lastGrid.GetNearGrid(probeDirection);
            if (probeGrid == targetGrid)
            {
                ret = true;
                break;
            }
        }
        //Debug.Log($"[Function]{name}ProbeForWater({ori},{dir1},{dir2}) = {ret}");
        return ret;
    }

    public void ClearConnection()
    {
        for (int i = 1;i <= 6; i++)
        {
            DisconnectAll((Direction)i);
            //Get selfCoveringGrid and grid
            LevelGrid grid = GetGrid((Direction)i);
            LevelGrid selfCoveringGrid = grid.coveringGrid;
            if(selfCoveringGrid != null)
            {
                for (int j = 1; j <= 4; j++) 
                {
                    LevelGrid selfNearGrid = GetSelfRelativeDirectionGrid((Direction)i, (Direction)j);
                    LevelGrid coveringGrid = selfNearGrid.coveringGrid;
                    //if there is only one covering grid, just pass
                    if(coveringGrid == null)
                    {
                        continue;
                    }

                    //first delete covering
                    selfCoveringGrid.SetCoveringGrid(null);
                    coveringGrid.SetCoveringGrid(null);

                    //Set Height and Near grid
                    Direction CoveringGridRelativeDirection = GetRelativeDirection((Direction)i, GetOpposeDirection((Direction)j), 0);
                    coveringGrid.SetNearGrid(selfCoveringGrid, CoveringGridRelativeDirection);
                    coveringGrid.SetHeight(CoveringGridRelativeDirection, 2);
                    Direction SelfCoveringGridRelativeDirection = GetRelativeDirection(coveringGrid.direction, CoveringGridRelativeDirection, 2);
                    selfCoveringGrid.SetNearGrid(coveringGrid, SelfCoveringGridRelativeDirection);
                    selfCoveringGrid.SetHeight(SelfCoveringGridRelativeDirection, 2);
                }
            }
            else
            {
                for (int j = 1; j <= 4; j++)
                {
                    //Get height Status
                    int height = grid.GetHeight((Direction)j);

                    //if self connecting, continue
                    if (height == 0)
                    {
                        continue;
                    }

                    //Get covering grid if there is one
                    LevelGrid selfNearGrid = GetSelfRelativeDirectionGrid((Direction)i, (Direction)j);
                    LevelGrid coveringGrid = selfNearGrid.coveringGrid;

                    //Get Nearby Grid
                    LevelGrid nearGrid = grid.GetNearGrid((Direction)j);
                    Direction nearGridRelativeDirection = GetRelativeDirection((Direction)i, (Direction)j, height);
                    if (coveringGrid != null)//if it is covering a grid, then this would be new nearby grid
                    {
                        coveringGrid.SetCoveringGrid(null);//first delete self
                        nearGrid.AddHeight(nearGridRelativeDirection, -1); //lower the height
                        nearGrid.SetNearGrid(coveringGrid, nearGridRelativeDirection);//Set new Near grid
                        Direction newGridRelativeDirection = GetRelativeDirection(nearGrid.direction, nearGridRelativeDirection, nearGrid.GetHeight(nearGridRelativeDirection));
                        coveringGrid.SetNearGrid(nearGrid, newGridRelativeDirection);
                        coveringGrid.SetHeight(newGridRelativeDirection, nearGrid.GetHeight(nearGridRelativeDirection));
                        Debug.Log($"Set CoveringGrid:{coveringGrid.direction} and NearbyGrid:{nearGrid.direction}, height:{nearGrid.GetHeight(nearGridRelativeDirection)}");
                    }
                    else//if there is no such grid,then there must be one connect to self nearby grid
                    {
                        LevelGrid newGrid = selfNearGrid.GetNearGrid(GetRelativeDirection((Direction)i, (Direction)j, 0));
                        nearGrid.SetHeight(nearGridRelativeDirection, 0);//set to the lowest
                        nearGrid.SetNearGrid(newGrid, nearGridRelativeDirection);
                        Direction newGridRelativeDirection = GetRelativeDirection(nearGrid.direction, nearGridRelativeDirection, 0);
                        newGrid.SetNearGrid(nearGrid, newGridRelativeDirection);
                        newGrid.SetHeight(newGridRelativeDirection, 0);
                        Debug.Log($"Set NewGrid:{newGrid.direction} and NearbyGrid:{nearGrid.direction}, height:{nearGrid.GetHeight(nearGridRelativeDirection)}");
                    }
                }
            }
        }

        for (int i = 1; i <= 6; i++)
        {
            LevelGrid grid = GetGrid((Direction)i);
            LevelGrid selfCoveringGrid = grid.coveringGrid;
            if(selfCoveringGrid != null)
            {
                selfCoveringGrid.UpdateAllVisual(1);
            }
        }
    }

    public LevelGrid GetGrid(Direction dir)//get one grid
    {
        return grids[(int)dir - 1];
    }

    private Direction GetOpposeDirection(Direction dir)
    {
        Direction ret;
        if((int)dir > 4)
        {
            ret = (Direction)LoopAdd((int)dir, 1, 5, 6);
        }
        else
        {
            ret = (Direction)LoopAdd((int)dir, 2, 1, 4);
        }
        //Debug.Log($"[Function]GetOpposeDirection({dir}) = {ret}");
        return ret;
    }

    private Direction GetRelativeDirection(Direction ori, Direction near, int height)
    {
        Direction ret;
        if((int)ori <= 4)
        {
            if (near == Direction.LEFT || near == Direction.RIGHT)
            {
                ret = (Direction)LoopAdd((int)near, 2, 1, 4);
            }
            else if (ori == Direction.LEFT || ori == Direction.RIGHT) 
            {
                if (near == Direction.FORWARD && ori == Direction.RIGHT || near == Direction.BACK && ori == Direction.LEFT) 
                {
                    ret = (Direction)LoopAdd((int)ori, -height, 1, 4);
                }
                else
                {
                    ret = (Direction)LoopAdd((int)ori, height, 1, 4);
                }
            }
            else
            {
                if(height != 1)
                {
                    ret = (Direction)LoopAdd((int)ori, height, 1, 4);
                }
                else
                {
                    ret = (Direction)LoopAdd((int)near, 2, 1, 4);
                }
            }
        }else if(height != 1 && near == Direction.FORWARD || near == Direction.BACK)
        {
            if(height == 0 && ori == Direction.UP || height == 2 && ori == Direction.DOWN)
            {
                ret = Direction.BACK;
            }
            else
            {
                ret = Direction.FORWARD;
            }
        }
        else
        {
            if ((near == Direction.FORWARD || near == Direction.RIGHT) && ori == Direction.UP ||
                ((near == Direction.LEFT) || (near == Direction.BACK)) && ori == Direction.DOWN)  
            {
                ret = (Direction)LoopAdd((int)near, -height - 1, 1, 4);
            }
            else
            {
                ret = (Direction)LoopAdd((int)near, height + 1, 1, 4);
            }
        }
        //Debug.Log($"[Function]GetRelativeDirection({ori},{near},{height}) = {ret}");
        return ret;
    }

    public LevelGrid GetSelfRelativeDirectionGrid(Direction ori,Direction near)
    {
        LevelGrid ret;
        if((int)ori > 4)
        {
            ret = GetGrid(near);
        }
        else
        {
            if(near == Direction.FORWARD)
            {
                ret = GetGrid(Direction.DOWN);
            }
            else if(near == Direction.BACK)
            {
                ret = GetGrid(Direction.UP);
            }
            else if(near == Direction.RIGHT)
            {
                ret = GetGrid((Direction)LoopAdd((int)ori, -1, 1, 4));
            }
            else
            {
                ret = GetGrid((Direction)LoopAdd((int)ori, 1, 1, 4));
            }
        }
        //Debug.Log($"[Function]GetSelfRelativeDirectionGrid({ori},{near}) = {ret.transform.parent.name} - {ret.direction}");
        return ret;
    }

    private int LoopAdd(int a,int b,int min,int max)
    {
        int d = a;
        int c = b % (max - min + 1);
        a += c;
        if(a > max)
        {
            a -= (max - min + 1);
        }else if(a < min)
        {
            a += (max - min + 1);
        }
        //Debug.Log($"LoopAdd:a = {d}, b = {b}, result = {a}");
        return a;
    }

    public Direction GetDirectionOfHitPoint(Vector3 p)
    {
        Vector3 pp = transform.parent.parent.InverseTransformPoint(p) - transform.localPosition;
        if (Mathf.Approximately(pp.x, 0.5f))
        {
            return Direction.RIGHT;
        }
        else if (Mathf.Approximately(pp.x, -0.5f))
        {
            return Direction.LEFT;
        }
        else if (Mathf.Approximately(pp.y, 0.5f))
        {
            return Direction.UP;
        }
        else if (Mathf.Approximately(pp.y, -0.5f))
        {
            return Direction.DOWN;
        }
        else if (Mathf.Approximately(pp.z, -0.5f)) 
        {
            return Direction.BACK;
        }
        return Direction.FORWARD;
    }

    public void GetConnectingDirectionOfHitPoint(Vector3 p)
    {
        Vector3 pp = transform.parent.parent.InverseTransformPoint(p) - transform.localPosition;
        Debug.Log(pp);
    }

    public Vector3[] GetPointsForView(Direction dir)
    {
        Vector3[] squarePoints = new Vector3[4];
        float p = 0.51f;
        switch (dir)
        {
            case Direction.FORWARD:
                {
                    squarePoints[0] = transform.TransformPoint(new Vector3(-p, p, p));
                    squarePoints[1] = transform.TransformPoint(new Vector3(p, p, p));
                    squarePoints[2] = transform.TransformPoint(new Vector3(p, -p, p));
                    squarePoints[3] = transform.TransformPoint(new Vector3(-p, -p, p));
                    break;
                }
            case Direction.LEFT:
                {
                    squarePoints[0] = transform.TransformPoint(new Vector3(-p, p, p));
                    squarePoints[1] = transform.TransformPoint(new Vector3(-p, p, -p));
                    squarePoints[2] = transform.TransformPoint(new Vector3(-p, -p, -p));
                    squarePoints[3] = transform.TransformPoint(new Vector3(-p, -p, p));
                    break;
                }
            case Direction.BACK:
                {
                    squarePoints[0] = transform.TransformPoint(new Vector3(-p, p, -p));
                    squarePoints[1] = transform.TransformPoint(new Vector3(p, p, -p));
                    squarePoints[2] = transform.TransformPoint(new Vector3(-p, p, -p));
                    squarePoints[3] = transform.TransformPoint(new Vector3(-p, -p, -p));
                    break;
                }
            case Direction.RIGHT:
                {
                    squarePoints[0] = transform.TransformPoint(new Vector3(p, p, -p));
                    squarePoints[1] = transform.TransformPoint(new Vector3(p, -p, -p));
                    squarePoints[2] = transform.TransformPoint(new Vector3(p, -p, p));
                    squarePoints[3] = transform.TransformPoint(new Vector3(p, p, p));
                    break;
                }
            case Direction.UP:
                {
                    squarePoints[0] = transform.TransformPoint(new Vector3(p, p, p));
                    squarePoints[1] = transform.TransformPoint(new Vector3(-p, p, p));
                    squarePoints[2] = transform.TransformPoint(new Vector3(-p, p, -p));
                    squarePoints[3] = transform.TransformPoint(new Vector3(p, p, -p));
                    break;
                }
            case Direction.DOWN:
                {
                    squarePoints[0] = transform.TransformPoint(new Vector3(p, -p, p));
                    squarePoints[1] = transform.TransformPoint(new Vector3(-p, -p, p));
                    squarePoints[2] = transform.TransformPoint(new Vector3(-p, -p, -p));
                    squarePoints[3] = transform.TransformPoint(new Vector3(p, -p, -p));
                    break;
                }
        }
        return squarePoints;
    }

    public Vector3 GetDirection(Direction d)
    {
        return transform.TransformDirection(DirectionVectors[(int)d - 1]);
    }

    public void InitializeGridVisual(string defaultPath,string framePath,string planePath)
    {
        foreach(LevelGrid grid in grids)
        {
            grid.defaultModelPath = defaultPath;
            grid.frameFilePath = framePath;
            grid.planeFilePath = planePath;
            grid.SetType(GridType.NOTSPRAYABLE);
            grid.UpdateVisual();
        }
    }

}
