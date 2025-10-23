using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        Patrolling,
        Chasing,
        Stunned
    }

    [Header("State")]
    private State currentState;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    private int patrolDestinationIndex;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float detectionDistance = 5f; 
    [SerializeField] private float detectionAreaHeight = 3f;
    [SerializeField] private float loseSightRadius = 7f;
    [SerializeField] private LayerMask whatIsPlayer;
    [SerializeField] private LayerMask whatIsObstacle;

    [Header("Components")]
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentState = State.Patrolling;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrol();
                CheckForPlayer();
                break;
            case State.Chasing:
                HandleChase();
                CheckIfPlayerIsLost();
                break;
            case State.Stunned:
                HandleStun();
                break;
        }
    }

    private void HandlePatrol()
    {
        Vector2 direction = (patrolPoints[patrolDestinationIndex].position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * patrolSpeed, rb.velocity.y);
        Flip(direction.x);

        if (Vector2.Distance(transform.position, patrolPoints[patrolDestinationIndex].position) < 0.5f)
        {
            patrolDestinationIndex = (patrolDestinationIndex + 1) % patrolPoints.Length;
        }
    }

    private void HandleChase()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * chaseSpeed, rb.velocity.y);
        Flip(direction.x);
    }
    
    private void HandleStun()
    {
        rb.velocity = Vector2.zero;
    }

    private void CheckForPlayer()
    {
        Vector2 origin = transform.position;
        Vector2 direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        Vector2 boxSize = new Vector2(0.1f, detectionAreaHeight);

        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0f, direction, detectionDistance, whatIsPlayer);

        if (hit.collider != null)
        {
            RaycastHit2D obstacleHit = Physics2D.Linecast(transform.position, hit.point, whatIsObstacle);
            if (obstacleHit.collider == null)
            {
                currentState = State.Chasing;
            }
        }
    }

    private void CheckIfPlayerIsLost()
    {
        if (Vector2.Distance(transform.position, playerTransform.position) > loseSightRadius)
        {
            currentState = State.Patrolling;
        }
    }

    public void StartStun()
    {
        if (currentState != State.Stunned)
        {
            currentState = State.Stunned;
        }
    }

    public void StopStun()
    {
        if (currentState == State.Stunned)
        {
            currentState = State.Patrolling;
        }
    }

    private void Flip(float horizontalDirection)
    {
        if (horizontalDirection > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (horizontalDirection < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = transform.position;
        Vector2 direction = new Vector2(Mathf.Sign(transform.localScale.x), 0);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(origin + (direction * (detectionDistance / 2)), new Vector3(detectionDistance, detectionAreaHeight, 1));

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseSightRadius);
    }
}