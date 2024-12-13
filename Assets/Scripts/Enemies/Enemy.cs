using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected SpriteRenderer sr => GetComponent<SpriteRenderer>();
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D[] colliders;

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();

        colliders = GetComponentsInChildren<Collider2D>();
    }
    
    protected Transform player;
    
    [Space]

    [SerializeField] protected float movementSpeed = 2;
    protected bool canMove = true;
    [SerializeField] protected float idleDuration = 1.5f;
    protected float idleTimer;

    [Header("Death")]
    [SerializeField] protected float deathImpactSpeed = 5;
    [SerializeField] protected float deathRotationSpeed = 150;
    protected bool isDead;
    protected int _deathRotationDireaction = 1;

    [Header("Collision")]
    [SerializeField] protected float groundCheckDistance = 1.1f;
    [SerializeField] protected float wallCheckDistance = .7f;
    [SerializeField] protected LayerMask whatIsGround;
    [SerializeField] protected float playerDetectionDistance = 15;
    [SerializeField] protected float backPlayerDetection = 0f;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected LayerMask whatIsPlayer;
    protected bool isPlayerDetected;
    protected bool isWallDetected;
    protected bool isGroundInFrontDetected;
    protected bool isGrounded;

    [Header("HP")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    protected int facingDir = -1;
    protected bool facingRight = false;
    protected bool justFlipped = false;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        if (sr.flipX && !facingRight)
        {
            sr.flipX = false;
            Flip();
        }

    }

    private void UpdatePlayersRef()
    {
        if (!player && GameManager.instance.player)
            player = GameManager.instance.player.transform;
    }

    protected virtual void Update()
    {
        UpdatePlayersRef();
        HandleCollision();
        HandleAnimator();

        idleTimer -= Time.deltaTime;

        if (isDead) HandleDeathRotation();
    }

    public virtual void Die()
    {
        if (rb.isKinematic)
            rb.isKinematic = false;
        EnableColliders(false);

        anim.SetTrigger("hit");
        AudioManager.instance.PlaySFX(1);
        rb.velocity = new Vector2(rb.velocity.x, deathImpactSpeed);
        rb.gravityScale = 3.5f;
        //rb.linearDamping = .5f;
        isDead = true;

        if (Random.Range(0, 100) < 50) _deathRotationDireaction *= -1;

        Destroy(gameObject, 5f);
    }

    protected void EnableColliders(bool enable)
    {
        foreach (var collider in colliders)
        {
            collider.enabled = enable;
        }
    }

    private void HandleDeathRotation()
    {
        transform.Rotate(0, 0, deathRotationSpeed * _deathRotationDireaction * Time.deltaTime);
    }

    protected virtual void HandleFlip(float xValue, float delay = 0)
    {
        if (justFlipped) return;
        if (xValue < transform.position.x && facingRight || xValue > transform.position.x && !facingRight)
        {
            Invoke(nameof(Flip), delay);
            justFlipped = true;
        }
    }

    protected virtual void Flip()
    {
        justFlipped = false;
        facingDir *= -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }

    [ContextMenu("ChangeFacingDirection")]
    public void FlipDefaultFacingDirection()
    {
        sr.flipX = !sr.flipX;
    }

    protected virtual void HandleAnimator()
    {
        if(movementSpeed > 0)
            anim.SetFloat("xVelocity", rb.velocity.x);
    }

    protected virtual void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isGroundInFrontDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position - new Vector3(0,groundCheckDistance - .69f, 0), Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        isPlayerDetected = Physics2D.Raycast(transform.position - (new Vector3(backPlayerDetection, 2, 0) * facingDir), Vector2.right * facingDir, playerDetectionDistance, whatIsPlayer);
    }

    public virtual void Hit()
    {
        currentHealth--;
        if(currentHealth <= 0)
        {
            currentHealth = 0; 
            Die();
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position- new Vector3(0,groundCheckDistance - .69f, 0), new Vector2(transform.position.x + (wallCheckDistance * facingDir), transform.position.y - groundCheckDistance + .69f));
        Gizmos.DrawLine(transform.position - (new Vector3(backPlayerDetection, 0, 0) * facingDir), new Vector2(transform.position.x + (playerDetectionDistance * facingDir), transform.position.y));
    }
}

