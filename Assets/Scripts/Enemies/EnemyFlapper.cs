using UnityEngine;

public class EnemyFlapper : Enemy
{
    [Header("Flapper")]
    [SerializeField] float travelDistance = 8; //DISTANCE BETWEEN 2 WAYPOINTS
    [SerializeField] private float flyForce = 1.5f; //FORCE AT WHICH THIS ENEMY IS PUSHED UP
    private Vector3[] wayPoints = new Vector3[2]; //TWO WAYPOINTS TO MOVE BETWEEN
    private int wayIndex; //INDEX OF THE WAYPOINT TO MOVE TOWARDS
    private bool inPlayMode; //FOR DEBUG ONLY

    protected override void Start()
    {
        base.Start();

        //SET WAYPOINTS BY CALCULATING DISTANCE FROM THE SPAWN POINT
        wayPoints[0] = new Vector3(transform.position.x - travelDistance/2, transform.position.y);
        wayPoints[1] = new Vector3(transform.position.x + travelDistance/2, transform.position.y);

        inPlayMode = true; //FOR DEBUG ONLY

        wayIndex = Random.Range(0, wayPoints.Length); //RANDOMLY SELECT THE FIRST WAYPOINT

        InvokeRepeating(nameof(FlyUp), 0, .5f); //FLY UP EVERY 0.5 SECONDS
    }

    protected override void Update()
    {
        base.Update();

        HandleMovement();
    }

    private void FlyUp()
    {
        if(isDead) return; //MAKE SURE NO FORCE IS APPLIED IF DEAD
        rb.velocity = new Vector2(rb.velocity.x, flyForce); //PUSH THE ENEMY UP BY ADDING FORCE TO THE RB
    } 

    private void HandleMovement()
    {
        if(!canMove) return;

        transform.position = Vector2.MoveTowards(transform.position, wayPoints[wayIndex], movementSpeed * Time.deltaTime); //MOVE TOWARDS THE WAYPOINT WITH SELECTED SPEED
        HandleFlip(wayPoints[wayIndex].x); //HANDLE FLIP ACCORDING TO THE WAYPOINT

        if(Vector2.Distance(transform.position, wayPoints[wayIndex]) < .1f) //IF REACHED THE WAYPOINT
            wayIndex = ++wayIndex % wayPoints.Length; //CALCULATE THE NEXT ONE 
        
    }


    protected override void OnDrawGizmos() //FOR DEBUG ONLY
    {
        base.OnDrawGizmos();

        if(!inPlayMode)
        {
            float distance = travelDistance/2;

            Vector3 leftPosition = new Vector3(transform.position.x - distance, 0);
            Vector3 rightPosition = new Vector3(transform.position.x + distance, 0);

            Gizmos.DrawLine(leftPosition, rightPosition);

            Gizmos.DrawWireSphere(leftPosition, .5f);
            Gizmos.DrawWireSphere(rightPosition, .5f);
        }
        else
        {
            Gizmos.DrawLine(transform.position, wayPoints[0]);
            Gizmos.DrawLine(transform.position, wayPoints[1]);
            Gizmos.DrawWireSphere(wayPoints[0], .5f);
            Gizmos.DrawWireSphere(wayPoints[1], .5f);
        }
    }
}