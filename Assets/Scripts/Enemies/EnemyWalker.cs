using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWalker : Enemy
{

    private bool flipped = false;

    protected override void Update()
    {
        base.Update();

        if (isDead) return;

        HandleMovement();

        if(isGrounded) HandleTurnAround();
    }

    private void HandleTurnAround()
    {
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
        if (idleTimer > 0 || (!isGrounded && isWallDetected)) return; 

        rb.velocity = new Vector2(movementSpeed *  facingDir, rb.velocity.y);
        if (flipped) flipped = false;
    }

}

