using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{

    public static WaveManager instance;

    public List<GameObject> pooledAgents;

    [SerializeField] private GameObject agentParent; //Object to sort all agents under. Object should exist at Vector3.zero

    [SerializeField] private GameObject zombieBearPrefab;

    public int WaveNumber { get; protected set; } //Current wave
    public int MaxSpawnedAgents { get; protected set; } //Max enemies on screen at any time
    public int WaveStrength { get; protected set; } //Max enemies per wave

    public bool waveComplete;

    private void Awake()
    {
        /////Singleton Initialization/////
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != null)
        {
            Destroy(gameObject);
        }
        /////Singleton Initialization/////

        agentParent = GameObject.Find("PooledAgents");
        if(agentParent.transform.position != Vector3.zero) //Resets agentParent to Vector3.zero if it has been moved
        {
            agentParent.transform.position = Vector3.zero;
        }

        WaveNumber = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        CreateWaveParameters();
        Debug.Log(WaveStrength);
    }

    void Update() //Used to create a number of agents to be pooled. This will be replaced with a spawning algorithm that spawns appropriate AI enemy types
    {
        if(pooledAgents.Count <= MaxSpawnedAgents)
        {
            int difference = MaxSpawnedAgents - pooledAgents.Count; //Checks the difference between the number of agents in the current wave to the actual number of pooled agents

            for(int i = 0; i < difference + 5; i++) //Adds any missing agents to the pool
            {
                GameObject newAgent = Instantiate(zombieBearPrefab, Vector3.zero, Quaternion.identity, agentParent.transform);
                pooledAgents.Add(newAgent);
                newAgent.SetActive(false);
            }
        }

        ////DEBUG STATEMENT////
        if(waveComplete)
        {
            CreateWaveParameters();
            Debug.Log("ADVANCED WAVE WITH " + WaveNumber + " " + WaveStrength + " " + MaxSpawnedAgents);
        }
        ////DEBUG STATEMENT////
    }

    void CreateWaveParameters()
    {
        WaveNumber++;
        WaveStrength = 5 + (WaveNumber * 8) + ((int)Mathf.Sqrt(WaveNumber * 8));
        MaxSpawnedAgents = 10 + (WaveNumber * 5);
        waveComplete = false;
    }
}
