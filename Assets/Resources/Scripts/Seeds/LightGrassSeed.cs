using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightGrassSeed : StartPoint
{
    public List<Door> doors = new List<Door>();
    public Transform lines;
    public Material lightedLineMat;
    public Material darkedLineMat;
    private AudioSource audioSource;
    public AudioClip successClip;
    public AudioClip failedClip;
    public bool isPre;
    // Start is called before the first frame update
    void Start()
    {
        seedType = SeedType.LIGHTGRASS;
        InitializeGrid();
        InitializeDoor();
        if(lines != null)
        {
            foreach (MeshRenderer renderer in lines.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.material = darkedLineMat;
            }
        }
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        UpdateNewVisual();
    }

    override public void Activate()
    {
        if(ChangeModel(true))   SeasameDoor();
    }

    override public void Deactivate()
    {
        if(ChangeModel(false))  SeasameDoor();
    }

    private void SeasameDoor()
    {
        if (isActivate)
        {
            if (isPre)
            {
                isPre = false;
            }
            else
            {
                audioSource.clip = successClip;
                audioSource.Play();
            }
            foreach (Door door in doors)
            {
                door.openLock();
                if (lines != null)
                {
                    foreach (MeshRenderer renderer in lines.GetComponentsInChildren<MeshRenderer>())
                    {
                        renderer.material = lightedLineMat;
                    }
                }
            }
        }
        else
        {
            if (isPre)
            {
                isPre = false;
            }
            else
            {
                audioSource.clip = failedClip;
                audioSource.Play();
            }
            foreach (Door door in doors)
            {
                door.addLock();
                if (lines != null)
                {
                    foreach (MeshRenderer renderer in lines.GetComponentsInChildren<MeshRenderer>())
                    {
                        renderer.material = darkedLineMat;
                    }
                }
            }
        }
    }

    private void InitializeDoor()
    {
        //TODO:加门（或许直接手动）
        foreach (Door door in doors)
        {
            door.addLock();
        }
    }
}
