using System;
using System.Collections;
using UnityEngine;

public class EnemyFloater : Enemy
{

    [Header("Floater")]
    [SerializeField] private float flyForce; //FORCE APPLIED TO THE ENEMY TO KEEP HIM UP
    [SerializeField] private float walkDuration = 2; //DUARTION OF STAING ON THE GROUND AFTER AIRBORNE HIT

    [Space]

    private RaycastHit2D groundBelowDetected;
    private float minFlyDistance; //MINIMUM DISTANCE TO THE GROUND TO KEEP FLYING
    private bool isFlying; //USED TO DETERMINE THE CURRENT STATE OF THE ENEMY
    private float walkTimer; //COUNTS DOWN TIME THE ENEMY STAYS ON THE GROUND
    private float xOriginalPosition; //BUFFER FOR THE ORIGINAL POSITION SO THAT THE ENEMY CAN FLOAT BACK TO WHERE HE STARTED

    private bool flipped = false; 

    protected override void Start()
    {
        base.Start();

        xOriginalPosition = transform.position.x; //SET UP THE POSITION TO RETURN TO
        isFlying = true; //SET THE STATE TO FLYING BY DEFAULT
        minFlyDistance = Physics2D.Raycast(transform.position, Vector2.down, float.MaxValue, whatIsGround).distance;
    }

    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        walkTimer -= Time.deltaTime; //COUNT DOWN THE TIMER FOR GROUND STATE CONSTANTLY

        if(isFlying)
        {
            HandleFlying();
        }
        else
        {
            float xDifference = Mathf.Abs(transform.position.x - xOriginalPosition); //DISTANCE BETWEEN CURRENT X POSITION AND THE ONE WHEN THE ENEMY HAS TO START FLOATING

            if(walkTimer < 0 && xDifference < .1f) //IF THE TIMER PASSED AND THE ENEMY IS CLOSE TO THE ORIGINAL POSITION
            {
                isFlying = true; //MAKE HIM FLY AGAIN
                rb.gravityScale = 1;
            }

            HandleMovement();
            HandleTurnAround();
        }
    }

    private void HandleFlying()
    {
        if(groundBelowDetected.distance < minFlyDistance) //WHEN IT IS POSSIBLE FOR THE ENEMY TO FLY, DO IT
            rb.velocity = new Vector2(0, flyForce); //BY ADDING THE Y VELOCITY TO THE RB
    }

    private void HandleTurnAround()
    {
        if(!isGrounded) return; //IGNORE WHILE AIRBORNE
        if (!isGroundInFrontDetected || isWallDetected)
        {
            if (flipped) return; //SAME AS OTHER ENEMIES

            Invoke(nameof(Flip), idleDuration);
            idleTimer = idleDuration;
            rb.velocity = Vector2.zero;
            flipped = true;
        }
    }

    private void HandleMovement()
    {
        if(!isGrounded) return; //IGNORE WHILE AIRBORNE, OTHER THAN THAT SAME AS OTHER ENEMIES

        if (idleTimer > 0 || (!isGrounded && isWallDetected)) return; 

        rb.velocity = new Vector2(movementSpeed *  facingDir, rb.velocity.y);
        if (flipped) flipped = false;
    }

    protected override void HandleAnimator()
    {
        base.HandleAnimator();

        //anim.SetBool("isFlying", isFlying);
    }

    protected override void HandleCollision()
    {
        base.HandleCollision();

        groundBelowDetected = Physics2D.Raycast(transform.position, Vector2.down, float.MaxValue, whatIsGround);
    }

    public override void Hit()
    {
        base.Hit();
        
        if(isFlying) //IF IS FLYING, SET TO GROUNDED AND START PATROLLING
        {
            isFlying = false;
            walkTimer = walkDuration;
            rb.gravityScale = 3;
        }
    }
}

