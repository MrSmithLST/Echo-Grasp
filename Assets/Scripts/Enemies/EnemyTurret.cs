using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurret : Enemy
{
    [Header("Turret")]
    [SerializeField] private float attackCooldown = 1.5f; //COOLDOWN BETWEEN SHOTS
    [SerializeField] private float bulletSpeed; 
    [SerializeField] private EnemyBullet bulletPrefab; //USED TO INSTANTIATE BULLETS
    [SerializeField] private Transform gunPoint; //USED TO DETERMINE THE SPAWN POINT OF THE BULLET
    private float lastTimeAttacked; //COOLDOWN TIMER

    protected override void Update()
    {
        base.Update();

        bool canAttack = Time.time > lastTimeAttacked + attackCooldown; //CHECK IF THE TIME BETWEEN THE LAST ATTACK AND NOW IS GREATER THAN THE COOLDOWN, IF SO ALLOW SHOOTING

        if (isPlayerDetected && canAttack) Attack(); //IF THE PLAYER IS DETECTED AND THE COOLDOWN IS OVER, SHOOT
    }

    private void CreateBullet()
    {
        EnemyBullet newBullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.identity); //INSTANTIATE THE BULLET AT GUNPOINT

        Vector2 bulletVelocity = new Vector2(bulletSpeed * facingDir, 0); //SET UP THE BULLET
        newBullet.SetVelocity(bulletVelocity); 
    }

    private void Attack()
    {
        lastTimeAttacked = Time.time; //SET UP COOLDOWN
        anim.SetTrigger("attack"); //PLAY THE SHOT ANIMATION
        Invoke(nameof(CreateBullet), .5f); //CREATE A BULLET WITH A DELAY SO THAT THE ANIMATION CAN PLAY
    }
}
