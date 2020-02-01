using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{

    // Responsible for picking up and dropping items
    [Header("Object Info")]
    public GameObject heldItem;
    public Transform heldPosition;

    [Space]
    [Header("Raycast Properties")]
    public float raycastRange = 1;
    public LayerMask mask; 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, raycastRange, mask.value))
            {
                if (hit.collider.gameObject != null && transform.parent != hit.collider.gameObject.transform.parent) //If object isn't null and they don't share same root
                {
                    HoldItem(hit.collider.gameObject);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            DropItem();
        }
    }

    public void HoldItem(GameObject item)
    {
        if (heldItem != null) //Item already in hand
        {
            DropItem();
        }

        
        if (heldPosition != null)
        {
            item.transform.parent = transform.parent;
            heldItem = item;
            heldItem.transform.position = heldPosition.position;
            heldItem.transform.rotation = heldPosition.rotation;
            
            Rigidbody itemRB = heldItem.GetComponentInChildren<Rigidbody>();

            if (itemRB != null)
            {
                itemRB.isKinematic = true;
            }
        }
    }

    public void DropItem()
    {
        if (heldItem != null)
        {
            Rigidbody itemRB = heldItem.GetComponentInChildren<Rigidbody>();

            if (itemRB != null)
            {
                itemRB.isKinematic = false;
            }

            heldItem.transform.SetParent(null, true);
            heldItem = null;
        }
    }

    private void OnDrawGizmos() 
    {
        Debug.DrawRay(transform.position, transform.forward*raycastRange, Color.red);
    }
}
