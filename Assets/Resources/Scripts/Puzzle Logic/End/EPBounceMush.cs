using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EPBounceMush: EndPoint
{
    private Rigidbody player;
    [Range(1.0f, 50.0f)]
    public float bounceY = 10.0f;
    private float epsilon = 0.0001f;
    private Vector3 bounceV;

    public override string endPointName
    {
        get
        {
            return "BounceMushroom";
        }
    }

    public override bool[] directionAllowedSpawn => throw new System.NotImplementedException();

    private void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.tag == "Player")
        {
            if (isActivate)
            {
                player = hit.gameObject.GetComponent<Rigidbody>();

                if (Mathf.Abs(player.velocity.y - 0) < epsilon)
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
        base.Activate();
    }

    override public void Deactivate()
    {
        base.Deactivate();
    }

    public override void UpdateVisual(int num, int code)
    {
        
    }
}
