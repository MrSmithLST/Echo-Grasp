using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance; //MAKING A STATIC INSTANCE OF THIS SCRIPT SO THAT IT IS AVALIBLE TO OTHER SCRIPTS

    private Rigidbody2D _rb; //THE RIGID BODY OF THE PLAYER, RESPONISBLE FOR MOVEMENT

    private Animator _anim; //ANIMATOR OF THE PLAYER, RESPONSIBLE FOR CHANGING ANIMATIONS

    private PlayersAbilityTracker _abilities; //CONTAINER THAT HOLDS INFORMATION ABOUT PLAYER'S UNLOCKABLE ABILITIES

    private void Awake() //HAPPENS BEGORE START
    {
        if(!instance) //IF THERE IS NO SCRIPT ASSIGNED TO THE INSTANCE
        {
            instance = this; //ASSIGN THIS SCRIPT
            DontDestroyOnLoad(gameObject); //AND DON'T DESTROY IT'S GAME OBJECT ON LOADING INTO DIFFERENT SCENES
        }
        else
        {
            Destroy(gameObject); //DESTROY THIS GAME OBJECT OTHERWISE IN ORDER TO REMOVE ADDITIONAL COPIES
        }

        //ASSIGNING OTHER VARIABLES
        _rb = GetComponent<Rigidbody2D>(); 

        _anim = GetComponentInChildren<Animator>();
        
        _abilities = GetComponent<PlayersAbilityTracker>();
    }

    [Header("Movement")]
    public bool canMove; //CONTROLLS WHEATHER THE PLAYER CAN MOVE, USED WHEN WE WANT TO BLOCK INPUT 
    [SerializeField] private float _speed; //MOVEMENT SPEED OF THE PLAYER, APPLIED TO RIGID BODY
    [SerializeField] private float _jumpForce; //FORCE APLIED TO THE RIGID BODY WHEN THE PLAYER JUMPS
    [SerializeField] private float _doubleJumpForce; //FORCE APPLIED TO THE RIGID BODY WHEN THE PLAYER PERFORMS A DOUBLE JUMP
    [SerializeField] private float _gravityScale; //GRAVITY SCALE OF THE RIGID BODY
    private bool _canDoubleJump; //TRACKS PLAYERS ABILITY TO DOUBLE JUMP (THIS IS NOT AN UNLOCK CHECK)


    [Header("Buffer & Coyote Jump")]
    [SerializeField] private float _bufferJumpWindow = .25f; //TIME PERIOD IN WHICH THE PLAYER CAN ACHIEVE A BUFFER JUMP
    [SerializeField] private float _coyoteJumpWindow = .5f; //TIME PERIOD IN WHICH THE PLAYER CAN ACHIEVE A COYOTE JUMP
    private float _coyoteJumpPressed = -1; //USED TO TRACK THE TIMING OF COYOTE JUMP
    private float _bufferJumpPressed = -1; //USED TO TRACK THE TIMING OF BUFFER JUMP


    [Header("Dash")]
    [SerializeField] private float _dashSpeed; //SPEED GIVEN TO PLAYER'S RIGID BODY UPON DASHING
    [SerializeField] private float _dashDuration; //DURATION OF THE DASH AND IGNORING PLAYER'S IMPUT WHILE DASHING
    [SerializeField] private float _dashCooldown; //COOLDOWN BETWEEN CONSECUTIVE DASHES
    [SerializeField] private float _dashFloatDuration; //DURATION OF ZERO GRAVITY STATE AFTER DASHING
    private float _dashTimer; //USED TO TRACK DASH DURATION
    private bool _dashedAirborne; //USED TO MARK THE TIME WHEN PLAYER DASHES WHILE AIRBORNE, HELPS MODIFY THE COOLDOWN
    private bool _triggerGravity; //AFTER SETTING THIS VARIABLE WE CAN HANDLE GRAVITY SCALE CHANGE
    private float _gravityTimer = -1; //USED TO TRACK ZERO GRAVITY STATE DURATION
    private bool _isDashing = false; //BLOCKS INPUT FROM THE PLAYER IF THE PLAYER IS DASHING


    [Header("Wall Interactions")]
    [SerializeField] private float _wallJumpDuration = .6f; //DURATION OF APPLYING VELOCITY TO THE RIGID BODY AND BLOCKING PLAYER'S INPUT WHILE WALL JUMPING
    [SerializeField] private Vector2 _wallJumpForce; //FORCE APPLIED TO THE RIGID BODY UPON WALL JUMPING
    private bool _iswallJumping; //TRACKS WHEATHER THE PLAYER IS CURRENTLY WALL JUMPING


    [Header("Knockback")]
    [SerializeField] private float _knockbackDuration = 1; //DURATION OF THE KNOCKBACK AND BLOCKING PLAYER'S INPUT
    [SerializeField] private Vector2 _knockbackPower; //FORCE APPLIED TO THE RIGID BODY ON KNOCKBACK TRIGGERED
    private bool _isKnocked; //BLOCKS PLAYER'S INPUT IF THE PLAYER IS KNOCKED


    [Header("Collision")]
    [SerializeField] private float _groundCheckDistance; //DISTANCE BETWEEN THE CENTER OF THE PLAYER TO THE GROUND, HELPS DETERMINE IF THE PLAYER IS CURRENTLY ON THE GROUND
    [SerializeField] private float _wallCheckDistance; //DISTANCE BETWEEN THE CENTER OF THE PLAYER TO HIS RIGHT SIDE, HELPS GETERMINE IF THE PLAYER IS CURRENTLY AGAINST THE WALL
    [SerializeField] private LayerMask _whatIsGround; //LAYER MASK ASSIGNED TO THE GROUND, HELPS WHILE CHECKING FOR COLLISIONS
    private bool _isGrounded; //TRUE IF THE PLAYER IS ON THE GROUND
    private bool _isAirborne; //TRUE IF THE PLAYER IS AIRBORNE
    private bool _isWallDetected; //TRUE IF THE PLAYER IS FACING A WALL

    
    private float _xInput; //INPUT ON THE HORIZONTAL AXIS
    private float _yInput; //INPUT ON THE VERTICAL AXIS
    private bool _facingright = true; //TRUE IF THE PLAYER IS CURRENTLY FACING RIGHT
    private int _facingDirection = 1; //-1 IF THE PLAYER IS FACING LEFT, 1 IF THE PLAYER IS FACING RIGHT

    // Start is called before the first frame update
    void Start()
    {
        canMove = true; //PLAYER CAN ALWAYS MOVE AT THE START
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAirbornestatus();

        if(_isKnocked) return; //IF THE PLAYER IS KNOCKED WE WANT TO CUT ALL THE INPUT

        if(_isDashing) //SAME THING GOES FOR THE DASHING
        {
            Dash(); //WE JUST WANT TO APPLY THE VELOCITY AND MOVE ON
            _triggerGravity = true; //ALSO MAKE SURE TO TRIGGER ZERO GRAVITY IF THE EFFECT ENDS
            return;
        }
        else if(_triggerGravity)
        {
            _gravityTimer = Time.time; //IF IT DOES, SET ZERO GRAVITY TIMER TO CURRENT TIME
        }
        
        //HANDLING EVERYTHING ELSE
        HandleGravity(); //DONE
        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleCollision();
        HandleAnimator();
    }

    //ANALIZES IF THE DASH CAN BE PERFORMED
    private void DashButton()
    {
        //IGNORE INPUT IF ANY OF THOSE CONDITIONS IS NOT MET
        if(!_abilities.canDash || _isDashing || Time.time < _dashTimer + _dashCooldown || _dashedAirborne) return;

        if(!_isAirborne) _dashTimer = Time.time; //IF THE PLAYER IS ON THE GROUND START COOLDOWN NORMALLY
        else _dashedAirborne = true; //ELSE BLOCK THE ABILITY TO DASH UNTIL LANDED

        StartCoroutine(DashRoutine()); //START DASHING 
    }

    //TURNS ON ZERO GRAVITY STATE AFTER DASHING
    private void HandleGravity()
    {
        _triggerGravity = false; //MAKE SURE THAT IT TRIGGERS ONLY ONCE AFTER DASH
        if(Time.time < _gravityTimer + _dashFloatDuration) _rb.gravityScale = 0.0f; //IF THE TIMER DIDN'T EXCEED EXPECTED TIME, KEEP GRAVITY AT 0
        else _rb.gravityScale = _gravityScale; //TURN IT BACK TO STARTING POSITION OTHERWISE
            
    }
    
    //APPLIES VELOCITY TO THE PLAYER WHILE DASHING
    private void Dash()
    {
        HandleCollision(); //MAKE SURE TO UPDATE COLLISIONS WHILE DASHING
        if(_isAirborne) _dashedAirborne = true; //IF THE PLAYER BECAME AIRBORNE WHILE DASHING, DET DASHED AIRBORNE VARIABLE
        _rb.velocity = new Vector2(_dashSpeed * _facingDirection, 0);
    }

    //MAKES SURE TO BLOCK USER INPUT AND UNBLOCK IT AFTER THE DASH FINNISHES
    private IEnumerator DashRoutine()
    {
        _isDashing = true;

        yield return new WaitForSeconds(_dashDuration);

        _isDashing = false;
    }

    //KNCOKS THE PLAYER BACK AND BLOCKS INPUT
    public void Knockback()
    {
        if(_isKnocked || !gameObject.activeSelf) return; //IGNORE COMMAND IF THE PLAYER IS KNOCKED ALREADY OR INACTIVE IN HIERARCHY

        StartCoroutine(KnockbackRoutine()); //START BLOCKING INPUT

        _rb.velocity = new Vector2(_knockbackPower.x * -_facingDirection, _knockbackPower.y); //APPLY APROPRIATE VELOCITY

    }

    //MAKES SURE TO BLOCK PLAYER'S INPUT ON BEING KNOCKED AND UNBLOCK IT AFTER IT ENDS
    private IEnumerator KnockbackRoutine()
    {
        _isKnocked = true;

        yield return new WaitForSeconds(_knockbackDuration);

        _isKnocked = false;
    }

    //EACH FRAME DECIDES WHEATHER TO HANDLE LANDING OR UPDATE ISAIRBORNE VARIABLE
    private void UpdateAirbornestatus()
    {
        if (_isGrounded && _isAirborne) HandleLanding(); //IF THE PLAYER IS ON THE GROUND AND ISAIRBORNE IS SET TO TRUE, THAT MEANS THE PLAYER IS LANDING IN CURRENT FRAME 
            
        if (!_isGrounded && !_isAirborne)  BecomeAirborne(); //MAKE IS AIRBORNE TRUE IF IT'S NOT ALREADY TRUE AND THE PLAYER IS OFF THE GROUND
    }

    //HANDLES PLAYER BECOMING AIRBORNE
    private void BecomeAirborne()
    {
        if(_isDashing) _dashedAirborne = true; //IF THE PLAYER BECAME AIRBORNE WHILE DASHING, THAT MEANS THEY CAN'T DASH AGAIN UNTIL TOUCHING THE GROUND
        _isAirborne = true; //BECOME AIRBORNE
        if(_rb.velocity.y < 0) ActivateCoyoteJump(); //ALLOW COYOTE JUMP IF THE PLAYER HAS FALLEN FROM THE LEDGE
    }

    //COORDINATES VARIABLES ON LANDING AND CHECKS FOR BUFFER JUMP
    private void HandleLanding()
    {
        _isAirborne = false; //PLAYER IS NO LONGER AIRBORNE
        _canDoubleJump = true; //PLAYER CAN DOUBLE JUMP IF HE JUMPS NOW
        _dashedAirborne = false; //PLAYER CAN DASH AGAIN IF HE DASHED WHILE AIRBORNE
        
        AttemptBufferJump(); //CHECK IF PLAYER REQUESTED BUFFER JUMP WHILE AIRBORN AND NAILED THE TIMING
    }

    //PROCESSES USER'S INPUT
    private void HandleInput()
    {
        //SET INPUT VARIABLES
        _xInput = Input.GetAxisRaw("Horizontal");
        _yInput = Input.GetAxisRaw("Vertical");

        //PASS USER REQUESTED ACTIONS FURTHER
        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            JumpButton();
            RequestBufferJump(); //IF THE PLAYER CANNOT JUMP RIGHT NOW, TRY TO BUFFER JUMP
        }
        if(Input.GetButtonDown("Fire2"))
        {
            DashButton();
        }

    }

    //SECTION DEDICATED FULLY TO BUFFER JUMPING AND COYOTE JUMPING
    #region Buffer & Coyote Jump

    //STARTS TIME OF BUFFER JUMP
    private void RequestBufferJump()
    {
        if(_isAirborne) _bufferJumpPressed = Time.time;
    }

    //IF THE PLAYER BUFFERED A JUMP, ALLOW HIM TO JUMP AGAIN
    private void AttemptBufferJump()
    {
        //IF THE TIMING OF THE PLAYER WAS CORRECT
        if(Time.time < _bufferJumpPressed + _bufferJumpWindow)
        {
            _bufferJumpPressed = Time.time - 1; //MAKE SURE TO RESET THE TIMER
            Jump(); //AND MAKE THE PLAYER JUMP
        }
    }  

    private void ActivateCoyoteJump() => _coyoteJumpPressed = Time.time; //START COUNTER FOR COYOTE JUMP

    private void CancelCoyoteJump() =>  _coyoteJumpPressed = Time.time - 1; //MAKE SURE THE TIMER OF COYOTE JUMP DOES NOT FIT THE WINDOW
    #endregion

    //ANALIZES WHAT KIND OF JUMP CAN BE PERFORMED
    private void JumpButton()
    {
        bool coyoteJumpAvalible = Time.time < _coyoteJumpPressed + _coyoteJumpWindow; //IF THE PLAYER TIMED PRESSING SPACE, ENABLE COYOTE JUMP

        //CHOOSE CORRECT TYPE OF JUMP
        if(_isGrounded || coyoteJumpAvalible) Jump(); 
        else if(_isWallDetected && !_isGrounded) WallJump(); 
        else if(_canDoubleJump) DoubleJump();

        CancelCoyoteJump(); //MAKE SURE THAT AFTER ANY TYPE OF JUMP PLAYER CANNOT COYOTE JUMP 
    }

    private void Jump() => _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce); //GIVE PLAYER VELOCITY TO JUMP

    //PERFORMS THE DOUBLE JUMP
    private void DoubleJump()
    {   
        if(!_abilities.canDoubleJump) return; //DON'T ALLOW DOUBLE JUMP IF THE ABILITY IS NOT UNLOCKED
        _iswallJumping = false; //REENABLE USER'S INPUT BACK IF DOUBLE JUMPING TO ALLOW CORRECTION
        _canDoubleJump = false; //DON'T ALLOW TO DOUBLE JUMP AGAIN
        _rb.velocity = new Vector2(_rb.velocity.x, _doubleJumpForce); //APPLY APROPRIATE VELOCITY
    }

    //PERFORMS THE WALL JUMP
    private void WallJump()
    {
        if(!_abilities.canWallJump) return; //DON'T ALLOW WALL JUMP IF THE ABILITY IS NOT UNLOCKED

        _canDoubleJump = true; //MAKE SURE THE PLAYER CAN DOUBLE JUMP AFTER WALL JUMPING
        _rb.velocity = new Vector2(_wallJumpForce.x * -_facingDirection, _wallJumpForce.y); //APPLY APROPRIATE VELOCITY
       
        Flip(); //FLIP THE CHARACTER BECAUSE WALL JUMP CAN BE PERFORMED ONLY WHILE FACING THE WALL

        StopAllCoroutines(); //MAKE SURE PREVIOUS WALL JUMP COROUTINE ENDS
        StartCoroutine(WallJumpRoutine()); //BLOCK USERS INPUT FOR A SHORT DUARTION
    }

    //MAKES SURE TO DISABLE USER'S INPUT WHILE WALL JUMPING AND ENABLE IT AFTERWARDS
    private IEnumerator WallJumpRoutine()
    {
        _iswallJumping = true;

        yield return new WaitForSeconds(_wallJumpDuration);

        _iswallJumping = false;
    }

    //DECIDES WHEATHER TO PERFORM WALL SLIDE 
    private void HandleWallSlide()
    {
        bool _canWallSlide = _isWallDetected && _rb.velocity.y < 0; //ALLOW PLAYER TO SLIDE IF HE'S FACING THE WALL
        
        float _yModifier = _yInput < 0 ? 1 : .05f; //IF THE USER IS PRESSING S MAKE SLIDING DOWN FASTER

        if(!_abilities.canWallJump || !_canWallSlide) return; //DON'T ALLOW WALL SLIDE IF ABILITY IS NOT UNLOCKED OR THE PLAYER IS NOT AGAINST THE WALL

        _dashedAirborne = false; //REENABLE DASH IF PLAYER DASHED  WHILE AIRBORNE

        _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * _yModifier); //SLOW DOWN THE PLAYER ON Y AXIS SO MIMIC SLIDING
    }

    //UPDATES COLLISIONS VARIABLES
    private void HandleCollision()
    {
        //DRAW A LINE FROM THE CENTER OF THE PLAYER TO THE BOTTOM OF THE HITBOX AND RETURN TRUE IF THIS LINE CUTS THROUGH GROUND
        _isGrounded = Physics2D. Raycast(transform.position, Vector2.down, _groundCheckDistance, _whatIsGround);

        //DRAW A LINE FROM THE CENTER OF THE PLAYER TO THE CURRENTLY FACING EDGE OF THE HITBOX AND RETURN TRUE IF THIS LINE CUTS THROUGH GROUND (WALL)
        _isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * _facingDirection, _wallCheckDistance, _whatIsGround);
    }

    //UPDATES ANIMATOR WITH DATA REQUIRED TO CHANGE ANIMATIONS
    private void HandleAnimator()
    {
        _anim.SetFloat("speed", Mathf.Abs(_rb.velocity.x));
    }

    //APPLIES VELOCITY TO THE RIGID BODY
    private void HandleMovement()
    {
        if(_isWallDetected || _iswallJumping) return; //IGNORE INPUT IF THE PLAYER IS FACING THE WALL OR WALL JUMPING

        _rb.velocity = new Vector2(_xInput * _speed, _rb.velocity.y); //APPLY APROPRIATE VELOCITY
    }

    //DECIDES IF FLIP SHOULD BE PERFORMED BASED ON USER INPUT AND CURRENT ROTATION
    private void HandleFlip()
    {
        if(_xInput < 0 && _facingright || _xInput > 0 && !_facingright) Flip();
    }

    //UPDATES DIRECTION VARIABLES AND FLIPS THE PLAYER
    private void Flip()
    {
        _facingDirection *= -1; //UPDATE FACING DIRECTION
        transform.Rotate(0, 180, 0); //ROTATE THE PLAYER
        _facingright = !_facingright; //UPDATE FACING RIGHT VARIABLE
    }

    //USED TO FREEZE ANIMATION WHILE GOING THROUGH DOOR BETWEEN SCENES
    public void StopResumeAnim()
    {
        _anim.enabled = !_anim.enabled;
    }

    //VISUALIZES DISTANCE OF COLLISIONS
    private void OnDrawGizmos() 
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - _groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (_wallCheckDistance * _facingDirection), transform.position.y));
    }

}
