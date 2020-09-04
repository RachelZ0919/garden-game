using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
