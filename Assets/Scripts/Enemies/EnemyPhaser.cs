using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyPhaser : Enemy
{

    [Header("Phaser")]
    [SerializeField] private float timeToAppear; //TIME BEFORE APPEARING AFTER ENTERING THE ROOM
    [SerializeField] private float activeDuration; //CHASE DURATION
    [Space]
    [SerializeField] private float xMinDistance; //X BOUNDS OF SPAWN
    [SerializeField] private float yMinDistance; //Y BOUNDS OF SPAWN
    [SerializeField] private float yMaxDistance; //Y BOUNDS OF SPAWN
    
    private float activeTimer; //USED TO COUNT DOWN THE CHASE DURATION
    private bool isChasing; //USED TO DETERMINE THE CURRENT STATE
    private Transform target; //TARGET TO CHASE

    protected override void Start()
    {
        ObjectCreator.instance.WakeMeUp(this.gameObject, timeToAppear); //CONTCT OBJECT CREATOR TO REMAIN DORMANT UNTILL APPEARING
        base.Start();
    }

    protected override void HandleAnimator()
    {
        
    }

    protected override void Update()
    {
        base.Update();

        if(isDead) return;

        activeTimer -= Time.deltaTime; //COUND DOWN THE CHASE DURATION

        if(!isChasing && idleTimer < 0) //IF ABLE TO CHASE
        {
            StartChase();
        }
        else if(isChasing && activeTimer < 0) //IF TIMER HAS RAN OUT
        {
            EndChase();
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        if(!canMove) return;

        HandleFlip(target.position.x); //FLIP ACCORDING TO TARGET POSITION
        transform.position = Vector2.MoveTowards(transform.position, target.position, movementSpeed * Time.deltaTime); //SIMPLY MOVE TOWARDS TARGET'S POSITION
    }

    private void StartChase()
    {
        if(!GameManager.instance.player) //IF THERE IS NO PLAYER TO CHASE, SIMPLY DON'T DUH
        {
            EndChase();
            return;
        }

        target = GameManager.instance.player.transform; //SET TARGET TO PLAYER IF THERE IS ONE

        float yPosition = Random.Range(yMinDistance, yMaxDistance); //SET RAMDP, SPAWN POSITION WITHIN Y BOUNDS
        float xOffset = Random.Range(0,10) < 5 ? 1 : -1; //CHOSE RANDOM SIDE TO SPAWN

        transform.position = target.position + new Vector3(xMinDistance * xOffset, yPosition); //SPAWN AT RANDOM POSITION NEAR THE TARGET

        activeTimer = activeDuration; //SET CHASE DURATION
        isChasing = true; //CHANGE STATE TO CHASING
        //anim.SetTrigger("appear");
        MakeVisible(); //TURN ON SPRITES

    }

    private void EndChase()
    {
        idleTimer = idleDuration; //SET TIME TO REMAIN DORMANT
        isChasing = false; //CHANGE STATE TO IDLE
        //anim.SetTrigger("disappear");
        MakeInvisible(); //TURN OF SPRITES
    }

    private void MakeInvisible() 
    {
        sr.color = Color.clear;
        EnableColliders(false); //DISABLE COLLIDERS TO AVOID UNWANTED COLLISIONS
    }

    private void MakeVisible() 
    {
        sr.color = Color.red;
        EnableColliders(true);
    }

    public override void Die()
    {
        base.Die();

        canMove = false; //PREVENT TELEOPRTING TO THE PLAYER AFTER DEATH
    }

}
