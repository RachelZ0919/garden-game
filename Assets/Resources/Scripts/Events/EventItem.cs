using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventItem : MonoBehaviour
{
    private Material mat;
    private Color highlightColor;
    private Color normalColor;
    private float normalWidth;
    private float highlightWidth;
    private bool highlightEnabled;

    private Animator animator;

    private void Awake()
    {
        highlightColor = new Color(1, 0.945f, 0.1875f);
        highlightWidth = 0.15f;
        normalColor = Color.black;
        normalWidth = 0.1f;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            mat = GetComponent<MeshRenderer>().material;
            mat.SetColor("_ASEOutlineColor", normalColor);
            mat.SetFloat("_ASEOutlineWidth", normalWidth);
        }
        animator = GetComponent<Animator>();
        highlightEnabled = false;
    }

    public void EnableHighLight(bool b)
    {
        if (highlightEnabled != b)
        {
            highlightEnabled = b;
            if(mat != null)
            {
                if (b)
                {
                    mat.SetColor("_ASEOutlineColor", highlightColor);
                    mat.SetFloat("_ASEOutlineWidth", highlightWidth);
                }
                else
                {
                    mat.SetColor("_ASEOutlineColor", normalColor);
                    mat.SetFloat("_ASEOutlineWidth", normalWidth);
                }
            }
        }
    }

    public void Interact()
    {
        transform.parent.GetComponent<EventTrigger>().Interact();
    }

    public void PlayAnimation()
    {
        Debug.Log(animator.gameObject.name);
        animator.SetBool("TriggerOn", true);
    }
}
