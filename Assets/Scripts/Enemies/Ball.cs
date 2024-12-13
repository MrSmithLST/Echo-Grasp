using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapSaw : MonoBehaviour
{
    //private Animator anim;

    private SpriteRenderer sr;


    private void Awake()
    {
        //GETTING COMPONENTS NEEDED TO ANIMATE THE OBJECT
        //anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    [SerializeField] private float moveSpeed = 3; //MOVEMENT SPEED OF THIS GAME OBJECT
    [SerializeField] private Transform[] wayPoint; //THIS OBJECT WILL TRAVEL BETWEEN THOSE TRANSFORM POINTS
    [SerializeField] private float cooldown; //AMOUNT OF TIME THIS OBJECT WILL WAIT AFTER FINISHING THE INITIAL PATH BEFORE HEADING BACK

    private Vector3[] wayPointPosition; //ARRAY OF PLAIN POSITION, NECESSARY TO UNPARENT WAYPOINTS AND KEEP THEIR INITIAL POSITIONS

    private int moveDirection = 1; //HAS VALUE OF 1 IF THIS OBJECT IS MOVING FROM THE FIRST WAYPOINT TO THE LAST ONE, AND -1 IF OTHERWISE, USED TO CALCULATE THE NEXT WAYPOINT
    private int wayPointIndex = 1; //INDEX OF THE NEXT WAYPOINT THAT THS OBJECT WILL MOVE TOWARDS
    private bool canMove = true; //USED TO STOP THE OBJECT AFTER FINISHING THE PATH AND UPDATE ANIMATIONS

    private void Start()
    {
        UpdateWayPointsInfo(); 
        //WE NEED TO UNPARENT THIS OBJECTS WAYPOINTS, BECAUSE THE WAYPOINTS WILL MOVE WITH THE OBJECT OTHERWISE
        //TO ACHIEVE THAT WE CREATE ANOTHER ARRAY OF WAYPOINTS THAT WILL STORE INITIAL POSITION OF THOSE POINTS
        //SO THAT WE CAN NAVIGATE BETWEEN THEM 
        transform.position = wayPointPosition[0]; //SNAP THIS OBJECT TO THE FIRST WAYPOINT IN THE ARRAY
    }

    private void UpdateWayPointsInfo()
    {
        //CREATING A LIST SO THAT DESIGNERS CAN CREATE AS MUCH WAYPOINTS AS POSSIBLE AND CREATE COMPLEX PATHS
        List<BallWaypoint> wayPointList = new List<BallWaypoint>(GetComponentsInChildren<BallWaypoint>()); //STORE ALL WAYPOINTS IN THIS LSIT

        if (wayPointList.Count != wayPoint.Length) //IF THERE WAS MORE WAYPOINTS ADDED (3 BY DEFAULT)
        {
            wayPoint = new Transform[wayPointList.Count]; //CREATE A NEW ARRAY OF WAYPOINT POSITIONS

            for (int i = 0; i < wayPointList.Count; i++)
            {
                wayPoint[i] = wayPointList[i].transform; //AND UPDATE IT
            }
        }

        wayPointPosition = new Vector3[wayPoint.Length]; //DEFINE THE FINAL SIZE OF THE VECTOR ARRAY

        for (int i = 0; i < wayPoint.Length; i++)
        {
            wayPointPosition[i] = wayPoint[i].position; //AND ASSIGN POSITIONS OF WAYPOINTS TO THIS ARRAY
        }
    }

    private void Update()
    {
        if (!canMove) return;

        //IF THIS OBJECT CAN MOVE, MOVE IT TO THE NEXT WAYPOINT
        transform.position = Vector2.MoveTowards(transform.position, wayPointPosition[wayPointIndex], moveSpeed * Time.deltaTime);

        //IF THE TARGET POINT IS VERY CLOSE CHANGE TARGER POINT TO THE NEXT ONE
        if (Vector2.Distance(transform.position, wayPointPosition[wayPointIndex]) < .1f)
        {   
            //IF WE'VE REACHED THE LAST OR THE FIRST POINT
            if (wayPointIndex == wayPointPosition.Length - 1 || wayPointIndex == 0)
            {
                moveDirection *= -1; //CHANGE THE MOVE DIRECTION OF THIS OBJECT
                StartCoroutine(StopMovement(cooldown)); // AND WAIT AT THE LAST WAYPOINT FOR THE COOLDOWN DURATION
            }

            wayPointIndex += moveDirection; //CHOSE NEXT WAYPOINT BASED ON THE MOVEMENT DIRECTION
        }
    }

    private IEnumerator StopMovement(float delay)
    {
        canMove = false;
        //anim.SetBool("active", canMove);

        yield return new WaitForSeconds(delay);

        canMove = true;
        //anim.SetBool("active", canMove);
        sr.flipX = !sr.flipX; //FLIP THIS OBJECT SO THAT IT LOOKS LIKE IT MOVES THE RIGHT DIRECTION
    }
}

