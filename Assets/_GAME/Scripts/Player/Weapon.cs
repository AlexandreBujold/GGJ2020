using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Weapon : MonoBehaviour, IActivatable
{   
    [ShowInInspector]
    public float cooldown { get; set; }
    public Coroutine cooldownCoroutine = null;

    public virtual void Activate()
    {
        if (StartCooldown()) //Can go on cooldown, which means it can be used
        {
            //Activate
        }
    }

    public virtual void Deactivate()
    {
        //Deactivate

        StartCooldown();
    }

    public virtual bool StartCooldown()
    {
        if (cooldownCoroutine == null)
        {
            cooldownCoroutine = StartCoroutine(Cooldown(cooldown));
            return true;
        }
        return false;
    }

    public IEnumerator Cooldown(float time)
    {
        yield return new WaitForSeconds(time);
        cooldownCoroutine = null;
    }
}
