using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{   
    public Player m_Player;
    public Animator animator;
    public GameObject modelGameObject;
    
    // Start is called before the first frame update
    void Start()
    {
        if (modelGameObject == null)
        {
            modelGameObject = gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_Player != null)
        {
            SetMoving(m_Player.m_playerController);
            SetJumping(m_Player.m_playerController);
            SetCarrying(m_Player.m_HandController);
        }
    }

#region Walking
    public void SetMoving(PlayerController playerController)
    {
        if (animator != null)
        {
            if (playerController != null)
            {
                Vector2 vel = new Vector2(playerController.velocity.x, playerController.velocity.z);
                bool value = vel.magnitude == 0 ? false : true;
                animator.SetBool("Moving", value);
            }
            
        }
    }

#endregion

#region Jumping
    public bool SetJumping(PlayerController playerController)
    {
        if (animator != null)
        {
            if (playerController != null)
            {
                animator.SetBool("Jump", playerController.jumping);
                return playerController.jumping;
            }
        }
        return false;
    }

    public bool SetJumpingFalse(PlayerController playerController)
    {
        if (animator != null)
        {
            if (playerController != null)
            {
                animator.SetBool("Jump", false);
                return false;
            }
        }
        return false;
    }

    // public IEnumerator JumpingState(PlayerController playerController)
    // {
    //     if (playerController != null)
    //     {
    //         for(;;)
    //         {
    //             if (SetJumping(playerController) == true)
    //             {
    //                 yield return new WaitForSeconds(0.25f);
    //                 SetJumpingFalse(playerController);
    //             }
    //             yield return new WaitForSeconds(0.25f);
    //         }
    //     }
    // }

#endregion

#region Hurt
    public void SetHurtTrue(Health hp)
    {
        if (animator != null)
        {
            animator.SetBool("Hurt", true);
            StartCoroutine(SetHurt());
        }
    }

    public void SetHurtFalse(Health hp)
    {
        if (animator != null)
        {
            animator.SetBool("Hurt", false);
        }
    }

    public IEnumerator SetHurt()
    {
        yield return new WaitForSeconds(0.25f);
        animator.SetBool("Hurt", false);
    }

#endregion

#region Alive
    public void SetAliveTrue(Health hp)
    {
        if (animator != null)
        {
            animator.SetBool("Alive", true);
        }
    }

    public void SetAliveFalse(Health hp)
    {
        if (animator != null)
        {
            animator.SetBool("Alive", false);
        }
    }
#endregion

#region Carrying
    public bool SetCarrying(HandController handController)
    {
        if (animator != null)
        {
            if (handController != null)
            {
                bool value = handController.heldItem == null ? false : true;
                animator.SetBool("Carrying", value);
                return value;
            }
        }
        return false;
    }

    public bool SetCarryingFalse(HandController handController)
    {
        if (animator != null)
        {
            if (handController != null)
            {
                animator.SetBool("Carrying", false);
                return false;
            }
        }
        return false;
    }

#endregion

}
