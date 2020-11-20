using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ObjectRotate : MonoBehaviour
{
    private bool isSpaceDown = false;
    private bool isMouseDown = false;
    private bool isRotating = false;

    private Vector3 mouseStartPosition;
    private float rotateAngle;
    private float targetAngle;
    private float startAngle;
    private float dampVel = 0;

    public float initialAngle = 0;
    public List<float> eventAngles;

    // Use this for initialization
    void Start()
    {
        rotateAngle = transform.rotation.eulerAngles.y;
        targetAngle = rotateAngle;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isSpaceDown = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isSpaceDown = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            isMouseDown = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isMouseDown = false;
        }

        if (isRotating)
        {
            if (!isMouseDown)
            {
                isRotating = false;
                foreach(float eventangle in eventAngles)
                {
                    if(Mathf.Abs(targetAngle - eventangle) < 7f)
                    {
                        targetAngle = eventangle;
                        break;
                    }
                }
            }
            else
            {
                RotateByMouse();
            }
        }
        else
        {
            if (isMouseDown && isSpaceDown)
            {
                isRotating = true;
                StartRotate();
                
            }
        }

        rotateAngle = Mathf.SmoothDamp(rotateAngle, targetAngle, ref dampVel, 0.1f);
        transform.rotation = Quaternion.Euler(0, rotateAngle, 0);
    }

    public void StartRotate()
    {
        mouseStartPosition = Input.mousePosition;
        startAngle = targetAngle;
    }

    public void RotateByMouse()
    {
        targetAngle = startAngle - (Input.mousePosition - mouseStartPosition).x * 0.2f;
    }

    public void RotateTo(float angle)
    {
        rotateAngle = angle;
        transform.rotation = Quaternion.Euler(0, rotateAngle, 0);
    }
}
