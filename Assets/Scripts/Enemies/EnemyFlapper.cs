using UnityEngine;

public class EnemyFlapper : Enemy
{
    [Header("Flapper")]
    [SerializeField] float travelDistance = 8;
    [SerializeField] private float flyForce = 1.5f;
    private Vector3[] wayPoints = new Vector3[2];
    private int wayIndex;
    private bool inPlayMode;

    protected override void Start()
    {
        base.Start();

        wayPoints[0] = new Vector3(transform.position.x - travelDistance/2, transform.position.y);
        wayPoints[1] = new Vector3(transform.position.x + travelDistance/2, transform.position.y);

        inPlayMode = true;

        wayIndex = Random.Range(0, wayPoints.Length);

        InvokeRepeating(nameof(FlyUp), 0, .5f);
    }

    protected override void Update()
    {
        base.Update();

        HandleMovement();
    }

    private void FlyUp()
    {
        if(isDead) return;
        rb.velocity = new Vector2(rb.velocity.x, flyForce);
    } 

    private void HandleMovement()
    {
        if(!canMove) return;

        transform.position = Vector2.MoveTowards(transform.position, wayPoints[wayIndex], movementSpeed * Time.deltaTime);
        HandleFlip(wayPoints[wayIndex].x);

        if(Vector2.Distance(transform.position, wayPoints[wayIndex]) < .1f)
            wayIndex = ++wayIndex % wayPoints.Length;
        
    }


    protected override void OnDrawGizmos()
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