using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharger : Enemy
{
    [Header("Charger")]
    [SerializeField] private Vector2 impactPower; //IMPACT POWER WHEN HITTING THE WALL
    [SerializeField] private float maxSpeed; //MAXIMUM SPEED OF THE ENEMY WHILE CHARGING
    [SerializeField] private float speedUpRate = .6f; //RATE AT WHICH THE SPEED INCREASES WHILE CHARGING
    [SerializeField] private float defaultSpeed; //USED TO RESET SPEED TO THE STARTING SPEED

    

    protected override void Start()
    {
        base.Start();
        canMove = false;
        defaultSpeed = movementSpeed; //SET THE DEFAULT SPEED
    }

    protected override void Update()
    {
        base.Update();

        HandleCharge();
    }

    private void HandleCharge()
    {
        if (!canMove) return; //RETURN IF IDLE OR BUMPED INTO A WALL
        HandleSpeedUp(); //IF IS CHARGIN, SPEED UP GRADUALLY

        rb.velocity = new Vector2(movementSpeed * facingDir, rb.velocity.y); //ADD VELOCITY

        if (isWallDetected)
        {
            WallHit(); //COLLIDE WITH THE WALL
            return;
        }

        if (!isGroundInFrontDetected) //TURN AROUND IF ABOUT TO FALL
            TurnAround();
    }

    private void HandleSpeedUp()
    {
        movementSpeed += (Time.deltaTime * speedUpRate); //SPEED UP ACCORDING TO THE RATE

        if (movementSpeed >= maxSpeed) //STOP SPEEDING UP IF REACHED THE CAP
            movementSpeed = maxSpeed;
    }

    private void TurnAround()
    {
        movementSpeed = defaultSpeed; //RESET SPEED
        canMove = false; //START IDLE
        rb.velocity = Vector2.zero; //RESET VELOCITY
        Flip();
    }

    private void ChargeIsOver()
    {
        anim.SetBool("hitWall", false); //SET ANIMATION  BACK FROM HITTING THE WALL
        Invoke(nameof(Flip), 1); //FLIP AFTER THE BUMP AND STUN
    }

    private void WallHit()
    {
        canMove = false; //STOP MOVEMENT

        SpeedReset(); //RESET SPEED

        anim.SetBool("hitWall", true); //PLAY ANIMATION OF HITTING THE WALL
        rb.velocity = new Vector2(impactPower.x * -facingDir, impactPower.y); //SET ENEMY FLYING BACKWARDS AFTER HITTING THE WALL
    }

    private void SpeedReset() => movementSpeed = defaultSpeed;
    
    protected override void HandleCollision()
    {
        base.HandleCollision();
        if (isPlayerDetected && isGrounded) //START CHARGE IF ON THE GROUND AND PLAYER IS DETECTED
            canMove = true;
    }
}
