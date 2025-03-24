using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Animation")]
    public float animationBlendSpeed;
    public Animator animator;
    public string startupName;
    private int startupHash;
    private float currentSpeed;

    [Header("Basic Movement")]
    public float walkSpeed;
    public float sprintSpeed;
    public float moveSpeed;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;
    private float desireMoveSpeed;
    private float lastDesiredMoveSpeed;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool readyToJump = true;

    [Header("Rolling")]
    public float rollSpeed;
    public float slopeRollSpeed;
    public bool rolling;

    [Header("Ground Check")]
    public float playerHeight;
    public float groundDrag;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    public float raySpacing;
    public float slopeCheckDistance;
    public float angleBuffer;
    private RaycastHit slopeHit;
    private Vector3 slopeMoveDirection;
    private Vector3[] raycastOrigins = new Vector3[5];
    private bool isOnSlope;

    [Header("Keybinds")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode rollKey = KeyCode.LeftControl;

    [Header("General")]
    public Transform orientation;
    public MovementState state;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        animator = GetComponentInChildren<Animator>();

        startupHash = Animator.StringToHash(startupName);
    }

    private void Update()
    {
        // ground check
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.6f;

        grounded = Physics.Raycast(raycastOrigin, Vector3.down, playerHeight * 0.5f + 0.4f, whatIsGround);

        bool isInStartupAnim = animator.GetCurrentAnimatorStateInfo(0).shortNameHash == startupHash;

        if (!isInStartupAnim)
        {
            PlayerInput();
            PlayerSpeedControl();
            PLayerAnimations();
            PlayerStateHandler();
        }

        if (grounded) rb.drag = groundDrag;
        else rb.drag = 0;
    }

    private void FixedUpdate() => MovePlayer();

    public enum MovementState
    {
        walking,
        sprinting,
        air,
        rolling
    }

    private void PlayerStateHandler()
    {
        // Rolling
        if (rolling)
        {
            state = MovementState.rolling;
            if (!onSlope() || rb.velocity.y > -0.1f) desireMoveSpeed = rollSpeed;
            else desireMoveSpeed = slopeRollSpeed;
        }
        // Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desireMoveSpeed = sprintSpeed;
        }
        // Walkling
        else if (grounded)
        {
            state = MovementState.walking;
            desireMoveSpeed = walkSpeed;
        }
        // Air
        else state = MovementState.air;


        if (Mathf.Abs(desireMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else moveSpeed = desireMoveSpeed;

        lastDesiredMoveSpeed = desireMoveSpeed;
    }

    private void PLayerAnimations()
    {
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float targetSpeed = horizontalVelocity.magnitude;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * animationBlendSpeed);

        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("Is_Grounded", grounded);
        animator.SetBool("roll_Anim", rolling);
    }

    private void PlayerInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Input.GetKey(leftKey)) horizontalInput = -1f;
        if (Input.GetKey(rightKey)) horizontalInput = 1f;
        if (Input.GetKey(forwardKey)) verticalInput = 1f;
        if (Input.GetKey(backwardKey)) verticalInput = -1f;

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
            Jump();

        if (Input.GetKeyDown(rollKey) && grounded)
            if (rolling)
                StopRoll();
            else
                StartRoll();
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Math.Abs(desireMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desireMoveSpeed, time / difference);

            if (onSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desireMoveSpeed;
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded && !onSlope())
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // on slope
        else if (grounded && onSlope())
        {
            // calculate onSlope movement direction
            slopeMoveDirection = GetSlopeMoveDirection(moveDirection);
            rb.AddForce(slopeMoveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void PlayerSpeedControl()
    {
        if (onSlope())
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        // reset y velocity (in order to jump always same height)
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        readyToJump = false;
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }


    private void StartRoll()
    {
        rolling = true;
    }

    private void StopRoll()
    {
        rolling = false;
    }

    private void UpdateRaycastOrigins()
    {
        // Center ray
        raycastOrigins[0] = transform.position + Vector3.up * 0.6f;

        // Corner rays
        float offset = raySpacing;
        raycastOrigins[1] = raycastOrigins[0] + transform.right * offset;
        raycastOrigins[2] = raycastOrigins[0] - transform.right * offset;
        raycastOrigins[3] = raycastOrigins[0] + transform.forward * offset;
        raycastOrigins[4] = raycastOrigins[0] - transform.forward * offset;
    }

    private bool onSlope()
    {
        UpdateRaycastOrigins();

        isOnSlope = false;
        float minAngle = maxSlopeAngle;

        foreach (Vector3 origin in raycastOrigins)
        {
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit,
                playerHeight * 0.5f + slopeCheckDistance, whatIsGround))
            {
                float angle = Vector3.Angle(Vector3.up, hit.normal);

                // Update if this is the most walkable slope found
                if (angle < minAngle && angle > angleBuffer)
                {
                    minAngle = angle;
                    slopeHit = hit;
                    isOnSlope = true;
                }
            }
        }

        return isOnSlope;
    }

    private Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        if (!isOnSlope) return direction;

        // Project the movement onto the slope plane and normalize
        Vector3 slopeDirection = Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;

        // Add slope influence factor based on steepness
        float slopeFactor = 1f - (Vector3.Angle(Vector3.up, slopeHit.normal) / maxSlopeAngle);
        return slopeDirection * Mathf.Lerp(0.5f, 1f, slopeFactor);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            StopAllCoroutines();
        }
    }
}
