using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour {
    [Header("Player Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Jump Settings")]
    [SerializeField] private float coyoteTimeDuration = 0.2f; // Time allowed after leaving the ground
    [SerializeField] private float jumpBufferDuration = 0.025f; // Time allowed before landing

    private PlayerStateHandler playerStateHandler;
    private PlayerInputActions inputActions;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 posBeforeJump;
    [SerializeField] private bool isGrounded;
    private HealthSystem healthSystem;

    private float coyoteTimeCounter; // Tracks how long since the player left the ground
    private float jumpBufferCounter; // Tracks how long since the jump button was pressed
    private bool hasJumped;
    private float timeSinceJump; // Tracks how long since the player jumped

    private void Awake() {
        playerStateHandler = GetComponentInChildren<PlayerStateHandler>();
        rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputActions();
        healthSystem = GetComponent<HealthSystem>();

        
    }

    private void OnEnable() {
        // Enable input actions
        inputActions.Movement.Enable();

        // Capture movement input
        inputActions.Movement.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
        inputActions.Movement.Move.canceled += ctx => Move(Vector2.zero);

        healthSystem.OnHit += () => OnTakeHit();
        healthSystem.OnDeath += () => OnDeath();

        // Capture jump input
        inputActions.Movement.Jump.performed += ctx => jumpBufferCounter = jumpBufferDuration;
    }

    private void OnDisable() {
        // Disable input actions
        inputActions.Movement.Disable();

        healthSystem.OnHit -= () => OnTakeHit();
        healthSystem.OnDeath -= () => OnDeath();
    }

    private void Update() {
        // Check if grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Update coyote time counter
        if (isGrounded) {
            posBeforeJump = transform.position;
            coyoteTimeCounter = coyoteTimeDuration;
            if (timeSinceJump > coyoteTimeDuration) {
                Arrive();
            }
        }
        else {
            timeSinceJump += Time.deltaTime;
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Update jump buffer counter
        if (jumpBufferCounter > 0) {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Handle jump
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0) {
            Jump();
        }

        if (playerStateHandler.IsState(PlayerStateHandler.State.Jump) && transform.position.y < posBeforeJump.y) {
            playerStateHandler.SetState(PlayerStateHandler.State.Falling);
        }
    }

    private void FixedUpdate() {
        // Apply movement
        rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
    }

    private void Move(Vector2 movement) {
        moveInput = movement;
        playerStateHandler.UpdateSpriteDirection(movement.x);                

        // Define the state based on the movement buffer time to avoid stick to Idle state
        if (!playerStateHandler.IsState(PlayerStateHandler.State.Jump) && 
            !playerStateHandler.IsState(PlayerStateHandler.State.Falling)) {
            if (movement.x != 0) {
                playerStateHandler.SetState(PlayerStateHandler.State.Walking);
            }
            else {
                playerStateHandler.SetState(PlayerStateHandler.State.Idle);                
            }
        }
    }

    private void Jump() {
        if (!hasJumped){
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0; // Reset jump buffer
            hasJumped = true;

            playerStateHandler.SetState(PlayerStateHandler.State.Jump);
        }
    }

    private void Arrive() {
        timeSinceJump = 0;
        hasJumped = false;
        playerStateHandler.SetState(PlayerStateHandler.State.Landing);
    }

    private void OnTakeHit() {
        playerStateHandler.SetState(PlayerStateHandler.State.Hit);
    }

    private void OnDeath() {
        playerStateHandler.SetState(PlayerStateHandler.State.Death);
    }

    private void OnDrawGizmos() {
        // Visualize ground check
        if (groundCheck != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
