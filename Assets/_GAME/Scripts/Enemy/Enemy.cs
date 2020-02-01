using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IKillable
{
    private bool alive;

    public AIManager aiManager;
    public GameObject target;

    private NavMeshAgent agent;

    private void Awake()
    {
        aiManager = GameObject.Find("Singletons").GetComponent<AIManager>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        aiManager = GameObject.Find("Singletons").GetComponent<AIManager>();
        target = GameObject.Find("Player");
    }

    private void OnEnable()
    {

        target = GameObject.Find("Player");
        alive = true;
        aiManager.AddToList(this.gameObject);
        StartCoroutine(UpdatePosition(0.15f));
    }

    private void OnDisable()
    {
        aiManager.RemoveFromList(this.gameObject);

        StopAllCoroutines();
    }

    private void Update()
    {

    }

    public void Kill()
    {
        if (aiManager.CheckisInList(this.gameObject))
        {
            aiManager.RemoveFromList(this.gameObject);
        }
        gameObject.SetActive(false);
    }


    private IEnumerator UpdatePosition(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            NavMeshHit hit;

            if(NavMesh.SamplePosition(target.transform.position, out hit, 1f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
}
