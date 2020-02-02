using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IKillable
{
    private bool alive;

    public AIManager aiManager;
    public RepairObjective repairObjective;
    public GameObject player1;
    public GameObject player2;
    public GameObject target;

    [SerializeField] private NavMeshAgent agent;

    Vector3 testPos;

    private void Awake()
    {
        aiManager = GameObject.Find("Singletons").GetComponent<AIManager>();
        agent = GetComponent<NavMeshAgent>();
        player1 = GameObject.Find("Player 1");
        player2 = GameObject.Find("Player 2");
    }

    private void Start()
    {
        aiManager = GameObject.Find("Singletons").GetComponent<AIManager>();
    }

    private void OnEnable()
    {

        target = GetInitialTarget();
        alive = true;
        aiManager.AddToList(this.gameObject);
        //repairObjective.AddToList(this.gameObject);
        StartCoroutine(UpdatePosition(0.15f));
    }

    private void OnDisable()
    {
        aiManager.RemoveFromList(this.gameObject);
        //repairObjective.RemoveFromList(this.gameObject);
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

    private GameObject GetInitialTarget()
    {
        float randomPlayer = Random.Range(0, 2);

        if (randomPlayer == 0)
        {
            return player1;
        }
        else
        {
            return player2;
        }
    }


    private IEnumerator UpdatePosition(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            NavMeshHit hit;
            int groundMask = 1 << NavMesh.GetAreaFromName("Walkable");

            if(NavMesh.SamplePosition(target.transform.position, out hit, 5f, groundMask))
            {
                agent.SetDestination(hit.position);
                testPos = hit.position;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target != null)
        {
            if (other.transform.parent.transform.gameObject == target)
            {
                target.gameObject.GetComponent<Health>().Damage(1f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(testPos, 1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(target.transform.position, 1f);
    }
}
