using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GamepadInput;

public class HandController : MonoBehaviour
{

    // Responsible for picking up and dropping items
    public GamePad.Index controllerIndex;
    [Space]
    [Header("Object Info")]
    //public GameObject myCamera;
    public GameObject heldItem;
    public Transform holdTransform;

    [Space]
    [Header("Raycast Properties")]
    public float raycastRange = 1;
    public LayerMask mask;

    [Space]
    [Header("Parent")]
    public Player parentPlayer;

    // Start is called before the first frame update
    void Start()
    {
        parentPlayer = GetComponentInParent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GamePad.GetButtonDown(GamePad.Button.X, controllerIndex))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, raycastRange, mask.value))
            {
                if (hit.collider.gameObject != null && transform.parent != hit.collider.gameObject.transform.parent) //If object isn't null and they don't share same root
                {
                    HoldItem(hit.collider.gameObject);
                }
            }
            else
            {
                DropItem();
            }
        }

        if (GamePad.GetButtonDown(GamePad.Button.LeftShoulder, controllerIndex) || GamePad.GetButtonDown(GamePad.Button.RightShoulder, controllerIndex) || GamePad.GetTrigger(GamePad.Trigger.LeftTrigger, controllerIndex, false) != 0 || GamePad.GetTrigger(GamePad.Trigger.RightTrigger, controllerIndex, false) != 0)
        {
            ActivateHeldItem();
        }
    }

    public void HoldItem(GameObject item)
    {
        if (heldItem != null) //Item already in hand
        {
            DropItem();
        }

        
        if (holdTransform != null)
        {
            heldItem = item;
            heldItem.transform.position = holdTransform.position;
            heldItem.transform.forward = transform.forward;
            item.transform.parent = transform.parent;
            heldItem.transform.localRotation = holdTransform.localRotation;
            
            Rigidbody itemRB = heldItem.GetComponentInChildren<Rigidbody>();

            if (itemRB != null)
            {
                itemRB.isKinematic = true;
            }
        }

        ShockShot shot = heldItem.GetComponentInChildren<ShockShot>();
        if (shot != null)
        {
            shot.drawGizmos = true;
            parentPlayer.m_gun = shot;
            FMODUnity.RuntimeManager.PlayOneShot("event:/SoundFX/Player/Pickup", transform.position);
        }
        else
        {
            FMODUnity.RuntimeManager.PlayOneShot("event:/SoundFX/Player/Landing", transform.position);
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

            ShockShot shot = heldItem.GetComponentInChildren<ShockShot>();
            if (shot != null)
            {
                shot.drawGizmos = false;
                parentPlayer.m_gun = null;
            }

            heldItem.transform.SetParent(null, true);
            heldItem = null;
        }
    }

    public void ActivateHeldItem()
    {
        if (heldItem != null)
        {
            IActivatable item = heldItem.GetComponentInChildren<IActivatable>();
            if (item != null)
            {
                item.Activate();
            }
        }
    }
    
    private void OnDrawGizmos() 
    {
        Debug.DrawRay(transform.position, transform.forward*raycastRange, Color.red);
    }
}
