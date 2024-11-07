using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance; //MAKING AN INSTANCE SO THAT THIS SCRIPT CAN BE REACHED FROM ANY OTHER SCRIPT
    //MAKING SURE NOT TO DESTROY THE PLAYER AFTER LOADING A NEW SCENE
    private void Awake() {
        if(!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public Rigidbody2D rb; //RIGID BODY OF THE PLAYER
    public float movementSpeed; //VELOCITY GIVEN TO THE PLAYER'S RIGID BODY WHILE MOVING
    public float jumpForce; //VELOCITY GIVEN TO THE PLAYER'S RIGID BODY WHILE JUMPING
    public Transform groundPoint; //POINT THAT CHECKS IF THE PLAYER IS ON THE GROUND
    public bool isOnGround; //TRUE IF PLAYERS IS ON THE GROUND, FALSE OTHERWISE
    public LayerMask ground; //LAYERS THAT'S BEING ASIGNED TO THE TILEMAP
    public float dashDuration; //TIMESPAN OF DASHING AFTER PRESSING THE DASH BUTTON
    public float dahsCooldown; //COOLDOWN BETWEEN EACH CONSECUTIVE DASH
    [SerializeField]
    private float _dashCounter; //COUNTER THAT MONITORS FOR HOW LONG THE PLAYER HAD BEEN DASHING ALREADY
    [SerializeField]
    private float _dashRechargeCounter; //COUNTER THAT MONITORS HOW LONG THE PLAYER MUST WAIT TO DASH AGAIN
    public float dashMovementSpeed; //PLAYERS MOVEMENT SPEED WHILE DASHING
    public float floatDuration; //DURATION OF FLOATING IN THE AIR AFTER DASHING
    private bool _justDashed; //TRUE IF PLAYER IS DASHING OR IS SHORTLY AFTER DASHING
    public Transform wallPoint; //POINT WHICH IS ON THE FRONT EDGE OF THE PLAYER'S HITBOX
    public bool isAgainstWall; //TRUE IF WALL POINT IS NEAR THE WALL, FALSE OTHERWISE
    [SerializeField]
    private bool _canDoubleJump; //TRUE IF THE PLAYER CAN PERFORM A DOUBLE JUMP WHILE IN THE AIR, FALSE OTHERWISE
    public bool canJumpOfWall; //TRUE IF THE PLAYER CAN PERFORM A WALL JUMP WHILE AGAINST THE WALL OR SLIGHTLY AFTERWARDS, FALSE OTHERWISE
    public bool isntFacingWall; //TRUE IF WALL BACK POINT IS IN A CLOSE PROXIMITY TO THE WALL
    public float gravityScale; //GRAVITY SCALE USED ON A PLAYER'S RIGID BODY
    public float wallSlideSpeed; //SPEED OF PLAYER SLIDING ON THE WALL
    public Transform wallBackPoint; //POINT THAT IS SLOGHTLY BEHIND THE PLAYER'S HITBOX
    public float jumpOfWallDuration; //DURATION OF GIVING THE PLAYER VELOCITY AFTER JUMPING OF WALL
    private float _jumpOfWallCounter; //COUNTER THAT MONITORS FOR HOW MUCH LONGER THE VELOCITY IS BEING GIVEN TO THE PLAYER AFTER WALL JUMPING
    public float jumpOfWallVelocity; //VELOCITY GIVEN TO THE PLAYER AFTER WALL JUMPING
    public bool canMove; //IF TRUE, THE PLAYER HAS CONTROL OVER HIS CHARACRER, IF FALSE THE INPUT IS BLOCKED
    public float minimumJumpOfWall; //MINIMUM DURATION OF WALL JUMPING
    public Animator anim;
    private PlayersAbilityTracker _abilities;

    // Start is called before the first frame update
    void Start()
    {
        _abilities = GetComponent<PlayersAbilityTracker>();

        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(canMove){
            //CHECKING IF THE PLAYER IS ON THE GROUND
            isOnGround = Physics2D.OverlapCircle(groundPoint.transform.position, .2f, ground);

            //CHECKING IF THE PLAYER IS SLIDING AGAINST THE WALL
            isAgainstWall = Physics2D.OverlapCircle(wallPoint.transform.position, .2f, ground);

            //CHECKING IF THE PLAYER CAN JUMP OF WALL
            isntFacingWall = Physics2D.OverlapCircle(wallBackPoint.transform.position, .2f, ground);

            //REACHARGING DASH
            if(_dashRechargeCounter > 0)
            {
                _dashRechargeCounter -= Time.deltaTime;
            }
            else if(_dashRechargeCounter <= 0) //GETTING DASH INPUT FROM THE PLAYER
            {
                if(Input.GetButtonDown("Fire2") && _abilities.canDash)
                {
                    _dashCounter = dashDuration;
                    _justDashed = true;
                }
            }

            //DASHING
            if(_dashCounter > 0)
            {
                _dashCounter -= Time.deltaTime;
                rb.velocity = new Vector3(rb.transform.localScale.x * dashMovementSpeed, 0);
                _dashRechargeCounter = dahsCooldown;
            }
            else
            {
                if(_justDashed)
                {
                    StartCoroutine(DashFloatRoutine());
                    _justDashed = false;
                }

                //AD MOVEMENT (GIVING THE RIGID BODY VELOCITY EQUAL TO INPUT TIMES MOVEMENT SPEED)
                
                
                    rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * movementSpeed,rb.velocity.y);
                
                if(rb.velocity.x > 0f)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
                else if(rb.velocity.x < 0f)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
            }

            //CHECKING IF THE PLAYER CAN JUMP OF WALL
            if(!isOnGround && (isntFacingWall || isAgainstWall) && Input.GetAxisRaw("Horizontal") != 0 && _abilities.canWallJump)
            {
                canJumpOfWall = true;
            }
            else
            {
                canJumpOfWall = false;
            }

            //JUMPING/ DOUBLE JUMPING
            if(Input.GetButtonDown("Jump") && (isOnGround || (_canDoubleJump && _abilities.canDoubleJump) || (canJumpOfWall && Input.GetAxisRaw("Horizontal") != 0)))
            {
                if(isOnGround || canJumpOfWall)
                {
                    _canDoubleJump = true;
                }
                else
                {
                    _canDoubleJump = false;
                }

                if(isAgainstWall && Input.GetAxis("Horizontal") != 0 && _abilities.canWallJump && !isOnGround)
                {
                    _jumpOfWallCounter = jumpOfWallDuration;
                }
                else
                {
                    rb.velocity = new Vector3(rb.velocity.x, jumpForce);
                }
            }

            //JUMPING OF WALL
            if(_jumpOfWallCounter > 0)
            {
                StartCoroutine(JumpingOfWallRoutine());
                _jumpOfWallCounter -= Time.deltaTime;
                if(Input.GetAxis("Horizontal") != 0)
                {
                    _jumpOfWallCounter = 0;
                }

                if(transform.localScale.x > 0)
                {
                    rb.velocity = new Vector2(-jumpOfWallVelocity, jumpForce);
                }   
                else
                {
                    rb.velocity = new Vector2(jumpOfWallVelocity, jumpForce);
                }
            }
            
            //SLIDING THE PLAYER ON THE WALL
            if(isAgainstWall && !isOnGround && !Input.GetButtonDown("Jump") && rb.velocity.y <= 0 && Input.GetAxisRaw("Horizontal") != 0 && _abilities.canWallJump)
            {
                rb.velocity = new Vector2(rb.velocity.x, wallSlideSpeed);
            }
        }

        //GIVING THE ANIMATOR INFORMATION ABOUT VELOCITY
        anim.SetFloat("speed",Mathf.Abs(rb.velocity.x));
    }

    //HANGING IN THE AIR AFTER DASHING
    IEnumerator DashFloatRoutine()
    {
        rb.gravityScale = 0.0f;
        yield return new WaitForSeconds(floatDuration);
        rb.gravityScale = gravityScale;
    }

    //JUMPING OF THE WALL BY A MINIMUM DISTANCE
    IEnumerator JumpingOfWallRoutine()
    {
        canMove = false;
        yield return new WaitForSeconds(minimumJumpOfWall);
        canMove = true;

    }
}
