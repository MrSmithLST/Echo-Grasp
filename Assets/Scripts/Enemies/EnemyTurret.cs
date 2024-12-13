using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurret : Enemy
{
    [Header("Plant")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private EnemyBullet bulletPrefab;
    [SerializeField] private Transform gunPoint;
    private float lastTimeAttacked;

    protected override void Update()
    {
        base.Update();

        bool canAttack = Time.time > lastTimeAttacked + attackCooldown;

        if (isPlayerDetected && canAttack) Attack();
    }

    private void CreateBullet()
    {
        EnemyBullet newBullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.identity);

        Vector2 bulletVelocity = new Vector2(bulletSpeed * facingDir, 0);
        newBullet.SetVelocity(bulletVelocity);
    }

    private void Attack()
    {
        lastTimeAttacked = Time.time;
        anim.SetTrigger("attack");
        Invoke(nameof(CreateBullet), .5f);
    }
}
