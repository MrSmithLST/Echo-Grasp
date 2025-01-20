using UnityEngine;

//THIS IS A SCRIPT THAT IS THE MAIN SOURCE OF INHERITANCE FOR ALL ENEMIES IN THE GAME
public class Enemy : MonoBehaviour
{
    protected SpriteRenderer sr => GetComponent<SpriteRenderer>(); //SINCE THE SPIRITE RENDERER IS NOT USED OFTENLY, FETCH IT ONLY ON USE
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D[] colliders; //ARRAY OF COLLIDERS, BOTH HITBOXES AND HURTBOXES

    protected virtual void Awake()
    {
        anim = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();

        colliders = GetComponentsInChildren<Collider2D>();
    }
    
    protected Transform player;
    
    [Space]

    [SerializeField] protected float movementSpeed = 2; //BASE MOVEMENT SPEED OF THE ENEMY
    protected bool canMove = true; //DETERMINES IF THE ENEMY IS STATIC OR MOVING, ALSO USED FOR PATROLLING
    [SerializeField] protected float idleDuration = 1.5f; //DURATION OF IDLE STATE WHILE PATROLLING
    protected float idleTimer; //TIMER USED TO COUNT DOWN THE IDLE DURATION

    [Header("Death")]
    [SerializeField] protected float deathImpactSpeed = 5; //FORCE OF THE ENEMY BEING KNOCKED ON DEATH
    [SerializeField] protected float deathRotationSpeed = 150; //ROTATION SPEED OF THE ENEMY ON DEATH
    protected bool isDead;
    protected int deathRotationDireaction = 1;

    [Header("Collision")]
    [SerializeField] protected float groundCheckDistance = 1.1f; //DISTANCE FROM THE POINT POINT THAT CHECKS THE GROUND BENEATH THE ENEMY, USED TO ROTATE ENEMY UPON REACHING AN EDGE
    [SerializeField] protected float wallCheckDistance = .7f; //DISTANCE FROM THE POINT THAT CHECKS THE WALL IN FRONT OF THE ENEMY, USED TO FLIP THE ENEMY UPON REACHING A WALL
    [SerializeField] protected LayerMask whatIsGround; //DETERMINES WHAT IS GROUND AND WHAT SHOULD BE DETECTED
    [SerializeField] protected float playerDetectionDistance = 15; //DISTANCE OF THE RAYCAST THAT DETECTS THE PLAYER
    [SerializeField] protected float backPlayerDetection = 0f; //DISTANCE OF THE RAYCAST THAT DETECTS THE PLAYER BEHIND THE ENEMY
    [SerializeField] protected Transform groundCheck; //POINT THAT CHECKS THE GROUND BENEATH THE ENEMY, USED TO ROTATE ENEMY UPON REACHING AN EDGE
    [SerializeField] protected LayerMask whatIsPlayer; //DETERMINES WHAT IS PLAYER AND WHAT SHOULD BE DETECTED
    protected bool isPlayerDetected; //USED TO CHANGE STATES OF THE ENEMIES BASED ON THEIR BEHAVIOUR AND AGRO
    protected bool isWallDetected; //USED FOR COLLISION CHECKING
    protected bool isGroundInFrontDetected; //USED FOR COLLISION CHECKING
    protected bool isGrounded; //TRUE IF THE ENEMY IS ON THE GROUND

    [Header("HP")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    protected int facingDir = -1; //DETERMINES THE DIRECTION THE ENEMY IS FACING, -1 MEANS LEFT, 1 MEANS RIGHT
    protected bool facingRight = false;
    protected bool justFlipped = false; //USED TO DELAY THE FLIP OF THE ENEMY

    protected virtual void Start()
    {
        currentHealth = maxHealth; //SET THE CURRENT HEALTH TO MAX HEALTH
        if (sr.flipX && !facingRight) //FLIP THE ENEMY ACORDINGLY TO THE SPRITE
        {
            sr.flipX = false;
            Flip();
        }

    }

    //USED TO PREVENT ERRORS WHEN THE PLAYER IS DESTROYED
    private void UpdatePlayersRef()
    {
        if (!player && GameManager.instance.player) //FETCH A NEW REFERENCE TO THE PLAYER FROM THE GAME MANAGER
            player = GameManager.instance.player.transform;
    }

    protected virtual void Update()
    {
        UpdatePlayersRef(); //CHECK FOR THE PLAYER REFERENCE EVERY FRAME
        HandleCollision();
        HandleAnimator();

        idleTimer -= Time.deltaTime; //COUNT DOWN THE TIMER AT ALL TIME SINCE THE BEHAVIOUR CHANGES ONLY WHEN SET TO POSITIVE

        if (isDead) HandleDeathRotation();
    }

    //CALLED WHEN ENEMY IS DEFEATED
    public virtual void Die()
    {
        if (rb.isKinematic) //TURN OF COLLIDIONS AND IF THE ENEMY IS FLYING, MAKE HIM FALL
            rb.isKinematic = false;
        EnableColliders(false);

        anim.SetTrigger("hit"); //SET TRIGGER FOR DEATH ANIMATION
        AudioManager.instance.PlaySFX(1); //PLAY DEATH SOUND
        rb.velocity = new Vector2(rb.velocity.x, deathImpactSpeed); //GIVE KNOCKBACK
        rb.gravityScale = 3.5f; //SET GRAVITY TO MAKE THE ENEMY FALL FASTER
        //rb.linearDamping = .5f;
        isDead = true;

        if (Random.Range(0, 100) < 50) deathRotationDireaction *= -1; //RANDOMLY CHOOSE THE ROTATION DIRECTION

        Destroy(gameObject, 5f); //DESTROY THIS OBJECT AFTER 5 SECONDS TO PREVENT UNNECESERY MEMORY USAGE
    }

    protected void EnableColliders(bool enable)
    {
        foreach (var collider in colliders) //ENABLE OR DISABLE ALL COLLIDERS
        {
            collider.enabled = enable;
        }
    }

    private void HandleDeathRotation()
    {
        transform.Rotate(0, 0, deathRotationSpeed * deathRotationDireaction * Time.deltaTime); //CONTINIOUSLY ROTATE ENEMY ON DEATH
    }

    protected virtual void HandleFlip(float xValue, float delay = 0)
    {
        if (justFlipped) return; //IF THERE IS A REQUEST FOR THE FLIP, BUT THE FLIP IS BUFFERED, RETURN
        if (xValue < transform.position.x && facingRight || xValue > transform.position.x && !facingRight) //FLIP IF NEEDED
        {
            Invoke(nameof(Flip), delay); //IF WITH DELAY, WAIT BY INVOKING
            justFlipped = true; //SET BUFFER
        }
    }

    protected virtual void Flip()
    {
        justFlipped = false; //MAKE SURE TO DISABLE BUFFER
        facingDir *= -1; //UPDATE DATA ON FACING DIRECTION AND FACING RIGHT
        transform.Rotate(0, 180, 0); //FLIP
        facingRight = !facingRight;
    }

    //USED TO FLIP THE ENEMY IN THE EDITOR
    [ContextMenu("ChangeFacingDirection")]
    public void FlipDefaultFacingDirection()
    {
        sr.flipX = !sr.flipX;
    }

    //SETS EVERY NECESERY PARAMETER FOR THE ENEMY TO BE ANIMATED
    protected virtual void HandleAnimator()
    {
        if(movementSpeed > 0)
            anim.SetFloat("xVelocity", rb.velocity.x);
    }

    //CHECK FOR ALL COLLISIONS AND UPDATE THE BOOLS (WORKS THE SAME AS PLAYER SCRIPT)
    protected virtual void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isGroundInFrontDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position - new Vector3(0,groundCheckDistance - .69f, 0), Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        isPlayerDetected = Physics2D.Raycast(transform.position - (new Vector3(backPlayerDetection, 2, 0) * facingDir), Vector2.right * facingDir, playerDetectionDistance, whatIsPlayer);
    }

    public virtual void Hit()
    {
        currentHealth--; //DECREASE HEALTH
        if(currentHealth <= 0)
        {
            currentHealth = 0; //IF DROPPED BELOW 0, SET IT TO 0
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

