using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SeedType
{
    LIGHTGRASS = 1,
    PUREGRASS = 2,
    BOUNCEMUSH = 3,
    BLOCKTREE = 4,
    WEED = 5
}

abstract public class StartPoint : MonoBehaviour
{
    public bool isActivate = false;
    public WaterColor color;
    public Vector3 centerPointOffset;

    public GameObject activatedGrass;
    public GameObject deactivatedGrass;
    public GameObject offGrass;

    public SeedType seedType;
    private LevelGrid grid;

    private Vector3 hitPoint;
    private bool needVisualUpdate = true;

    protected void InitializeGrid()
    {
        int layerMask = 1 << 9;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + transform.TransformVector(centerPointOffset) + transform.TransformDirection(Vector3.up) * 0.5f, transform.TransformDirection(Vector3.down), out hit, 1.2f, layerMask))
        {
            hitPoint = hit.point;
            grid = hit.collider.GetComponent<GridSplitter>().GetGridAtPosition(hit.point);
            grid.type = GridType.SEED;
            grid.groundColor = color;
            grid.state = 2;
            grid.seed = this;
            grid.grassStates[(int)seedType - 1] = 1;
        }
        GameManager.AddStartPoint(this);
    }

    abstract public void Activate();

    abstract public void Deactivate();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position + transform.TransformVector(centerPointOffset) + transform.TransformDirection(Vector3.up) * 0.5f, hitPoint);  
    }

    protected bool ChangeModel(bool b)
    {
        if(isActivate != b)
        {
            isActivate = b;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void VisualUpdateRequest()
    {
        needVisualUpdate = true;
    }

    protected void UpdateNewVisual()
    {
        if (needVisualUpdate)
        {
            activatedGrass.SetActive(false);
            deactivatedGrass.SetActive(false);
            offGrass.SetActive(false);
            GetNewVisual().SetActive(true);
            needVisualUpdate = false;
        }
    }
    private GameObject GetNewVisual()
    {
        if(grid.state == 2)
        {
            for(int i = 1; i <= 4; i++)
            {
                LevelGrid tmp = grid.GetNearGrid((NearGridDirection)i);
                if(tmp != null && tmp.state == 2 && color == tmp.groundColor)
                {
                    return deactivatedGrass;
                }
            }
            return offGrass;
        }
        else
        {
            return activatedGrass;
        }
    }
}
