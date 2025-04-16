using System.Linq;
using UnityEngine;

public class PlayerStateHandler : MonoBehaviour {
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float lastMoveDirection; // Armazena a última direção de movimento

    private void Awake() {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        // Check if the landing animation has finished
        if (HasEndedTemporaryState()) {
            // Transition to Walking if moving, otherwise Idle
            SetState(lastMoveDirection != 0 ? State.Walking : State.Idle);
        }
        
        if (CurrentState == State.Walking) {
            // Check if the Idle animation is playing while walking
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                // Transition to Walking animation
                UpdateAppearance();
            }
        }
    }

    public enum State {
        Idle,
        Walking,
        Jump,
        Falling,
        Landing,
        Attacking,
        Hit,
        Death,
    }

    private State[] temporaryStates = {
        State.Attacking,
        State.Hit,
        State.Death,
        State.Landing
    };

    public State CurrentState { get; private set; } = State.Idle;

    // Update the player's newState
    public void SetState(State newState) {
        CurrentState = newState;
        UpdateAppearance();
    }

    public bool HasEndedTemporaryState() => 
        temporaryStates.Contains(CurrentState) &&
        animator.GetCurrentAnimatorStateInfo(0).IsName(CurrentState.ToString()) &&
        animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;

    // Update the animator based on the current newState
    private void UpdateAppearance() {
        animator.Play(CurrentState.ToString());
    }

    // Changes the sprite direction based on the movement direction
    public void UpdateSpriteDirection(float moveDirection) {
        if (moveDirection != 0) {
            spriteRenderer.flipX = moveDirection < 0;
        }

        // Store the last movement direction
        lastMoveDirection = moveDirection;
    }

    public State GetState() {
        return CurrentState;
    }
    
    public bool IsState(State state) {
        return CurrentState == state;
    }
}
