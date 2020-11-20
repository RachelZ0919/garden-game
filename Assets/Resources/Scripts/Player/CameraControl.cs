using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityScript.Steps;

[ExecuteAlways]
public class CameraControl : MonoBehaviour
{
    private bool isSpaceDown = false;
    private bool isMouseDown = false;
    private bool isRotating = false;

    private Vector3 centerPoint;
    private Vector3 originPosition;
    private Vector3 mouseStartPosition;
    private float rotateAngle;
    private float targetAngle;
    private float startAngle;
    private float radius;
    private float dampVel = 0;

    public Transform centerObject;
    public Transform cameraTransform;

    // Start is called before the first frame update
    void Start()
    {
        centerPoint = centerObject.position;
        originPosition = transform.position;
        Vector3 vec = originPosition - centerPoint;
        float angle = Mathf.Acos(vec.y / vec.magnitude);
        radius = vec.magnitude * Mathf.Sin(angle);
        rotateAngle = Mathf.Atan2(vec.x, vec.z) * Mathf.Rad2Deg;
        targetAngle = rotateAngle;
    }


    // Update is called once per frame
    void Update()
    {
        //rotateAngle = Mathf.SmoothDamp(rotateAngle, targetAngle, ref dampVel, 0.1f);
        //transform.position = new Vector3(Mathf.Sin(rotateAngle * Mathf.Deg2Rad) * radius, originPosition.y, Mathf.Cos(rotateAngle * Mathf.Deg2Rad) * radius);
        cameraTransform.LookAt(centerObject);
        
    }
}
