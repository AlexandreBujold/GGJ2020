using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public static AIManager instance = null;

    [SerializeField] private WaveManager waveManager;

    [SerializeField] private List<GameObject> agentList;

    public int maxSpawnedAgents; //Maximum agents that can be spawned at any time, independent from the wave limit
    public int agentsPerWave; //Max agents that can be spawned in a single wave
    private int totalSpawnedAgents = 0; //Keeps track of all agents spawned during a wave

    public bool spawnAgents = false;

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
        agentList = new List<GameObject>();
    }

    private void Update()
    {
        if (spawnAgents)
        {
            ValidateSpawn();
            Debug.Log("Begin Spawning");
        }
    }

    void ValidateSpawn() //Method that spawns AI agents. Will only spawn if valid conditionals
    {
        if (totalSpawnedAgents < agentsPerWave) //If number of agents spawned exceeds the wave limit
        {
            foreach (GameObject agent in waveManager.pooledAgents) //Goes through list
            {
                if (totalSpawnedAgents >= agentsPerWave) //If spawned agents exceeds wave limit, exit loop
                {
                    spawnAgents = false;
                    break;
                }

                if (agentList.Count >= maxSpawnedAgents) //If the spawn list exceeds max amount of enemies existing at any one time, exit loop
                {
                    break;
                }

                /*FOR FUTURE DEVELOPMENT
                 *
                 *Decide what type of enemy to spawn here. This will be based off of a predetermined list of enemies made by the WaveManager (Maybe)
                 */

                if (!agentList.Contains(agent)) //Sees if object in question is on the agent list. If not, it is added and enabled
                {
                    /*FOR FUTURE DEVELOPMENT
                     * 
                     * This is where individual spawning algorithms will be called. Each enemy spawns in a different location based 
                     * on their type. GunPotatos rise up out of the ground, meanwhile other enemies may spawn in dedicated spawn zones.
                     * For now, this portion of the code will only spawn GunPotatos
                     */
                    agentList.Add(agent);
                    agent.SetActive(true);
                    totalSpawnedAgents++;
                }
            }
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
    #endregion

    public void StartWave(int numberOfAgents)
    {
        GetWaveParameters();
        spawnAgents = true;
        totalSpawnedAgents = 0;
    }

    private void GetWaveParameters()
    {
        maxSpawnedAgents = waveManager.MaxSpawnedAgents;
        agentsPerWave = waveManager.WaveStrength;
    }

}
