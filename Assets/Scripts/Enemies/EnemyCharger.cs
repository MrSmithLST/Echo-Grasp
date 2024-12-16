using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharger : Enemy
{
    [Header("Charger")]
    [SerializeField] private Vector2 impactPower;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float speedUpRate = .6f;
    [SerializeField] private float defaultSpeed;

    

    protected override void Start()
    {
        base.Start();
        canMove = false;
        defaultSpeed = movementSpeed;
    }

    protected override void Update()
    {
        base.Update();

        HandleCharge();
    }

    private void HandleCharge()
    {
        if (!canMove) return;
        HandleSpeedUp();

        rb.velocity = new Vector2(movementSpeed * facingDir, rb.velocity.y);

        if (isWallDetected)
        {
            WallHit();
            return;
        }

        if (!isGroundInFrontDetected)
            TurnAround();
    }

    private void HandleSpeedUp()
    {
        movementSpeed += (Time.deltaTime * speedUpRate);

        if (movementSpeed >= maxSpeed)
            movementSpeed = maxSpeed;
    }

    private void TurnAround()
    {
        movementSpeed = defaultSpeed;
        canMove = false;
        rb.velocity = Vector2.zero;
        Flip();
    }

    private void ChargeIsOver()
    {
        anim.SetBool("hitWall", false);
        Invoke(nameof(Flip), 1);
    }

    private void WallHit()
    {
        canMove = false;

        SpeedReset();
        
        anim.SetBool("hitWall", true);
        rb.velocity = new Vector2(impactPower.x * -facingDir, impactPower.y);
    }

    private void SpeedReset() => movementSpeed = defaultSpeed;
    
    protected override void HandleCollision()
    {
        base.HandleCollision();
        if (isPlayerDetected && isGrounded)
            canMove = true;
    }
}
