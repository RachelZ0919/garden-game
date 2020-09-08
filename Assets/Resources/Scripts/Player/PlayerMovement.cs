using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    #region Variables

    #region Player Look - Variables
    
    private Vector3 originalRotation;
    private Vector3 targetAngle;
    private Vector3 followAngle;
    private Vector3 followVelocity;
    public Camera playerCamera;
    [Range(90, 180)]
    public int verticalRotationRange;

    #endregion

    #region Player Movement - Variables

    public bool playerCanMove = true;
    public bool playerCanJump = true;
    public bool canHoldJump = true;
    public float walkSpeed = 4f;
    public float jumpPower = 5f;

    private CapsuleCollider capsule;
    private Rigidbody rigidbody;
    private float speed;
    private bool jumpInput;
    private bool isGrounded;
    private bool didJump;
    private float yVelocity;
    private PhysicMaterial zeroFrictionMaterial;


    #endregion Headbob - Variables
    public bool useHeadBob = true; //是否使用headbob
    public Transform head = null; //头（摄像机？）
    public bool snapHeadjointToCapsul = true; //影响下蹲效果
    public float headbobFrequency = 1.5f;//频率
    public float headbobSwayAngle = 5f;//?
    public float headbobHeight = 3f;//高度
    public float headbobSideMovement = 5f;//?
    public float jumpLandIntensity = 3f;//跳跃降落后的摇晃强度

    private Vector3 originalLocalPosition;
    private float nextStepTime = 0.5f;
    private float headbobCycle = 0.0f;
    private float headbobFade = 0.0f;
    private float springPosition = 0.0f;
    private float springVelocity = 0.0f;
    private float springElastic = 1.1f; //弹性系数
    private float springDampen = 0.8f; //阻力的系数
    private float springVelocityThreshold = 0.05f;
    private float springPositionThreshold = 0.05f;
    private Vector3 previousPosition;
    private Vector3 previousVelocity = Vector3.zero;
    private Vector3 miscRefVel; //?
    private bool previousGrounded;
    #region

    #endregion

    #endregion

    private void Awake()
    {
        #region Player Look - Awake

        originalRotation = transform.localRotation.eulerAngles;

        #endregion

        #region Player Movement - Awake

        capsule = GetComponent<CapsuleCollider>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        isGrounded = true;
        
        zeroFrictionMaterial = new PhysicMaterial();
        zeroFrictionMaterial.staticFriction = 0;
        zeroFrictionMaterial.dynamicFriction = 0;
        zeroFrictionMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        zeroFrictionMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
        capsule.sharedMaterial = zeroFrictionMaterial;

        #endregion

        #region Headbob - Awake
        
        #endregion
    }

    private void Start()
    {
        #region Player Look - Start

        Cursor.lockState = CursorLockMode.Locked;//lock cursor
        Cursor.visible = false;

        #endregion

        #region Player Movement - Start

        capsule.radius = capsule.height / 4;

        #endregion

        #region Headbob - Start

        originalLocalPosition = snapHeadjointToCapsul ? new Vector3(head.localPosition.x, (capsule.height / 2) * head.localScale.y, head.localPosition.z) : head.localPosition;
        previousPosition = rigidbody.position;

        #endregion
    }

    private void Update()
    {
        #region Player Look - Update
        
        float mouseXInput = 0;
        float mouseYInput = 0;
        mouseXInput = Input.GetAxis("Mouse X");//get input
        mouseYInput = Input.GetAxis("Mouse Y");

        if (targetAngle.y > 180)//targetAngle range from -180 to 180
        {
            targetAngle.y -= 360;
            followAngle.y -= 360;
        }
        else if (targetAngle.y < -180)
        {
            targetAngle.y += 360;
            followAngle.y += 360;
        }
        if (targetAngle.x > 180)
        {
            targetAngle.x -= 360;
            followAngle.x -= 360;
        }
        else if (targetAngle.x < -180)
        {
            targetAngle.x += 360;
            followAngle.x += 360;
        }

        targetAngle.y += mouseXInput;//add input
        targetAngle.x += mouseYInput;

        targetAngle.x = Mathf.Clamp(targetAngle.x, -0.5f * verticalRotationRange, 0.5f * verticalRotationRange);//clamp to range

        followAngle = Vector3.SmoothDamp(followAngle, targetAngle, ref followVelocity, 0f);//smooth damp(calculate the actural angle)

        playerCamera.transform.localRotation = Quaternion.Euler(-followAngle.x + originalRotation.x, 0, 0);//rotate player(camera - head, transform - body)
        transform.localRotation = Quaternion.Euler(0, followAngle.y + originalRotation.y, 0);

        #endregion

        #region Player Movement - Update
        #endregion

        #region Player Jump - Update
        
        if(canHoldJump?(playerCanJump && Input.GetButton("Jump")):(Input.GetButtonDown("Jump") && playerCanJump))
        {
            jumpInput = true;
        }else if (Input.GetButtonUp("Jump"))
        {
            jumpInput = false;
        }

        #endregion

        #region Headbob - Update
        
        #endregion
    }

    private void FixedUpdate()
    {
        #region Player Look - FixedUpdate

        #endregion
        #region Player Movement - FixedUpdate

        Vector3 moveDirection = Vector3.zero;
        speed = walkSpeed;//get speed

        Vector2 inputXY = Vector2.zero;
        if (playerCanMove)
        {
            float horizontalInput = Input.GetAxis("Horizontal");//get input direction
            float verticalInput = Input.GetAxis("Vertical");
            inputXY = new Vector2(horizontalInput, verticalInput);

            if (inputXY.magnitude > 1) { inputXY.Normalize(); }
        }

        #region Player Jump
        
        yVelocity = rigidbody.velocity.y;

        if(isGrounded && jumpInput && !didJump)
        {
            didJump = true;
            jumpInput = false;
            yVelocity += jumpPower;
        }

        #endregion

        moveDirection = transform.forward * inputXY.y * speed + transform.right * inputXY.x * speed;

        rigidbody.velocity = moveDirection + (Vector3.up * yVelocity);

        #endregion
        #region Headbob - Update
        float ypos = 0;
        float xpos = 0;
        float zTilt = 0;
        float xTilt = 0;
        float bobSwayFactor = 0;
        float bobFactor = 0;
        float strideLangthen = 0;
        float flatVel = 0;

        if (useHeadBob)
        {
            Vector3 vel = (rigidbody.position - previousPosition) / Time.deltaTime;
            Vector3 velChange = vel - previousVelocity;
            previousPosition = rigidbody.position;
            previousVelocity = vel;

            springVelocity -= velChange.y;
            springVelocity -= springPosition * springElastic;
            springVelocity *= springDampen;
            springPosition += springVelocity * Time.deltaTime;
            springPosition = Mathf.Clamp(springPosition, -0.3f, 0.3f);

            if(Mathf.Abs(springVelocity) < springVelocityThreshold && Mathf.Abs(springPosition) < springPositionThreshold)
            {
                springPosition = 0;
                springVelocity = 0;
            }

            flatVel = new Vector3(vel.x, 0, vel.z).magnitude;
            strideLangthen = 1 + (flatVel * ((headbobFrequency * 2) / 10));
            headbobCycle += (flatVel / strideLangthen) * (Time.deltaTime / headbobFrequency);
            bobFactor = Mathf.Sin(headbobCycle * Mathf.PI * 2);
            bobSwayFactor = Mathf.Sin(Mathf.PI * (2 * headbobCycle + 0.5f));
            bobFactor = 1 - (bobFactor * 0.5f + 1);
            bobFactor *= bobFactor;

            if(jumpLandIntensity > 0)
            {
                xTilt = -springPosition * (jumpLandIntensity * 5.5f);
            }
            else
            {
                xTilt = -springPosition;
            }

            if (isGrounded)
            {
                if(flatVel < 0.1f)
                {
                    headbobFade = Mathf.MoveTowards(headbobFade, 0.0f, 0.5f);
                }
                else
                {
                    headbobFade = Mathf.MoveTowards(headbobFade, 1.0f, Time.deltaTime);
                }
                float speedHeightFactor = 1 + (flatVel * 0.3f);
                xpos = -(headbobSideMovement / 10) * headbobFade * bobSwayFactor;
                ypos = springPosition * (jumpLandIntensity / 10) + bobFactor * (headbobHeight / 10) * headbobFade * speedHeightFactor;
                zTilt = bobSwayFactor * (headbobSwayAngle / 10) * headbobFade;
            }

            if(rigidbody.velocity.magnitude > 0.1f)
            {
                head.localPosition = Vector3.MoveTowards(head.localPosition,
                    snapHeadjointToCapsul ? (new Vector3(originalLocalPosition.x, (capsule.height / 2) * head.localScale.y, originalLocalPosition.z) + new Vector3(xpos, ypos, 0)) :
                                            originalLocalPosition + new Vector3(xpos, ypos, 0), 
                    0.5f);
            }
            else
            {
                head.localPosition = Vector3.SmoothDamp(head.localPosition,
                    snapHeadjointToCapsul ? (new Vector3(originalLocalPosition.x, (capsule.height / 2) * head.localScale.y, originalLocalPosition.z) + new Vector3(xpos, ypos, 0)) :
                                            originalLocalPosition + new Vector3(xpos, ypos, 0),
                    ref miscRefVel, 0.15f);
            }
            head.localRotation = Quaternion.Euler(xTilt, 0, zTilt);
        }


        #endregion

        #region Reset Variables

        isGrounded = false;

        #endregion
    }

    private void OnCollisionEnter(Collision collision)
    {
        for(int i = 0; i < collision.contactCount; i++)
        {
            float a = Vector3.Angle(collision.GetContact(i).normal, Vector3.up);
            if (collision.GetContact(i).point.y < transform.position.y - ((capsule.height / 2) - capsule.radius * 0.95f))
            {
                if (!isGrounded)
                {
                    isGrounded = true;
                    if(didJump && a < 70 )
                    {
                        didJump = false;
                    }
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            float a = Vector3.Angle(collision.GetContact(i).normal, Vector3.up);
            if (collision.GetContact(i).point.y < transform.position.y - ((capsule.height / 2) - capsule.radius * 0.95f))
            {
                if (!isGrounded)
                {
                    isGrounded = true;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}
