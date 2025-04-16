using System.Diagnostics;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyAI : MonoBehaviour {
    [Header("Patrol Settings")]
    [SerializeField] private Vector2 pointA;
    [SerializeField] private Vector2 pointB;
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Chase Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Patrol Settings")]
    [SerializeField] private float movingTimeLimit = 10f;
    [SerializeField] private float restingTimeLimit = 5f;
    private float movingTime;
    private float restingTime;

    private Transform player;

    private SpriteRenderer spriteRenderer;
    private Vector2 currentTarget;
    private Collider2D targetCollider;
    [SerializeField] private float attackTimer;

    private Animator animator;    

    private enum State { Patrolling, Chasing, Attacking, Resting }
    private readonly string[] animations = { "Idle", "Hit", "Move", "Attack"};
    private State currentState = State.Patrolling;

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.transform.TryGetComponent<Collider2D>(out var collider)) {
            if (targetCollider == collider) {
                return;
            }

            var groundBounds = collider.bounds;
            targetCollider = collider;

            pointA = new (groundBounds.min.x, transform.position.y);
            pointB = new (groundBounds.max.x, transform.position.y);

            currentTarget = pointA;
        }
    }

    private void Start() {
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        SetState(State.Patrolling);
    }

    private void Update() {
        switch (currentState) {
            case State.Patrolling:
            Patrol();
            break;
            case State.Chasing:
            Chase();
            break;
            case State.Attacking:
            Attack();
            break;
            case State.Resting:
            Rest();
            break;
        }

        DetectPlayer();
        attackTimer -= Time.deltaTime;
    }

    private void SetState(State newState) {
        currentState = newState;
        restingTime = 0f;
        movingTime = 0f;

        PlayStateAnimation();        
    }

    private void Patrol() {
        if (movingTime >= movingTimeLimit && currentState == State.Patrolling) {
            SetState(State.Resting);
            return;
        }

        Move(patrolSpeed, currentTarget);

        if (Vector2.Distance(transform.position, currentTarget) < 0.5f) {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }

    private void Chase() {
        if (Vector2.Distance(transform.position, player.position) <= attackRange) {
            return;
        }

        Move(chaseSpeed, player.position);
    }

    private void Attack() {
        // Verifica se a animação de ataque está em execução
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Aguarda a animação de ataque ser concluída
        if (stateInfo.IsName(animations[3]) && stateInfo.normalizedTime >= 1f) {
            // Após a animação, volta para o estado de perseguição
            SetState(State.Chasing);
            attackTimer = attackCooldown;
            player.transform.GetComponent<HealthSystem>().ApplyDamage(attackDamage); // Dano de 1 ponto ao jogador
        }
    }


    private void Rest() {
        restingTime += Time.deltaTime;

        if (restingTime > restingTimeLimit){            
            SetState(State.Patrolling);
        }
    }

    private void Move(float modifier, Vector2 target) {
        movingTime += Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, target, modifier * Time.deltaTime);
        spriteRenderer.flipX = transform.position.x > target.x;
    }

    private void DetectPlayer() {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Prioriza o ataque se o jogador estiver dentro do alcance de ataque
        if (distanceToPlayer <= attackRange && attackTimer <= 0) {
            SetState(State.Attacking);
        }
        // Caso contrário, verifica se o jogador está dentro do alcance de detecção
        else if (distanceToPlayer <= detectionRadius) {
            if (distanceToPlayer <= attackRange) {
                SetState(State.Resting);
            }
            else {
                SetState(State.Chasing);
            }
        }
        // Se o jogador estiver fora do alcance de detecção, volta para patrulha
        else if (currentState == State.Chasing) {
            SetState(State.Patrolling);
            currentTarget = pointA;
        }
    }

    private void PlayStateAnimation() {
        switch (currentState) {
            case State.Patrolling:
            animator.Play(animations[2]);
            break;
            case State.Chasing:
            animator.Play(animations[2]);
            break;
            case State.Attacking:
            animator.Play(animations[3]);
            break;
            case State.Resting:
            animator.Play(animations[0]);
            break;
        }
    }
}
