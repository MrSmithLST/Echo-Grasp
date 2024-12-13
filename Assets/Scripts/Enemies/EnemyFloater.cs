using System;
using System.Collections;
using UnityEngine;

public class EnemyFloater : Enemy
{

    [Header("Floater")]
    [SerializeField] private float flyForce;
    [SerializeField] private float walkDuration = 2;

    [Space]

    private RaycastHit2D groundBelowDetected;
    private float minFlyDistance;
    private bool isFlying;
    private float walkTimer;
    private float xOriginalPosition;

    private bool flipped = false;

    protected override void Start()
    {
        base.Start();

        xOriginalPosition = transform.position.x;
        isFlying = true;
        minFlyDistance = Physics2D.Raycast(transform.position, Vector2.down, float.MaxValue, whatIsGround).distance;
    }

    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        walkTimer -= Time.deltaTime;

        if(isFlying)
        {
            HandleFlying();
        }
        else
        {
            float xDifference = Mathf.Abs(transform.position.x - xOriginalPosition);

            if(walkTimer < 0 && xDifference < .1f)
            {
                isFlying = true;
                rb.gravityScale = 1;
            }

            HandleMovement();
            HandleTurnAround();
        }
    }

    private void HandleFlying()
    {
        if(groundBelowDetected.distance < minFlyDistance)
            rb.velocity = new Vector2(0, flyForce);
    }

    private void HandleTurnAround()
    {
        if(!isGrounded) return;
        if (!isGroundInFrontDetected || isWallDetected)
        {
            if (flipped) return;

            Invoke(nameof(Flip), idleDuration);
            idleTimer = idleDuration;
            rb.velocity = Vector2.zero;
            flipped = true;
        }
    }

    private void HandleMovement()
    {
        if(!isGrounded) return;

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
        
        if(isFlying)
        {
            isFlying = false;
            walkTimer = walkDuration;
            rb.gravityScale = 3;
        }
    }
}

