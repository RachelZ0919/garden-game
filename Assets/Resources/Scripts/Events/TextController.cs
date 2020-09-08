using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextController : MonoBehaviour
{
    public GameObject tip;
    public GameObject textObject;

    private float duration,disappear = -1;
    private string str;
    private bool flag = false;
    // Start is called before the first frame update
    void Start()
    {
        //showInfo("aaaaaaaaaaaaa", 3f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!flag) tip.SetActive(false);
        if(flag && this.disappear > 0)
        {
            if (Time.time > this.disappear)
            {
                hideInfo();
            }
        }

    }

    public void showInfo(string str,float duration)
    {
        flag = true;
        this.str = str;
        this.duration = duration;
        Text text = textObject.GetComponent<Text>();
        text.text = this.str;
        if (this.duration < 0)
        {
            this.disappear = -1;
        }
        else
        {
            this.disappear = Time.time + this.duration;
        }
        tip.SetActive(true);
    }

    public void hideInfo()
    {
        flag = false;
        this.disappear = -1;
    }

}
