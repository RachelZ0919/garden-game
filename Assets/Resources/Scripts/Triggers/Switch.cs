using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    public Vector3 centerPointOffset;
    public bool initialState = true;
    private LevelGrid affectingGrid;
    private LevelCube affectingCube;
    private GridType oriType;
    private MeshRenderer meshRenderer;

    private Vector3 hitPoint;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        InitializeGrid();
        if (initialState)
        {
            affectingGrid.type = GridType.GROUND;
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }
    
    private void InitializeGrid()
    {
        int layerMask = 1 << 9 | 1 << 10;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + transform.TransformVector(centerPointOffset) + transform.TransformDirection(Vector3.up) * 0.5f, transform.TransformDirection(Vector3.down), out hit, 1.2f, layerMask))
        {
            hitPoint = hit.point;
            affectingCube = hit.collider.GetComponent<LevelCube>();
            affectingGrid = affectingCube.GetGridAtPosition(hit.point);
            oriType = affectingGrid.type;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position + transform.TransformVector(centerPointOffset) + transform.TransformDirection(Vector3.up) * 0.5f, hitPoint);

    }

    public void SealGrid()
    {
        affectingGrid.ClearGrid();
        affectingGrid.type = GridType.GROUND;
        affectingCube.DisableLawnAtGrid(affectingGrid);
        meshRenderer.enabled = true;
    }

    public void UnsealGrid()
    {
        affectingGrid.type = oriType;
        meshRenderer.enabled = false;
    }
}
