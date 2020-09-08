using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    int lockCount = 0;
    private Animator animator;
    // Start is called before the first frame update

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void openLock()
    {
        lockCount--;
        if(lockCount == 0)
        {
            animator.SetBool("isOpen", true);
            animator.SetBool("startChanging", true);
            //gameObject.SetActive(false);
        }
    }

    public void addLock()
    {
        if(lockCount == 0)
        {
            animator.SetBool("isOpen", false);
            //gameObject.SetActive(true);
        }
        lockCount++;
    }

    public void Open()
    {
        animator.SetBool("isOpen", true);
        //gameObject.SetActive(false);
    }
}
