using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairObjective : MonoBehaviour
{
    private WaveManager waveManager;

    [SerializeField] private List<GameObject> killableZombies;
    [SerializeField] private List<Transform> teleportPositions;

    private GameObject positions;

    private SphereCollider shockCollider;

    public LayerMask zombieLayer;

    public float delay = 1.5f;

    [SerializeField] private float shockRadius;
    [SerializeField] private int activationCost;
    [SerializeField] private int depositedMaterialCount;

    [SerializeField] private int currentTeleIndex = 0;

    private void Awake()
    {
        positions = GameObject.Find("ObjectiveLocations");
        foreach(Transform child in positions.transform)
        {
            teleportPositions.Add(child);
        }
        waveManager = GameObject.Find("Singletons").GetComponent<WaveManager>();
        killableZombies = new List<GameObject>();
        shockRadius = 5f;
        activationCost = 1;
    }

    void Start()
    {
        transform.position = RandomizePosition();
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

    public void DepositMaterial()
    {
        depositedMaterialCount++;
        if(depositedMaterialCount >= activationCost)
        {
            depositedMaterialCount = 0;
            activationCost++;
            Debug.Log("START CHARGE");
            StartCoroutine(ChargeUp(delay));
            //Shock();
        }
    }

    private IEnumerator ChargeUp(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Debug.Log("KABOOM");
        Shock();
    }

    private IEnumerator Teleport(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        transform.position = RandomizePosition();
    }

    private void Shock()
    {
        waveManager.ReturnAgentsToPool(killableZombies);
        killableZombies.Clear();
        shockRadius += 5f;
        shockCollider.radius = shockRadius;
        ReloadAllGuns();
        StartCoroutine(Teleport(delay));
    }

    private Vector3 RandomizePosition()
    {
        int randomIndex = Random.Range(0, teleportPositions.Count);
        while(currentTeleIndex == randomIndex)
        {
            randomIndex = Random.Range(0, teleportPositions.Count);
        }
        currentTeleIndex = randomIndex;

        return teleportPositions[randomIndex].transform.position;
    }

    private void ReloadAllGuns()
    {
        ShockShot[] gunList = FindObjectsOfType<ShockShot>();
        if(gunList == null)
        {
            return;
        }
        foreach(ShockShot gun in gunList)
        {
            gun.UpdateAmmo();
        }
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
