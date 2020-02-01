using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairObjective : MonoBehaviour
{
    private WaveManager waveManager;

    [SerializeField] private List<GameObject> killableZombies;

    private SphereCollider shockCollider;

    public LayerMask zombieLayer;

    [SerializeField] private float shockRadius;
    [SerializeField] private int activationCost;
    [SerializeField] private int depositedMaterialCount;

    private void Awake()
    {
        waveManager = GameObject.Find("Singletons").GetComponent<WaveManager>();
        killableZombies = new List<GameObject>();
        shockRadius = 5f;
        activationCost = 1;
    }

    void Start()
    {
        shockCollider = GetComponent<SphereCollider>();
        shockCollider.radius = shockRadius;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DepositMaterial();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, shockRadius);
    }

    private void DepositMaterial()
    {
        depositedMaterialCount++;
        if(depositedMaterialCount >= activationCost)
        {
            depositedMaterialCount = 0;
            activationCost++;
            Shock();
        }
    }

    private void Shock()
    {
        waveManager.ReturnAgentsToPool(killableZombies);
        killableZombies.Clear();
        shockRadius += 5f;
        shockCollider.radius = shockRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            if (!killableZombies.Contains(other.gameObject))
            {
                killableZombies.Add(other.gameObject);
                Debug.Log(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(killableZombies.Contains(other.gameObject))
        {
            killableZombies.Remove(other.gameObject);
        }

    }
}
