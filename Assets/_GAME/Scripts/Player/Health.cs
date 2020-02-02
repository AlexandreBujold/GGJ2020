using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

public class Health : MonoBehaviour
{
    //Objects with this class will have health, and be able to take damage

    //TODO
    // - Add recording for who dealt damage
    [HideInInspector] public float timeLastHit;
    [HideInInspector] public float timeLastHealed;
    
    [ShowInInspector] [BoxGroup("Health Properties", true, false, 0)] [PropertyOrder(0)] [ReadOnly] public float health {get; private set; }

    //Base
    [BoxGroup("Health Properties")] [PropertyOrder(1)] public bool heal; //Can this object heal?
    [BoxGroup("Health Properties")] [PropertyOrder(1)] private bool isHealing; //Is this object healing?
    [BoxGroup("Health Properties")] [PropertyOrder(1)] public float startHealth;
    [BoxGroup("Health Properties")] [PropertyOrder(1)] public float maxHealth;
    [BoxGroup("Health Properties")] [PropertyOrder(1)] public float minHealth;

    //Invincibility
    [BoxGroup("Health Properties")] [PropertyOrder(1)] public bool invincible; //Can this object be invincible? Either periodically, circumstancially or permanently?
    [BoxGroup("Health Properties")] [PropertyOrder(1)] [ShowIf("invincible")] public bool invincibleOnStart; //Is the object invincible on start?
    [BoxGroup("Health Properties")] [PropertyOrder(1)] [ShowIf("invincible")] private bool isInvincible; //Allow Invincibility code to run
    [BoxGroup("Health Properties")] [PropertyOrder(1)] [SuffixLabel("seconds", Overlay = true)] [ShowIf("invincible")] public float invincibilityTime; //How long does this object stay invincible once hit?
    [BoxGroup("Health Properties")] [PropertyOrder(1)] [HideInInspector] [ReadOnly] public float timeSinceLastHit;

    //Recovery
    [BoxGroup("Health Properties")] [PropertyOrder(1)] public bool regenerate; //Can this object regenerate health? Either periodically, circumstancially or permanently?
    [BoxGroup("Health Properties")] [PropertyOrder(1)] private bool isRegenerating; //Allow Regeneration code to run
    [BoxGroup("Health Properties")] [PropertyOrder(1)] [SuffixLabel("health/sec", Overlay = true)] [ShowIf("regenerate")] public float regenerationRate; //How fast does this object's health recover?
    [BoxGroup("Health Properties")] [PropertyOrder(1)] [SuffixLabel("health/sec", Overlay = true)] [ShowIf("regenerate")] public float timeSinceHitToRegenerate; //How much time since being last hit will it take to start regenerating?

    public TextMeshProUGUI healthValueText;

    [HideInInspector] public List<Coroutine> healingCoroutines;
    [HideInInspector] public List<Coroutine> damagingCoroutines;
    [HideInInspector] public Coroutine regenerationCoroutine;

    //Events
    public OnHealth onHealed;
    public OnHealth onDamaged;
    public OnHealth onDeath;

    // Start is called before the first frame update
    void Awake()
    {
        Setup();
        //healingCoroutines.Add(StartCoroutine(TestRoutine()));
    }

    // Update is called once per frame
    void Update()
    {
       UpdateTimeSinceHit();
       InvincibilityTimer();
       RegenerationTimer();
    }

    [Button]
    public void TestHealingAmountAtRate()
    {
        StartCoroutine(HealAmountAtRate(5, 0.1f));
    }

    [Button]
    public void TestDamage()
    {
        Damage(UnityEngine.Random.Range(1, 10));
    }

    public void Setup()
    {
        SetHealth(startHealth);  

        healingCoroutines = new List<Coroutine>();
        damagingCoroutines = new List<Coroutine>();

        InitializeEvents();
        if (regenerate)
        {
           regenerationCoroutine = StartCoroutine(Regenerate()); 
        }

        if (invincibleOnStart)
        {
            timeLastHit = Time.time;
        }
        onHealed.AddListener(UpdateHealthUI);
        onDamaged.AddListener(UpdateHealthUI);
    }

    public void InitializeEvents()
    {
        if (onHealed == null)
        {
            onHealed = new OnHealth();
        }

        if (onDamaged == null)
        {
            onDamaged = new OnHealth();
        }

        if (onDeath == null)
        {
            onDeath = new OnHealth();
        }
    }

#region Health

    public void SetHealth(float targetAmount)
    {
        health = targetAmount;
    }

    public bool AllowRegeneration(bool value)
    {
        regenerate = value;
        return regenerate;
    }

    public bool AllowInvincibility(bool value)
    {
        invincible = value;
        return invincible;
    }

    public bool SetRegeneration(bool value)
    {
        isRegenerating = value;
        return isRegenerating;
    }

    public bool SetInvincibility(bool value)
    {
        isInvincible = value;
        return isInvincible;
    }

    public void UpdateTimeSinceHit()
    {
        timeSinceLastHit = Time.time - timeLastHit;
    }

    public void RegenerationTimer()
    {
        if (health == maxHealth) { SetRegeneration(false); return; }
        
        if (regenerate)
        {
            if (!isRegenerating)
            {
                if (timeSinceLastHit >= timeSinceHitToRegenerate && health != maxHealth)
                {
                    SetRegeneration(true);
                }
            }
        }
        
    }

    public void InvincibilityTimer()
    {
        if (invincible)
        {
            if (isInvincible)
            {
                if (timeSinceLastHit >= invincibilityTime)
                {
                    SetInvincibility(false);
                }
            }
        }
    }

#endregion

#region Health Recovery

    
    public void Heal(float amount)
    {
        if (heal)
        {
            float healthBeforeHeal = health;
            health = Mathf.Clamp(health + amount, minHealth, maxHealth);
            health = (float)Math.Round((double)health, 2);
            onHealed.Invoke(this);
            timeLastHealed = Time.time;
        }
    }

    public void FullHeal() //Add the remainder of health missing to this object's health
    {
        Heal(maxHealth % health);
    }

    public IEnumerator HealOverTime(float amount, float time) //Heal the specified amount over specified amount of time. 
    {
        //Calculate ratePerSecond
        float ratePerSecond = GetRateOverTime(amount, time);
        float amountHealed = 0;
        
        //Round time down and heal for the full ratePerSecond for that number of seconds
        for (int i = 0; i < Mathf.FloorToInt(time); i++)
        {
            if (IsMaxHealth() == true)
            {
                continue;
            }
            Heal(ratePerSecond);
            amountHealed += ratePerSecond;
            yield return new WaitForSeconds(1);
        }

        //Once full seconds have been healed for, wait the remainder of the time and then heal the remainder of the amount
        yield return new WaitForSeconds(time % 1f);
        Heal(amount % amountHealed);
    }

    public IEnumerator HealAmountAtRate(float amount, float healRatePerSecond) //Heal for amount at a rate of tickRate (e.g. Heal 50HP at a rate of 2.5 HP / 0.25 seconds)
    {
        //Ignores regenerationRate
        float numberOfTicks = GetRateOverTime(amount, healRatePerSecond);
        float amountHealed = 0;

        for (float i = 0; i < numberOfTicks; i++)
        {
            if (IsMaxHealth() == true)
            {
                continue;
            }
            Heal(healRatePerSecond);
            amountHealed += healRatePerSecond;
            yield return new WaitForSeconds(healRatePerSecond);
        }
        yield return null;
        //healingCoroutine = null;
    }

    public IEnumerator Regenerate()
    {
        for(;;)
        {
            if (isRegenerating)
            {
                Heal(regenerationRate);
                yield return new WaitForSeconds(1); 
            }
            else
            {
                yield return new WaitForSeconds(0.1f);  
            }
        }
    }

    public bool RemoveCoroutineFromList(ref List<Coroutine> coroutineList, Coroutine coroutineToRemove)
    {
        return false;
    }

#endregion

#region Health Loss

    public void Damage()
    {
        if (!isInvincible)
        {
            health = Mathf.Clamp(health - 1f, minHealth, maxHealth);

            OnHit();
            if (IsDead())
            {
                onDeath.Invoke(this);
                return;
            }
            onDamaged.Invoke(this); 
        }
    }

    public void Damage(float amount)
    {
        if (!isInvincible)
        {
            health = Mathf.Clamp(health - Mathf.Abs(amount), minHealth, maxHealth);
            OnHit();
            if (IsDead())
            {
                onDeath.Invoke(this);
                return;
            }         
        }
    }

    public void Kill(bool ignoreInvincibility)
    {
        if (!isInvincible)
        {
            health = 0;
        }
        else if (ignoreInvincibility)
        {
            health = 0;
        }
    }

    public Coroutine DamageOverTime(float amount, float time)
    {
        return StartCoroutine(DamageOverTimeCoroutine(amount, time));
    }
    
    public Coroutine DamageOverTimeAtRate(float amountPerSecond, float time)
    {
        return StartCoroutine(DamageOverTimeAtRateCoroutine(amountPerSecond, time));
    }

    private IEnumerator DamageOverTimeCoroutine(float amount, float time)
    {
        float ratePerSecond = GetRateOverTime(amount, time);

        yield return null;
        //damagingCoroutine = null;
    }

    private IEnumerator DamageOverTimeAtRateCoroutine(float amountPerSecond, float time)
    {        
        yield return null;
        //damagingCoroutine = null;
    }

#endregion

#region Checks

    public bool IsDead()
    {
        if (IsAlive() == false)
        {
            onDeath.Invoke(this);

            //Basic turnoffs on death
            regenerate = false;
            invincible = false;

            return true;
        }
        return false;
    }

    public bool IsAlive()
    {
        if (health > 0)
        {
            return true;
        }
        return false;
    }

    public float GetRateOverTime(float amount, float time)
    {
        return amount/time;
    }

    public bool IsMaxHealth()
    {
        return health >= maxHealth ? true : false;
    }

#endregion

#region UI

    public void UpdateHealthUI(Health hp)
    {
        if (healthValueText != null)
        {
            healthValueText.text = health.ToString();
        }
    }

    public void SetHealthBarVisibility(bool visible)
    {
       
    }

#endregion


    public void OnHit()
    {
        onDamaged.Invoke(this);
        timeLastHit = Time.time;

        if (invincible)
        {
            SetInvincibility(true);
        }

        if (regenerate)
        {
            SetRegeneration(false);
        }
        
    }

}
