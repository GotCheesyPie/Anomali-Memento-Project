using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NPCFollow : MonoBehaviour
{
    [Header("Target & Following")]
    public Transform target;
    public Vector3 offset;
    public float moveSpeed = 3f;
    public float stoppingDistance = 1.5f;
    public float runningDistance = 3f;

    [Header("Status")]
    [SerializeField] private bool isFollowing;

    private Rigidbody2D rb;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (target == null || !isFollowing)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null)
            {
                anim.SetFloat("xVelocity", 0f);
            }
            return;
        }

        FollowTarget();
    }

    private void FollowTarget()
    {
        float targetFacingDirection = Mathf.Sign(target.localScale.x);
        Vector3 dynamicOffset = new Vector3(offset.x * targetFacingDirection, offset.y, offset.z);
        Vector3 targetPosition = target.position - dynamicOffset;

        float distanceToTargetPos = targetPosition.x - transform.position.x;
        
        if (Mathf.Abs(distanceToTargetPos) > stoppingDistance && Mathf.Abs(distanceToTargetPos) < runningDistance)
        {
            float moveDirection = Mathf.Sign(distanceToTargetPos);
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
        }
        else if (Mathf.Abs(distanceToTargetPos) > runningDistance)
        {
            float moveDirection = Mathf.Sign(distanceToTargetPos);
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed * 1.8f, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        FlipBasedOnVelocity();
        
        UpdateAnimation();
    }

    private void FlipBasedOnVelocity()
    {
        if (rb.linearVelocity.x > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (rb.linearVelocity.x < -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
    
    private void UpdateAnimation()
    {
        if (anim == null) return;
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
    }
}