using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceMushSeed : StartPoint
{
    private Rigidbody rigidbody, player;
    private BoxCollider collider;
    [Range(1.0f, 50.0f)]
    public float bounceY = 10.0f;
    private float epsilon = 0.0001f;
    private Vector3 bounceV;

    // Start is called before the first frame update
    void Start()
    {
        seedType = SeedType.BOUNCEMUSH;
        InitializeGrid();

        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        UpdateNewVisual();
    }


    //private void OnCollisionEnter(Collision hit)
    //{
    //    if (hit.gameObject.tag == "Player")
    //    {
    //        if (isActivate)
    //        {
    //            //Debug.Log("Hit bounce mushroom");
    //            player = hit.gameObject.GetComponent<Rigidbody>();

    //            if (player.velocity[1] - 0 < epsilon)
    //            {
    //                bounceV = new Vector3(0f, bounceY, 0f);
    //                player.velocity = bounceV;
    //            }
    //            else
    //            {
    //                player.velocity *= -1;
    //            }
    //        }
    //    }
    //}

    private void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.tag == "Player")
        {
            if (isActivate)
            {
                //Debug.Log("Hit bounce mushroom");
                player = hit.gameObject.GetComponent<Rigidbody>();

                if (player.velocity[1] - 0 < epsilon)
                {
                    bounceV = new Vector3(0f, bounceY, 0f);
                    player.velocity = bounceV;
                }
                else
                {
                    player.velocity *= -1;
                }
            }
        }
    }

    override public void Activate()
    {
        ChangeModel(true);
    }

    override public void Deactivate()
    {
        ChangeModel(false);
    }
}
