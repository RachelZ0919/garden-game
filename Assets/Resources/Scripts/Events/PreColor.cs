using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreColor : MonoBehaviour
{
    public WaterColor color;
    Vector3 centerPointOffset = new Vector3(0.5f, 0, 0.5f);
    private bool hasFilled = false;
    LevelCube cube;
    LevelGrid grid;
    Vector3 hitPoint;
    // Start is called before the first frame update
    void Start()
    {
        centerPointOffset = new Vector3(0.5f, 0, 0.5f);
        int layerMask = 1 << 9;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + transform.TransformVector(centerPointOffset) + transform.TransformDirection(Vector3.up) * 0.5f, transform.TransformDirection(Vector3.down), out hit, 1.2f, layerMask))
        {
            hitPoint = hit.point;
            cube = hit.collider.GetComponent<LevelCube>();
            grid = cube.GetGridAtPosition(hit.point);
        }
    }

    private void Update()
    {
        if (!hasFilled)
        {
            hasFilled = true;
            cube.FillAtPosition(hitPoint, color);
            grid.SprayWater(color);
        }

    }

    private void OnDrawGizmos()
    {
       // Gizmos.DrawCube(transform.position + transform.TransformVector(centerPointOffset), new Vector3(0.1f, 0.3f, 0.1f));
    }
}
