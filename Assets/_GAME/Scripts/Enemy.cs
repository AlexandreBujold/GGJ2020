using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IKillable
{
    private bool alive;

    public AIManager aiManager;
    public GameObject target;

    private void Awake()
    {
        aiManager = GameObject.Find("Singletons").GetComponent<AIManager>();
    }

    private void Start()
    {
        aiManager = GameObject.Find("Singletons").GetComponent<AIManager>();
    }

    private void OnEnable()
    {
        alive = true;
        aiManager.AddToList(this.gameObject);
    }

    private void OnDisable()
    {
        aiManager.RemoveFromList(this.gameObject);
    }

    public virtual void Kill()
    {
        if (aiManager.CheckisInList(this.gameObject))
        {
            aiManager.RemoveFromList(this.gameObject);
        }
        gameObject.SetActive(false);
    }


}
