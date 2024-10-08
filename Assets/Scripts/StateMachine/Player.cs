using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region Objects
    [Header("Objects")]
    public GameObject Weapon;
    public GameObject WeaponBack;
    public Transform cam;
    public AudioSource footSteps;
    public Rigidbody rb;
    public Animator animator;
    #endregion

    private Vector2 direction;
    public bool grounded;
    public Vector3 moveDirection;
    public float ySpeed;
    public bool gettingReadyToJump;
    public bool isEquipping;
    public bool isEquipped;

    float velocityZ = 0.0f;
    float velocityX = 0.0f;

    #region Movement Variables
    [Header("Movement Variables")]
    public float moveSpeed;
    public float runSpeed;
    public float acceleration = 2.0f;
    public float deceleration = 3.0f;
    public float maximumWalkVelocity = 0.5f;
    public float maximumRunVelocity = 2.0f;
    public float rotationSpeed = 600f;
    #endregion

    #region Melee Variables
    [Header("Melee Variables")]
    public float cooldownTime = 4f;
    public static int noOfClicks = 0;
    private float nextFireTime = 0f;
    float lastClickedTime = 0f;
    float maxComboDelay = 1f;
    #endregion

    #region Physics Variables
    [Header("Physics Variable")]
    public float groundDrag;
    public float airDrag;
    public float playerHeight;
    #endregion

    public LayerMask isGround;
    public int CameraMode = 1;

    public float minimumFootstepPitch = 0.5f;
    public float maximumFootstepPitch = 1f;
    public bool CanMove;
    public bool CanJump;

    public bool isRunning = false;

    #region State Machine Variables
    public PlayerStateMachine PlayerStateMachine { get; set; }
    public PlayerIdleState PlayerIdleState { get; set; }
    public PlayerJumpState PlayerJumpState { get; set; }
    public PlayerRollingState PlayerRollingState { get; set; }
    #endregion

    private void Awake()
    {
        PlayerStateMachine = new PlayerStateMachine();

        PlayerIdleState = new PlayerIdleState(this, PlayerStateMachine);
        PlayerJumpState = new PlayerJumpState(this, PlayerStateMachine);
        PlayerRollingState = new PlayerRollingState(this, PlayerStateMachine);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        PlayerStateMachine.Initalize(PlayerIdleState);
    }

    // Update is called once per frame
    void Update()
    {
        PlayerStateMachine.CurrentState.FrameUpdate();

        // Reset the combo if the delay is exceeded
        if (Time.time - lastClickedTime > maxComboDelay)
        {
            noOfClicks = 0;
        }

        ySpeed += Physics.gravity.y * Time.deltaTime;

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.1f, isGround);

        if (grounded)
        {
            if (!gettingReadyToJump)
            {
                animator.SetBool("Landing", true);
                animator.SetBool("IsFalling", false);
                rb.drag = groundDrag;
            }
            ySpeed = -0.5f;
        }
        else
        {
            animator.SetBool("Landing", false);
            rb.drag = airDrag;

            if (ySpeed < -1f) // Falling
            {
                animator.SetBool("IsFalling", true);
            }
            else
            {
                animator.SetBool("IsFalling", false);
            }
        }

        CheckDirection();
        UpdateCamera();

        // Reset animations when they are done
        ResetComboState("Slash1");
        ResetComboState("Slash2");
        ResetComboState("Slash3");
    }

    public void Equipped()
    {
        isEquipping = false;
    }

    public void Equip(InputAction.CallbackContext context)
    {
        if (!gettingReadyToJump && grounded && context.performed && PlayerStateMachine.CurrentState == PlayerIdleState)
        {
            //Equip
            if (!isEquipped) 
            {
                isEquipping = true;
                animator.SetTrigger("Equip");
            }
            //Unequip
            else if (isEquipped) 
            {
                isEquipping = true;
                animator.SetTrigger("Unequip");
            }
        }
    }

    public void ActivateWeapon()
    {
        if (!isEquipped) //Equpping
        {
            Weapon.SetActive(true);
            WeaponBack.SetActive(false);
        }
        else
        {
            Weapon.SetActive(false);
            WeaponBack.SetActive(true);
        }
        isEquipped = !isEquipped;
        animator.SetBool("IsEquipped", isEquipped);
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && isEquipped && Time.time - nextFireTime > cooldownTime)
        {

            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(2);

            if (noOfClicks == 0 || (noOfClicks == 1 && currentState.IsName("Slash1") && currentState.normalizedTime > 0.5f) || (noOfClicks == 2 && currentState.IsName("Slash2") && currentState.normalizedTime > 0.5f))
            {
                lastClickedTime = Time.time;
                noOfClicks++;
            }

            if (noOfClicks == 1)
            {
                animator.SetBool("Slash3", false);
                animator.SetBool("Slash2", false);
                animator.SetBool("Slash1", true);
            }
            else if (noOfClicks == 2 && currentState.IsName("Slash1") && currentState.normalizedTime > 0.5f)
            {
                animator.SetBool("Slash2", true);
                animator.SetBool("Slash1", false);
                animator.SetBool("Slash3", false);
            }
            else if (noOfClicks == 3 && currentState.IsName("Slash2") && currentState.normalizedTime > 0.5f)
            {
                animator.SetBool("Slash3", true);
                animator.SetBool("Slash2", false);
                animator.SetBool("Slash1", false);
                // Reset the combo after the final hit
                noOfClicks = 0;
                // Start cooldown after the combo
                nextFireTime = Time.time + cooldownTime;
                Debug.Log(nextFireTime);
            }

            // Clamp the number of clicks to prevent overflow
            noOfClicks = Mathf.Clamp(noOfClicks, 0, 3);

        }
    }

    private void ResetComboState(string stateName)
    {
        if (animator.GetCurrentAnimatorStateInfo(2).IsName(stateName) && animator.GetCurrentAnimatorStateInfo(2).normalizedTime > 0.7f)
        {
            animator.SetBool(stateName, false);
        }
    }

    private void FixedUpdate()
    {
        PlayerStateMachine.CurrentState.PhysicsUpdate();

        if (!grounded || PlayerStateMachine.CurrentState == PlayerIdleState)
        {
            if (!isRunning)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Force);
            }
            else
            {
                rb.AddForce(moveDirection.normalized * runSpeed, ForceMode.Force);
            }
        }
        
    }

    public void MoveAction(InputAction.CallbackContext context)
    {
        direction = context.ReadValue<Vector2>(); //Update Vector2 Direction
    }

    public bool CanRollCollisionCheck()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward) * 5f, out hit, Mathf.Infinity, isGround)) 
        {
            Debug.Log("Did Hit");
            return false;
        }
        return true;
    }

    public void RollAction(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (PlayerStateMachine.CurrentState == PlayerIdleState && grounded && moveDirection != Vector3.zero && isRunning && CanRollCollisionCheck()) //Roll if IdleState && grounded && running
        {
            PlayerStateMachine.ChangeState(PlayerRollingState);
        }
    }

    public void JumpAction(InputAction.CallbackContext context)
    {
        if (PlayerStateMachine.CurrentState == PlayerIdleState && grounded && context.performed) //Jump if IdleState && grounded
        {
            PlayerStateMachine.ChangeState(PlayerJumpState);
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        if (PlayerStateMachine.CurrentState == PlayerIdleState && grounded) //Sprint If IdleState && grounded
        {
            isRunning = context.started || context.performed;
        }
    }

    public void UpdateCamera()
    {
        //Camera Direction
        Vector3 camFoward = cam.forward;
        Vector3 camRight = cam.right;
        camFoward.y = 0f;
        camRight.y = 0f;

        //Create Relative Camera Direction
        Vector3 forwardRelaive = direction.y * camFoward;
        Vector3 rightRelative = direction.x * camRight;

        //Update moveDirection 
        moveDirection = forwardRelaive + rightRelative;

    }

    public void SwitchCameraMode(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (CameraMode == 1)
            {
                CameraMode = 2;
            }
            else
            {
                CameraMode = 1;
            }
        }
    }

    //Function that returns true if cameramode == 2
    private bool CanStrafe() 
    {
        return CameraMode == 2;
    }

    private void CheckDirection()
    {
        //Sets current maxVelocity
        float currentMaxVelocity = isRunning ? maximumRunVelocity : maximumWalkVelocity;

        // If the player was running and is now walking, adjust the velocity
        if (!isRunning)
        {
            if (Mathf.Abs(velocityZ) > maximumWalkVelocity)
            {
                if (velocityZ > 0) //Fowards movement
                {
                    velocityZ -= Time.deltaTime * deceleration;
                    if (velocityZ < maximumWalkVelocity)
                    {
                        velocityZ = maximumWalkVelocity; // Clamp to walk velocity
                    }
                }
                else //Backwards movement
                {
                    velocityZ += Time.deltaTime * deceleration;
                    if (velocityZ > -maximumWalkVelocity)
                    {
                        velocityZ = -maximumWalkVelocity; // Clamp to walk velocity
                    }
                }
            }

            // Adjust X velocity for left/right movement
            if (Mathf.Abs(velocityX) > maximumWalkVelocity)
            {
                if (velocityX > 0) //Right movement
                {
                    velocityX -= Time.deltaTime * deceleration;
                    if (velocityX < maximumWalkVelocity)
                    {
                        velocityX = maximumWalkVelocity; // Clamp to walk velocity
                    }
                }
                else //Left movement
                {
                    velocityX += Time.deltaTime * deceleration;
                    if (velocityX > -maximumWalkVelocity)
                    {
                        velocityX = -maximumWalkVelocity; // Clamp to walk velocity
                    }
                }
            }
        }

        if (direction == Vector2.zero) //Idle state
        {
            footSteps.enabled = false;
            // Idle: Gradually reduce velocities towards zero
            if (velocityZ > 0)
            {
                velocityZ -= Time.deltaTime * deceleration;
                if (velocityZ < 0) velocityZ = 0; // Clamp to zero
            }
            else if (velocityZ < 0)
            {
                velocityZ += Time.deltaTime * deceleration;
                if (velocityZ > 0) velocityZ = 0; // Clamp to zero
            }

            if (velocityX > 0)
            {
                velocityX -= Time.deltaTime * deceleration;
                if (velocityX < 0) velocityX = 0; // Clamp to zero
            }
            else if (velocityX < 0)
            {
                velocityX += Time.deltaTime * deceleration;
                if (velocityX > 0) velocityX = 0; // Clamp to zero
            }
        }
        else
        {
            if (!grounded)
            {
                footSteps.enabled = false;
            }
            else
            {
                footSteps.enabled = true;
            }
            if (isRunning)
            {
                footSteps.pitch += Time.deltaTime * 1.2f;
                if (footSteps.pitch > maximumFootstepPitch) footSteps.pitch = maximumFootstepPitch;
            }
            else
            {
                footSteps.pitch -= Time.deltaTime * 1.2f;
                if (footSteps.pitch < minimumFootstepPitch) footSteps.pitch = minimumFootstepPitch;
            }
            //Handling Rotation
            if (moveDirection != Vector3.zero && !CanStrafe()) //Only Rotate when moving
            {
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }
            
            if (direction.y > 0 && velocityZ < currentMaxVelocity)
            {
                // Forward
                velocityZ += Time.deltaTime * acceleration;
            }
            else if (direction.y < 0 && velocityZ > -currentMaxVelocity)
            {
                // Backward
                velocityZ -= Time.deltaTime * acceleration;
            }
            else if (direction.y == 0)
            {
                // No vertical input: Gradually reduce velocityY towards zero
                if (velocityZ > 0)
                {
                    velocityZ -= Time.deltaTime * deceleration;
                    if (velocityZ < 0) velocityZ = 0; // Clamp to zero
                }
                else if (velocityZ < 0)
                {
                    velocityZ += Time.deltaTime * deceleration;
                    if (velocityZ > 0) velocityZ = 0; // Clamp to zero
                }
            }

            if (direction.x > 0 && velocityX < currentMaxVelocity)
            {
                // Right
                velocityX += Time.deltaTime * acceleration;
            }
            else if (direction.x < 0 && velocityX > -currentMaxVelocity)
            {
                // Left
                velocityX -= Time.deltaTime * acceleration;
            }
            else if (direction.x == 0)
            {
                // No horizontal input: Gradually reduce velocityX towards zero
                if (velocityX > 0)
                {
                    velocityX -= Time.deltaTime * deceleration;
                    if (velocityX < 0) velocityX = 0; // Clamp to zero
                }
                else if (velocityX < 0)
                {
                    velocityX += Time.deltaTime * deceleration;
                    if (velocityX > 0) velocityX = 0; // Clamp to zero
                }
            }
        }

        //Update Animator floats based on CanStrafe
        if (!CanStrafe())
        {
            // Use the velocity to determine the animation when strafing is not allowed
            float absVelocityZ = Mathf.Abs(velocityZ);
            float absVelocityX = Mathf.Abs(velocityX);
            if (absVelocityZ > absVelocityX)
            {
                // Forward/Backward is dominant
                animator.SetFloat("Velocity Z", absVelocityZ);
                animator.SetFloat("Velocity X", 0);
            }
            else
            {
                // Left/Right is dominant
                animator.SetFloat("Velocity Z", absVelocityX);
                animator.SetFloat("Velocity X", 0);
            }
        }
        else
        {
            // Use both velocities for strafing animations
            animator.SetFloat("Velocity Z", velocityZ);
            animator.SetFloat("Velocity X", velocityX);
        }
    }
}
