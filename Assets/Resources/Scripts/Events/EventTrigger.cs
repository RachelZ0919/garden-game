using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EventTrigger : MonoBehaviour
{
    private Collider collider;
    private TextController textController;
    private PlayerShooting player;
    public string[] messages;
    public WaterColor color;
    public bool isMessage;
    public bool isColorAdd;
    public bool isGetGun;
    public bool isAnimation;
    public bool needRaycast;
    public bool isAuto;
    public bool isNextLevel;
    public EventTrigger[] nextEvents;

    private float startTime;
    private float duration;
    private bool isShowingMessage;
    private int messageIndex;

    private bool inRange;
    public bool isLookedAt;

    private EventItem item;
    // Start is called before the first frame update
    void Awake()
    {
        collider = GetComponent<Collider>();
        collider.isTrigger = true;
        textController = GameObject.Find("TIPSbase").GetComponent<TextController>();
        player = GameObject.Find("player").GetComponent<PlayerShooting>();
        inRange = false;
        gameObject.layer = 11;
    }

    private void Start()
    {
        item = GetComponentInChildren<EventItem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isShowingMessage)
        {
            if(Time.time > startTime + duration + 0.5f)
            {
                if (messageIndex == messages.Length - 1)
                {
                    Destroy(gameObject);
                }
                else
                {
                    startTime = Time.time;
                    messageIndex++;
                    duration = messages[messageIndex].Length * 0.08f;
                    textController.showInfo(messages[messageIndex], duration);
                }

            }
        }
        if (needRaycast)
        {
            if (inRange && isLookedAt)
            {
                //TODO:HighLight
                item.EnableHighLight(true);
            }
            else
            {
                item.EnableHighLight(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isAuto)
        {
            if (!needRaycast)
            {
                Act();
            }
            else
            {
                inRange = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        inRange = false;
    }


    private void GivePlayerGun()
    {
        isGetGun = false;
        player.SetGun(true);
        Destroy(gameObject);
    }

    private void GivePlayerColor()
    {
        isColorAdd = false;
        player.AddColor(color);
        Destroy(gameObject);
    }

    private void ShowMessgae()
    {
        isShowingMessage = true;
        if(messages != null)
        {
            messageIndex = 0;
            startTime = Time.time;
            duration = messages[0].Length * 0.08f;
            textController.showInfo(messages[0], duration);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Interact()
    {
        if (inRange && isLookedAt)
        {
            Act();
        }
    }
   

    public void Act()
    {
        if (isColorAdd)
        {
            GivePlayerColor();
        }
        else if (isGetGun)
        {
            GivePlayerGun();
        }
        else if (isMessage)
        {
            ShowMessgae();
        }else if (isAnimation)
        {
            PlayAnimation();
        }else if (isNextLevel)
        {
            LevelManager.NextLevel();
        }
    }

    private void PlayAnimation()
    {
        item.PlayAnimation();
        isAnimation = false;
    }

    private void OnDestroy()
    {
        foreach(EventTrigger eventTrigger in nextEvents)
        {
            eventTrigger.Act();
        }
    }
}
