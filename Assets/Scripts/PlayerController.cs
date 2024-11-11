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

    private Rigidbody2D _rb; //RIGID BODY OF THE PLAYER

    private Transform _groundPoint; //POINT THAT CHECKS IF THE PLAYER IS ON THE GROUND

    private Transform _wallPoint; //POINT WHICH IS ON THE FRONT EDGE OF THE PLAYER'S HITBOX

    private Transform _wallBackPoint; //POINT THAT IS SLOGHTLY BEHIND THE PLAYER'S HITBOX

    private Animator _anim; //ANIMATOR, RESPONISBLE FOR CHANGING THE ANIMATIONS BASED ON GIVEN DATA

    private PlayersAbilityTracker _abilities; //CONTAINER THAT STORES PLAYER'S UNLOCKED ABILITIES

    private void Awake() {
        if(!instance) //IF THERE IS NO INSTANCE YET
        {
            instance = this; //ASSIGN THIS SCRIPT TO BE THE INSTANCE
            DontDestroyOnLoad(gameObject); //DON'T DESTROY THIS SCRIPTS GAME OBJECT UPON LOADING INTO A DIFFERENT SCENE
        }
        else
        {
            Destroy(gameObject); //IF THERE IS AN INSTANCE ALREADY, DESTROY THIS GAME OBJECT IN ORDER TO NOT HAVE A COPY OF IT
        }

        _rb = GetComponent<Rigidbody2D>();  //ASSIGNING THE RIGID BODY TO THIS SCRIPT

        //_groundPoint  = GameObject.Find("Ground Point").GetComponent<Transform>(); //ASSIGNING THE OTHER VARIABLES

       // _wallPoint = GameObject.Find("Wall Point").GetComponent<Transform>();

       // _wallBackPoint = GameObject.Find("Wall Back Point").GetComponent<Transform>();

        _anim = GameObject.Find("Sprite").GetComponent<Animator>();

        _abilities = GetComponent<PlayersAbilityTracker>(); //GETTING THE ABILITIES CONTAINER
    }
    
    public float movementSpeed; //VELOCITY GIVEN TO THE PLAYER'S RIGID BODY WHILE MOVING
    public float jumpForce; //VELOCITY GIVEN TO THE PLAYER'S RIGID BODY WHILE JUMPING
    
    //[SerializeField]
    private bool _isOnGround; //TRUE IF PLAYERS IS ON THE GROUND, FALSE OTHERWISE
    public LayerMask ground; //LAYERS THAT'S BEING ASIGNED TO THE TILEMAP
    public float dashDuration; //TIMESPAN OF DASHING AFTER PRESSING THE DASH BUTTON
    public float dahsCooldown; //COOLDOWN BETWEEN EACH CONSECUTIVE DASH
    //[SerializeField]
    private float _dashCounter; //COUNTER THAT MONITORS FOR HOW LONG THE PLAYER HAD BEEN DASHING ALREADY
    //[SerializeField]
    private float _dashRechargeCounter; //COUNTER THAT MONITORS HOW LONG THE PLAYER MUST WAIT TO DASH AGAIN
    public float dashMovementSpeed; //PLAYERS MOVEMENT SPEED WHILE DASHING
    public float floatDuration; //DURATION OF FLOATING IN THE AIR AFTER DASHING
    //[SerializeField]
    private bool _justDashed; //TRUE IF PLAYER IS DASHING OR IS SHORTLY AFTER DASHING, USED MAINLY FOR CHECKING WHETHER THE GRVITY SHOULD BE TURNED OFF SO THAT THE PLAYER CAN HANG IN THE AIR FOR SOME TIME AFTER DASHING
    private bool _isAgainstWall; //TRUE IF WALL POINT IS NEAR THE WALL, FALSE OTHERWISE
    //[SerializeField]
    private bool _canDoubleJump; //TRUE IF THE PLAYER CAN PERFORM A DOUBLE JUMP WHILE IN THE AIR, FALSE OTHERWISE
    //[SerializeField]
    private bool _canJumpOfWall; //TRUE IF THE PLAYER CAN PERFORM A WALL JUMP WHILE AGAINST THE WALL OR SLIGHTLY AFTERWARDS, FALSE OTHERWISE
    //[SerializeField]
    private bool _isntFacingWall; //TRUE IF WALL BACK POINT IS IN A CLOSE PROXIMITY TO THE WALL
    public float gravityScale; //GRAVITY SCALE USED ON A PLAYER'S RIGID BODY
    public float wallSlideSpeed; //SPEED OF PLAYER SLIDING ON THE WALL
    public float jumpOfWallDuration; //DURATION OF GIVING THE PLAYER VELOCITY AFTER JUMPING OF WALL
    private float _jumpOfWallCounter; //COUNTER THAT MONITORS FOR HOW MUCH LONGER THE VELOCITY IS BEING GIVEN TO THE PLAYER AFTER WALL JUMPING
    public float jumpOfWallVelocity; //VELOCITY GIVEN TO THE PLAYER AFTER WALL JUMPING
    public bool canMove; //IF TRUE, THE PLAYER HAS CONTROL OVER HIS CHARACRER, IF FALSE THE INPUT IS BLOCKED
    public float minimumJumpOfWall; //MINIMUM DURATION OF WALL JUMPING
    

    // Start is called before the first frame update
    void Start()
    {
        canMove = true; //ALLOWING THE PLAYER TO MOVE BY DEFAULT
    }

    // Update is called once per frame
    void Update()
    {
        if(canMove){
            //CHECKING IF THE PLAYER IS ON THE GROUND (DRAWING CIRCLE AROUND GROUND POINT AND CHECKING IF THERE IS AN OBJECT LABLED AS GROUND IN IT)
            _isOnGround = Physics2D.OverlapCircle(_groundPoint.transform.position, .2f, ground);

            //CHECKING IF THE PLAYER IS SLIDING AGAINST THE WALL (-||-)
            _isAgainstWall = Physics2D.OverlapCircle(_wallPoint.transform.position, .2f, ground);

            //CHECKING IF THE PLAYER CAN JUMP OF WALL (-||-)
            _isntFacingWall = Physics2D.OverlapCircle(_wallBackPoint.transform.position, .2f, ground);

            //REACHARGING DASH
            if(_dashRechargeCounter > 0)
            {
                _dashRechargeCounter -= Time.deltaTime; //IF THERE IS A COOLDOWN TO BE COUNTED DOWN, DO IT AND BLOCK GETTING THE DASH INPUT
            }
            else  //GET THE DASH INPUT OTHERWISE
            {
                if(Input.GetButtonDown("Fire2") && _abilities.canDash) //IF THE PLAYER IS PRESSING THE DASH BUTTON AND HAS THE DASH ABILITY UNLOCKED
                {
                    _dashCounter = dashDuration; //SET THE DASH COUNTER MARKING THAT THE PLAYER IS CURRENTLY DASHING
                    _justDashed = true; //SET THIS VARIABLE SO THAT LATER IT CAN BE USED TO TURN OFF GRAVITY
                }
            }

            //DASHING
            if(_dashCounter > 0) 
            {
                _dashCounter -= Time.deltaTime; //IF THE PLAYER IS DASHING COUNT DOWN THE DASHING TIMER
                _rb.velocity = new Vector3(_rb.transform.localScale.x * dashMovementSpeed, 0); //GIVE THE PLAYER'S RIGID BODY VELOCITY AND BLOCK THE Y AXIS VELOCITY
                _dashRechargeCounter = dahsCooldown; //KEEP ON SETTING RECHARGE COUNTER SO THAT IT STARTS COUNTING DOWN PROPERLY AFTER THE LAST FRAME WHEN THE DASH FINISHES
            }
            else //MOVE NORMALLY IF THE PLAYER ISN'T DASHING
            {
                if(_justDashed) //IF THE PLAYER JUST DASHED HANG HIM IN THE AIR FOR A FRACTION OF A SECOND USING A COROUTINE
                {
                    StartCoroutine(DashFloatRoutine());
                    _justDashed = false;
                }

                //AD MOVEMENT (GIVING THE RIGID BODY VELOCITY EQUAL TO INPUT TIMES MOVEMENT SPEED)
                _rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * movementSpeed, _rb.velocity.y);
                
                //HANDLING DIRECTION CHANGE, BY SETTING THE SCALE OF THE PLAYER MANUALY BASED ON VELOCITY SO THAT THE SPRITES AND AUXILIARY POINTS ARE ADJUSTED TO THE DIRECTION OF THE MOVEMENT
                if(_rb.velocity.x > 0f)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }
                else if(_rb.velocity.x < 0f)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
            }

            //CHECKING IF THE PLAYER CAN JUMP OF WALL
            if(!_isOnGround && (_isntFacingWall || _isAgainstWall) && Input.GetAxisRaw("Horizontal") != 0 && _abilities.canWallJump)
            {
                _canJumpOfWall = true;
            }
            else
            {
                _canJumpOfWall = false;
            }

            //JUMPING/ DOUBLE JUMPING (U HAVE TO FIGURE OUT THE CONDITIONS >~<)
            if(Input.GetButtonDown("Jump") && (_isOnGround || (_canDoubleJump && _abilities.canDoubleJump) || (_canJumpOfWall && Input.GetAxisRaw("Horizontal") != 0)))
            {
                if(_isOnGround || _canJumpOfWall) //ALLOWING THE PLAYER TO DOUBLE JUMP ONLY IF HE DIDN'T ALREADY DOUBLE JUMP
                {
                    _canDoubleJump = true;
                }
                else
                {
                    _canDoubleJump = false;
                }
                
                //SETTING THE COUNTER FOR WALL JUMP IF ALL THE CONDITIONS ARE MET
                if(_isAgainstWall && Input.GetAxis("Horizontal") != 0 && _abilities.canWallJump && !_isOnGround)
                {
                    _jumpOfWallCounter = jumpOfWallDuration;
                }
                else //JUMPING REGULARRY OTHERWISE
                {
                    _rb.velocity = new Vector3(_rb.velocity.x, jumpForce); //ADDING VELOCITY ON Y AXIS
                }
            }

            //JUMPING OF WALL (ACTIVATE IF THE TIMER WAS SET IN PREVIOUS BLOCK)
            if(_jumpOfWallCounter > 0)
            {
                StartCoroutine(JumpingOfWallRoutine()); //BLOCKING THE PLAYER MOVEMENT SO THAT THE WALL JUMP HAS IT'S MINIMAL DISTANCE
                _jumpOfWallCounter -= Time.deltaTime; //COUNTING DOWN THE TIMER
                if(Input.GetAxis("Horizontal") != 0) //SETTING IT TO 0 IF BEING INTERRUPTED BY THE PLAYER INPUT AFTER THE MINIMUM WALL JUMP DURATION
                {
                    _jumpOfWallCounter = 0;
                }

                if(transform.localScale.x > 0) //WALL JUMPING BY GIVING THE PLAYER BOTH VELOCITY ON Y AXIS BUT ALSO ON THE X AXIS BASED ON SCALE TO PUSH HIM AWAY FROM THE WALL
                {
                    _rb.velocity = new Vector2(-jumpOfWallVelocity, jumpForce);
                }   
                else
                {
                    _rb.velocity = new Vector2(jumpOfWallVelocity, jumpForce);
                }
            }
            
            //SLIDING THE PLAYER ON THE WALL (IF HE'S CURRENTLY AGAINST THE WALL AN PRESSING THE SLIDE BUTTON AND HAS THE ABILITY UNLOCKED)
            if(_isAgainstWall && !_isOnGround && !Input.GetButtonDown("Jump") && _rb.velocity.y <= 0 && Input.GetAxisRaw("Horizontal") != 0 && _abilities.canWallJump)
            {
                _rb.velocity = new Vector2(_rb.velocity.x, wallSlideSpeed); //GIVE HIM THE NEGATIVE Y AXIS VELOCITY SO THAT THE PLAYER MOVES DOWN
            }
        }

        //GIVING THE ANIMATOR INFORMATION ABOUT VELOCITY
        _anim.SetFloat("speed",Mathf.Abs(_rb.velocity.x));
    }

    //HANGING IN THE AIR AFTER DASHING (SETTING THE GRAVITY TO 0 FOR A FRACTION OF A SECOND)
    IEnumerator DashFloatRoutine()
    {
        _rb.gravityScale = 0.0f; 
        yield return new WaitForSeconds(floatDuration); 
        _rb.gravityScale = gravityScale; 
    }

    //JUMPING OF THE WALL BY A MINIMUM DISTANCE
    IEnumerator JumpingOfWallRoutine()
    {
        canMove = false;
        yield return new WaitForSeconds(minimumJumpOfWall);
        canMove = true;

    }

    public void StopResumeAnim() //CHANGES THE CURRENT STATE OF THE ANIMATOR
    {
        _anim.enabled = !_anim.enabled;
    }
}
