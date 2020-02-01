using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    #region Variables
    [Header("Controller Configuration")]
    [Tooltip("Player's movement will be relative to the perspective of the Camera.")]
    public bool cameraRelMovement = false;
    [Tooltip("Player will constantly accelerate (move) in the direction it is facing.")]
    public bool constForwardMovement = false;
    [Tooltip("Controller will consider the rotation of the rotation target instead of the movement target's.")]
    public bool rotTargetRelMovement = false;
    [Tooltip("Set the rotation for the rotationTargets (X and Y) in this script.")]
    public bool controlRotations = true;
    [Tooltip("Variable value for input instead of binary on/off movement.")]
    public bool inputSensitive;
    [Tooltip("Allow user input to influence movement calculations.")]
    public bool canControl;
    [Range(0.01f, 2f)]
    public float groundCheckRadius = 0.5f;
    public LayerMask groundMask;
    [HideInInspector]
    public bool calculateMovement = true;
    [HideInInspector]
    public Vector3 overrideSpeed;
    public float bounceSpeed = 10f;
    public float bounceTime = 0.5f;
    [Space]
    [Header("Movement Configuration")]

    [Range(0, 25)]
    public float zAxialMaxSpeed = 10f;
    public float zAxialAccel = 1f;
    public float zAxialDecel = 1f;
    [Range(0, 25)]
    public float xAxialMaxSpeed = 10f;
    public float xAxialAccel = 1f;
    public float xAxialDecel = 1f;
    [Space]
    [Tooltip("Set to 0 if no gravity is desired.")]
    public float gravity = -9.81f;
    public bool allowJumping = true;
    public float jumpHeight;
    private Vector3 calculationDirection;
    [Space]

    [Header("Input Configuration")]
    [Range(0, 250)]
    public float rotateSpeed = 150f;

    [Space]
    [Header("References & Readouts")]
    public Transform movementTarget;
    public Transform rotationTargetY;
    public Transform rotationTargetX;
    public Transform groundCheck;
    public CharacterController m_characterController;
    public Camera gameCamera;

    [Space]
    public bool isGrounded = false;
    public bool jumping;
    public Vector3 velocity;

    [Space(5)]
    private Vector3 horizontalForward;
    public Vector2 movementInput;
    public Vector2 rawMovementInput;
    public Vector2 rotationInput;
    public Vector2 rotationValue = Vector2.zero;
    #endregion

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        if (m_characterController == null)
        {
            m_characterController = GetComponentInParent<CharacterController>();
        }
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        calculationDirection = new Vector3(1, 0, 1);
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        rawMovementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 movementValue = Vector3.zero;

        //Calculate Values (all scaled to Time.deltaTime already)
        movementValue = constForwardMovement == true ? CalculatePlanarMovement(new Vector2(0, 1), true) : CalculatePlanarMovement(rawMovementInput, false);

        //Combine Movement Values
        movementValue = new Vector3(movementValue.x, CalculateGravityAndJumpMovement(Input.GetButtonDown("Jump")), movementValue.z);

        //Apply Values
        if (m_characterController != null)
        {
            m_characterController.Move(movementValue);
        }

        if (controlRotations) //If control rotations from this script
        {
            //FPS Input
            rotationInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            //Calculate Values (all scaled to Time.deltaTime already)
            Vector3 rotVal = Vector3.zero;
            rotVal = CalculateRotation(rotationInput);

            //Apply Values
            rotationTargetY.localRotation = Quaternion.Euler(0, rotVal.y, 0f); //Rotation Y
            rotationTargetX.localRotation = Quaternion.Euler(rotVal.x, 0, 0); //Rotation X
        }
    }

    public Vector3 CalculateRotation(Vector2 controlInput)
    {
        //First Person Control
        Cursor.lockState = CursorLockMode.Locked;
        controlInput *= rotateSpeed * Time.deltaTime;
        Vector3 previousRotationValue = rotationValue;
        rotationValue.x -= controlInput.y;
        rotationValue.x = Mathf.Clamp(rotationValue.x, -90f, 90f);
        rotationValue.y += controlInput.x;

        return rotationValue;
    }

    public Vector3 CalculatePlanarMovement(Vector2 controlInput, bool constantForwardAccel)
    {
        movementInput = ProcessInput(controlInput);
        Vector3 displacement = Vector3.zero;
        if (constForwardMovement == true)
        {
            velocity.x = xAxialMaxSpeed;
            velocity.z = zAxialMaxSpeed;
            if (rotationTargetY != null && rotTargetRelMovement == true)
            {
                displacement = (rotationTargetY.forward * controlInput.y * velocity.z);
            }
            else
            {
                displacement = (transform.forward * controlInput.y * velocity.z);
            }
            displacement *= Time.deltaTime;
        }
        else
        {
            //* X Movement
            if (movementInput.x != 0) //Right or Left
            {
                calculationDirection.x = movementInput.x >= 0 ? 1 : -1;
                velocity.x = AccelerateInAxis(calculationDirection.x, xAxialMaxSpeed, xAxialAccel, ref velocity.x);
            }
            else //No input, decelerate
            {
                velocity.x = DecelerateInAxis(calculationDirection.x, xAxialMaxSpeed, xAxialDecel, ref velocity.x);
            }

            //* Z Movement
            if (movementInput.y != 0) //Forwards or Backwards
            {
                calculationDirection.z = movementInput.y >= 0 ? 1 : -1;
                velocity.z = AccelerateInAxis(calculationDirection.z, zAxialMaxSpeed, zAxialAccel, ref velocity.z);
            }
            else //No input, decelerate
            {
                velocity.z = DecelerateInAxis(calculationDirection.z, zAxialMaxSpeed, zAxialDecel, ref velocity.z);
            }

            if (cameraRelMovement == true)
            {
                if (gameCamera != null)
                {
                    Vector3 cameraForward = gameCamera.transform.forward;
                    Vector3 cameraRight = gameCamera.transform.right;

                    cameraForward.y = 0;
                    cameraRight.y = 0;

                    if (rotationTargetY != null)
                    {
                        horizontalForward = rotationTargetY.forward;
                        horizontalForward = new Vector3(horizontalForward.x, 0, horizontalForward.z);

                        Vector3 right = rotationTargetY.right;

                        displacement = (right * velocity.x) + (horizontalForward * velocity.z);
                        //displacement = (cam_R * velocity.x) + (cam_F * velocity.z);
                        displacement *= Time.deltaTime;
                    }
                }
                else
                {
                    Debug.LogWarning("No camera has been assigned for this player!", gameObject);
                }
            }
            else
            {
                if (rotTargetRelMovement == true && rotationTargetY != null)
                {
                    if (inputSensitive == true)
                    {
                        displacement = (rotationTargetY.right * velocity.x * movementInput.x) + (rotationTargetY.forward * velocity.z * movementInput.y);
                    }
                    else
                    {
                        displacement = (rotationTargetY.right * velocity.x) + (rotationTargetY.forward * velocity.z);
                    }
                    displacement *= Time.deltaTime;
                }
                else
                {
                    if (inputSensitive == true)
                    {
                        displacement = (transform.right * velocity.x * movementInput.x) + (transform.forward * velocity.z * movementInput.y);
                    }
                    else
                    {
                        displacement = (transform.right * velocity.x) + (transform.forward * velocity.z);
                    }
                    displacement *= Time.deltaTime;
                }
            }
        }

        //velocity.y *= Time.deltaTime;
        //displacement.y = velocity.y;

        if (calculateMovement == false && canControl == false)
        {
            overrideSpeed *= Time.deltaTime;
            velocity.z = 0;
            velocity.x = 0;
            return overrideSpeed;
        }
        else if (calculateMovement == false && canControl == true)
        {
            overrideSpeed += (rotationTargetY.forward * controlInput.y);
            overrideSpeed *= Time.deltaTime;
            return overrideSpeed;
        }
        else if (calculateMovement == true && canControl == true)
        {
            displacement.y = 0;
            return displacement;
        }
        return displacement;
    }

    public float CalculateGravityAndJumpMovement(bool jumped) //Y Movement Exclusively
    {
        //Gravity (Y Movement)
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = 0f;
                jumping = false;
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
            }
        }

        //Jumping
        if (allowJumping)
        {
            if (jumped && isGrounded)
            {
                //v = Sqrt(h x -2 x g)
                velocity.y = (Mathf.Sqrt(jumpHeight * -2f * gravity));
                jumping = true;
            }
        }

        if (jumping)
        {
            if ((m_characterController.collisionFlags & CollisionFlags.Above) != 0) //Hit head
            {
                velocity.y = 0;
                jumping = false;
            }
        }
        return velocity.y * Time.deltaTime;
    }

    private Vector2 ProcessInput(Vector2 input)
    {
        Vector2 processedMovementValue = input;
        if (Mathf.Abs(processedMovementValue.x) < 0.4f)
        {
            processedMovementValue.x = 0;
        }

        if (Mathf.Abs(processedMovementValue.y) < 0.4f)
        {
            processedMovementValue.y = 0;
        }
        return new Vector2(processedMovementValue.x != 0 ? Mathf.Sign(processedMovementValue.x) : 0, processedMovementValue.y != 0 ? Mathf.Sign(processedMovementValue.y) : 0);
    }

    // IEnumerator RotateBy(float amount, float lerpTime) //Rotate smoothly by amount over lerpTime
    // {
    //     float initialRotation = Mathf.FloorToInt(transform.eulerAngles.y);
    //     float targetRotation = Mathf.FloorToInt(transform.eulerAngles.y + amount);
    //     for (float t = 0; t <= 1; t += 1 / (lerpTime / Time.deltaTime))
    //     {
    //         transform.eulerAngles = new Vector3(0, Mathf.Lerp(initialRotation, targetRotation, t), 0);
    //         yield return null;
    //     }
    //     transform.eulerAngles = new Vector3(0, targetRotation, 0);
    // }

    #region Acceleration / Deceleration

    public float AccelerateInAxis(float input, float maxVelocity, float accelerationTime, ref float target)
    {
        float acceleration = (maxVelocity / accelerationTime) * Time.deltaTime;
        float newVelocity = Mathf.Clamp(target + (acceleration * Mathf.Sign(input)), -maxVelocity, maxVelocity);
        return newVelocity;
    }

    public float DecelerateInAxis(float input, float maxVelocity, float decelerationTime, ref float target)
    {
        if (target == 0)
        {
            return 0;
        }

        //Calculate Deceleration
        float deceleration = (maxVelocity / decelerationTime) * Time.deltaTime;
        //Apply the deceleration to the current velocity in the opposite direction that the input is
        float newVelocity = Mathf.Clamp(target + (deceleration * -Mathf.Sign(input)), -maxVelocity, maxVelocity);

        //If decelerating past 0, set velocity to 0. 
        if (Mathf.Sign(newVelocity) == -Mathf.Sign(input))
        {
            newVelocity = 0;
        }

        return newVelocity;
    }

    public void Bounce(Vector3 sourceDirection)
    {
        SetControl(false);
        Vector3 pushDirection;
        pushDirection = transform.position - sourceDirection;
        StartCoroutine(BounceAway(pushDirection, bounceTime));
    }

    public IEnumerator BounceAway(Vector3 direction, float duration)
    {
        float timer = 0;
        while (timer < bounceTime)
        {
            timer += bounceTime;
            m_characterController.Move(direction * bounceSpeed);
            yield return new WaitForEndOfFrame();
        }
        SetControl(true);
    }

    #endregion

    #region Stunning

    public void StunPlayer()
    {
        StunPlayer(1f);
    }

    public void StunPlayer(float duration)
    {
        SetControl(false);
        Invoke("UnstunPlayer", duration);
    }

    public void UnstunPlayer()
    {
        SetControl(true);
    }

    public void SetControl(bool value)
    {
        canControl = value;
    }

    #endregion

    private void OnDrawGizmos()
    {
        // if (rotationTargetY != null)
        // {
        //     Debug.DrawRay(transform.position, rotationTargetY.forward * 10f, Color.magenta);
        //     //Debug.DrawRay(transform.position, horizontalForward * 10f, Color.yellow);
        // }

        if (groundCheck != null)
        {
            if (isGrounded)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            }
        }
    }
}
