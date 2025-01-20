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

        HandleMovement(); //HANDLE MOVEMENT IF NOT DEAD

        if(isGrounded) HandleTurnAround(); //DISABLE TURN AROUND IF NOT GROUNDED OR IF DEAD
    }

    private void HandleTurnAround()
    {
        if (!isGroundInFrontDetected || isWallDetected) //ID THE ENEMY IS ABOUT TO FALL OR HIT THE WALL
        {
            if (flipped) return; //DO NOTHING IF THE FLIP IS BUFFERED

            Invoke(nameof(Flip), idleDuration); //BUFFER THE FLIP, BUT WAIT FOR IDLE DURATION
            idleTimer = idleDuration; //SET THE WAIT TIMER TO ITS DURATION
            rb.velocity = Vector2.zero; //MAKE SURE THE ENEMY STOPS
            flipped = true; //SET THE FLIP BUFFER
        }
    }

    private void HandleMovement()
    {
        if (idleTimer > 0 || (!isGrounded && isWallDetected)) return; //IF THE ENEMY MEETS CONDITIONS TO STOP, DO NOTHING

        rb.velocity = new Vector2(movementSpeed *  facingDir, rb.velocity.y); //ELSE MOVE THE ENEMY
        if (flipped) flipped = false; //RESET THE FLIP BUFFER
    }

}

