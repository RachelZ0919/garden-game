using UnityEngine;
using System.Collections;


abstract public class Trigger : MonoBehaviour
{
    public bool isActivated
    {
        get
        {
            return seedRegistered == activatedSeedCount && seedRegistered != 0;
        }
    }
    private int seedRegistered = 0;
    private int activatedSeedCount = 0;

    abstract protected void Activate();
    abstract protected void DeActivate();

    public void Register()
    {
        seedRegistered++;
    }

    public void ActivateTrigger()
    {
        activatedSeedCount++;
        if(activatedSeedCount == seedRegistered)
        {
            Activate();
        }
    }

    public void DeactivateTrigger()
    {
        if(activatedSeedCount == seedRegistered)
        {
            DeActivate();
        }
        activatedSeedCount--;
    }
}
