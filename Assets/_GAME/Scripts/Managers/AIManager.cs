using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;

public class AIManager : MonoBehaviour
{
    public static AIManager instance = null;

    [Header("Wave Manager and Agent List")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private List<GameObject> agentList;

    [Space]
    [Header("Spawn Zones")]
    [SerializeField] private GameObject zoneParent;
    [SerializeField] private List<GameObject> spawnZoneList;

    [Space]
    [Header("Wave integers")]
    public int waveNumber;
    public int maxSpawnedAgents; //Maximum agents that can be spawned at any time, independent from the wave limit
    public int agentsPerWave; //Max agents that can be spawned in a single wave
    [SerializeField] private int agentsSpawnedInWave = 0; //Keeps track of all agents spawned during a wave
    [SerializeField] private int waveCooldown = 10;

    [Space]
    [Header("Wave bools")]
    public bool spawnAgents = true;
    public bool disableSpawnTemp = false;

    private void Awake()
    {
        /////Singleton initialization/////
        DontDestroyOnLoad(this.gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }
        /////Singleton initialization/////

        waveManager = GetComponent<WaveManager>();
        zoneParent = GameObject.Find("SpawnZones");
        foreach(Transform child in zoneParent.transform)
        {
            spawnZoneList.Add(child.gameObject);
        }
        agentList = new List<GameObject>();
    }

    private void Start()
    {
        Debug.Log("Game start");
        StartCoroutine(WaveCooldown(waveCooldown));
    }

    private void Update()
    {
        if (spawnAgents)
        {
            if (!disableSpawnTemp)
            {
                ValidateSpawn();
            }
            else
            {
                CheckForAgentDeath();
            }
        }
    }

    void ValidateSpawn() //Method that spawns AI agents. Will only spawn if valid conditionals
    {
        if (agentsSpawnedInWave >= agentsPerWave)
        {
            if (agentList.Count <= 0)
            {
                StartCoroutine(WaveCooldown(waveCooldown));
                spawnAgents = false;
                Debug.Log("This works");
                waveManager.waveComplete = true;
                
            }
        }

        else if (agentsSpawnedInWave < agentsPerWave) //If number of agents spawned exceeds the wave limit
        {
            foreach (GameObject agent in waveManager.pooledAgents) //Goes through list
            {
                if (agentList.Count >= maxSpawnedAgents) //If the spawn list exceeds max amount of enemies existing at any one time, exit loop
                {
                    disableSpawnTemp = true;
                    Debug.Log("Spawning temporarily disabled");
                    break;
                }

                if (agentsSpawnedInWave >= agentsPerWave) //If spawned agents exceeds wave limit, exit loop
                {
                    spawnAgents = false;
                    break;
                }

                if (!agentList.Contains(agent)) //Sees if object in question is on the agent list. If not, it is added, enabled, and spawned according to the spawn algorithm
                {
                    agentList.Add(agent);
                    SetAgentSpawnPosition(agent);
                    agent.SetActive(true);
                    agentsSpawnedInWave++;
                }
            }
        }
    }

    void CheckForAgentDeath()
    {
        if(agentList.Count < maxSpawnedAgents)
        {
            disableSpawnTemp = false;
        }
    }

    void SetAgentSpawnPosition(GameObject agent)
    {
        agent.transform.position = GenerateSpawnPoint(spawnZoneList.Count);
    }

    private Vector3 GenerateSpawnPoint(int spawnerMax)
    {
        int spawnerToUse = Random.Range(0, spawnerMax);

        GameObject selectedSpawner = spawnZoneList[spawnerToUse];
        Debug.Log(selectedSpawner);
        ZombieSpawnZone spawnerScript = selectedSpawner.GetComponent<ZombieSpawnZone>();
        Vector3 spawnerPosition = selectedSpawner.transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(spawnerPosition + (Random.insideUnitSphere * spawnerScript.zoneRadius), out hit, spawnerScript.zoneRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return Vector3.zero;
        }
    }

    #region ListMethods
    public void AddToList(GameObject agent) //Adds the agent in question to the list of active agents
    {
        if(!agentList.Contains(agent))
        {
            agentList.Add(agent);
        }
    }

    public void RemoveFromList(GameObject agent) //Adds the agent in question to the list of active agents
    {
        if(agentList.Contains(agent))
        {
            agentList.Remove(agent);
        }
    }

    public bool CheckisInList(GameObject agent)
    {
        if (agentList.Contains(agent))
        {
            return true;
        }
        else
            return false;
    }

    //public List<GameObject> RetrieveList()
    //{
    //    return agentList;
    //}
    #endregion

    #region WaveMethods
    public void StartWave()
    {
        GetWaveParameters();
        spawnAgents = true;
        agentsSpawnedInWave = 0;
    }

    public IEnumerator WaveCooldown(int waitTime)
    {
        Debug.Log("Wave Ended");
        yield return new WaitForSeconds(waitTime);
        Debug.Log("Cooldown over");
        StartWave();
    }

    private void GetWaveParameters()
    {
        waveNumber = waveManager.WaveNumber;
        maxSpawnedAgents = waveManager.MaxSpawnedAgents;
        agentsPerWave = waveManager.WaveStrength;
    }
    #endregion

}
