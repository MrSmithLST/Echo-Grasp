using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyPhaser : Enemy
{

    [Header("Phaser")]
    [SerializeField] private float timeToAppear;
    [SerializeField] private float activeDuration;
    [Space]
    [SerializeField] private float xMinDistance;
    [SerializeField] private float yMinDistance;
    [SerializeField] private float yMaxDistance;

    
    private float activeTimer;
    private bool isChasing;
    private Transform target;

    protected override void Start()
    {
        ObjectCreator.instance.WakeMeUp(this.gameObject, timeToAppear);
        base.Start();
    }

    protected override void HandleAnimator()
    {
        
    }

    protected override void Update()
    {
        base.Update();

        if(isDead) return;

        activeTimer -= Time.deltaTime;

        if(!isChasing && idleTimer < 0)
        {
            StartChase();
        }
        else if(isChasing && activeTimer < 0)
        {
            EndChase();
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        if(!canMove) return;

        HandleFlip(target.position.x);
        transform.position = Vector2.MoveTowards(transform.position, target.position, movementSpeed * Time.deltaTime);
    }

    private void StartChase()
    {
        if(!GameManager.instance.player)
        {
            EndChase();
            return;
        }

        target = GameManager.instance.player.transform;

        float yPosition = Random.Range(yMinDistance, yMaxDistance);
        float xOffset = Random.Range(0,10) < 5 ? 1 : -1;

        transform.position = target.position + new Vector3(xMinDistance * xOffset, yPosition);

        activeTimer = activeDuration;
        isChasing = true;
        //anim.SetTrigger("appear");
        MakeVisible();

    }

    private void EndChase()
    {
        idleTimer = idleDuration;
        isChasing = false;
        //anim.SetTrigger("disappear");
        MakeInvisible();
    }

    private void MakeInvisible() 
    {
        sr.color = Color.clear;
        EnableColliders(false);
    }

    private void MakeVisible() 
    {
        sr.color = Color.red;
        EnableColliders(true);
    }

    public override void Die()
    {
        base.Die();

        canMove = false;
    }

}
