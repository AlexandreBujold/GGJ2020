using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : MonoBehaviour
{
    public string tagToDepositOn = "Objective";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag(tagToDepositOn))
        {
            RepairObjective repairObj = other.gameObject.GetComponentInParent<RepairObjective>();
            if (repairObj != null)
            {
                repairObj.DepositMaterial();
                Destroy(gameObject);
            }
        }
    }
}
