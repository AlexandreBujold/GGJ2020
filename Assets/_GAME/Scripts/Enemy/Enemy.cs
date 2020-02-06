using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IKillable
{
    private bool alive;

    public AIManager aiManager;
    public RepairObjective repairObjective;

    public List<Player> players;
    public GameObject target;

    [SerializeField] private GameObject batteryPrefab;

    [SerializeField] private NavMeshAgent agent;

    Vector3 testPos;

    public Animator animator;
    public GameObject effect;

    private void Awake()
    {
        aiManager = GameObject.Find("Singletons").GetComponent<AIManager>();
        agent = GetComponent<NavMeshAgent>();
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
        GenerateBattery();
        if (aiManager.CheckisInList(this.gameObject))
        {
            aiManager.RemoveFromList(this.gameObject);
        }
        FMODUnity.RuntimeManager.PlayOneShot("event:/SoundFX/Zombo/ZomboDeath", transform.position);
        if (effect != null)
        {
            Instantiate(effect, transform.position, Quaternion.identity);
            FMODUnity.RuntimeManager.PlayOneShot("event:/SoundFX/Zombo/GibletBounce", transform.position);
        }
        gameObject.SetActive(false);
    }

    private GameObject GetInitialTarget()
    {
        if (players != null && players.Count > 0)
        {
            int randomPlayer = 0;
            if (players.Count > 1)
            {
                randomPlayer = Random.Range(0, players.Count);
            }
            return players[randomPlayer].gameObject;
        }
        return this.gameObject;
    }

    private void GenerateBattery()
    {
        float random = Random.value;
        if(random < 0.05)
        {
            Instantiate(batteryPrefab, transform.position, Quaternion.identity);
        }
    }


    private IEnumerator UpdatePosition(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            NavMeshHit hit;
            int groundMask = 1 << NavMesh.GetAreaFromName("Walkable");

            if (target != null)
            {
                if(NavMesh.SamplePosition(target.transform.position, out hit, 5f, groundMask))
                {
                    agent.SetDestination(hit.position);
                    testPos = hit.position;
                }
            }
            
            if (Random.value <= 0.02f)
            {
                FMODUnity.RuntimeManager.PlayOneShot("event:/SoundFX/Zombo/Groan", transform.position);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (target != null)
        {
            if (other.transform.gameObject.name == "Collider")
            {
                if (other.transform.parent.transform.gameObject == target)
                {
                    if (animator != null)
                    {
                        animator.SetBool("Attack", true);
                        StartCoroutine(SetAttackFalse());
                    }

                    target.gameObject.GetComponent<Health>().Damage(1f);
                }
            }
        }
    }

    public IEnumerator SetAttackFalse()
    {
        yield return new WaitForSeconds(0.25f);
        animator.SetBool("Attack", false);
    }



    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(testPos, 1f);
        // Gizmos.color = Color.blue;
        // Gizmos.DrawWireSphere(target.transform.position, 1f);
    }
}
