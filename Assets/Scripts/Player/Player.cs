using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance; //MAKING A STATIC INSTANCE OF THIS SCRIPT SO THAT IT IS AVALIBLE TO OTHER SCRIPTS

    private Rigidbody2D rb; //THE RIGID BODY OF THE PLAYER, RESPONISBLE FOR MOVEMENT

    private Animator anim; //ANIMATOR OF THE PLAYER, RESPONSIBLE FOR CHANGING ANIMATIONS

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
        rb = GetComponent<Rigidbody2D>(); 

        anim = GetComponentInChildren<Animator>();        
    }

    [Header("Movement")]
    public bool canMove; //CONTROLLS WHEATHER THE PLAYER CAN MOVE, USED WHEN WE WANT TO BLOCK INPUT 
    [SerializeField] private float speed; //MOVEMENT SPEED OF THE PLAYER, APPLIED TO RIGID BODY
    [SerializeField] private float jumpForce; //FORCE APLIED TO THE RIGID BODY WHEN THE PLAYER JUMPS
    [SerializeField] private float doubleJumpForce; //FORCE APPLIED TO THE RIGID BODY WHEN THE PLAYER PERFORMS A DOUBLE JUMP
    [SerializeField] private float gravityScale; //GRAVITY SCALE OF THE RIGID BODY
    private bool canDoubleJump; //TRACKS PLAYERS ABILITY TO DOUBLE JUMP (THIS IS NOT AN UNLOCK CHECK)


    [Header("Buffer & Coyote Jump")]
    [SerializeField] private float bufferJumpWindow = .25f; //TIME PERIOD IN WHICH THE PLAYER CAN ACHIEVE A BUFFER JUMP
    [SerializeField] private float coyoteJumpWindow = .5f; //TIME PERIOD IN WHICH THE PLAYER CAN ACHIEVE A COYOTE JUMP
    private float coyoteJumpPressed = -1; //USED TO TRACK THE TIMING OF COYOTE JUMP
    private float bufferJumpPressed = -1; //USED TO TRACK THE TIMING OF BUFFER JUMP


    [Header("Dash")]
    [SerializeField] private float dashSpeed; //SPEED GIVEN TO PLAYER'S RIGID BODY UPON DASHING
    [SerializeField] private float dashDuration; //DURATION OF THE DASH AND IGNORING PLAYER'S IMPUT WHILE DASHING
    [SerializeField] private float dashCooldown; //COOLDOWN BETWEEN CONSECUTIVE DASHES
    [SerializeField] private float dashFloatDuration; //DURATION OF ZERO GRAVITY STATE AFTER DASHING
    private float dashTimer; //USED TO TRACK DASH DURATION
    private bool dashedAirborne; //USED TO MARK THE TIME WHEN PLAYER DASHES WHILE AIRBORNE, HELPS MODIFY THE COOLDOWN
    private bool triggerGravity; //AFTER SETTING THIS VARIABLE WE CAN HANDLE GRAVITY SCALE CHANGE
    private float gravityTimer = -1; //USED TO TRACK ZERO GRAVITY STATE DURATION
    private bool isDashing = false; //BLOCKS INPUT FROM THE PLAYER IF THE PLAYER IS DASHING


    [Header("Wall Interactions")]
    [SerializeField] private float wallJumpDuration = .6f; //DURATION OF APPLYING VELOCITY TO THE RIGID BODY AND BLOCKING PLAYER'S INPUT WHILE WALL JUMPING
    [SerializeField] private Vector2 wallJumpForce; //FORCE APPLIED TO THE RIGID BODY UPON WALL JUMPING
    private bool iswallJumping; //TRACKS WHEATHER THE PLAYER IS CURRENTLY WALL JUMPING


    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 1; //DURATION OF THE KNOCKBACK AND BLOCKING PLAYER'S INPUT
    [SerializeField] private Vector2 knockbackPower; //FORCE APPLIED TO THE RIGID BODY ON KNOCKBACK TRIGGERED
    private bool isKnocked; //BLOCKS PLAYER'S INPUT IF THE PLAYER IS KNOCKED


    [Header("Collision")]
    [SerializeField] private float groundCheckDistance; //DISTANCE BETWEEN THE CENTER OF THE PLAYER TO THE GROUND, HELPS DETERMINE IF THE PLAYER IS CURRENTLY ON THE GROUND
    [SerializeField] private float wallCheckDistance; //DISTANCE BETWEEN THE CENTER OF THE PLAYER TO HIS RIGHT SIDE, HELPS GETERMINE IF THE PLAYER IS CURRENTLY AGAINST THE WALL
    [SerializeField] private LayerMask whatIsGround; //LAYER MASK ASSIGNED TO THE GROUND, HELPS WHILE CHECKING FOR COLLISIONS
    private bool isGrounded; //TRUE IF THE PLAYER IS ON THE GROUND
    private bool isAirborne; //TRUE IF THE PLAYER IS AIRBORNE
    private bool isWallDetected; //TRUE IF THE PLAYER IS FACING A WALL

    [Header("Enemy Interactions")]
    [SerializeField] private float enemyCheckRadius;
    [SerializeField] private LayerMask whatIsEnemy;
    [SerializeField] private Transform enemyCheck;

    private float xInput; //INPUT ON THE HORIZONTAL AXIS
    private float yInput; //INPUT ON THE VERTICAL AXIS
    private bool facingright = true; //TRUE IF THE PLAYER IS CURRENTLY FACING RIGHT
    private int facingDirection = 1; //-1 IF THE PLAYER IS FACING LEFT, 1 IF THE PLAYER IS FACING RIGHT

    // Start is called before the first frame update
    void Start()
    {
        canMove = true; //PLAYER CAN ALWAYS MOVE AT THE START
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAirbornestatus();

        if(isKnocked) return; //IF THE PLAYER IS KNOCKED WE WANT TO CUT ALL THE INPUT

        if(isDashing) //SAME THING GOES FOR THE DASHING
        {
            Dash(); //WE JUST WANT TO APPLY THE VELOCITY AND MOVE ON
            triggerGravity = true; //ALSO MAKE SURE TO TRIGGER ZERO GRAVITY IF THE EFFECT ENDS
            return;
        }
        else if(triggerGravity)
        {
            gravityTimer = Time.time; //IF IT DOES, SET ZERO GRAVITY TIMER TO CURRENT TIME
        }
        
        //HANDLING EVERYTHING ELSE
        HandleEnemyDetection();
        HandleGravity(); 
        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleCollision();
        HandleAnimator();
    }

    private void HandleEnemyDetection()
    {
        if (rb.velocity.y >= 0) return; //ENABLE DAMAGING ENEMIES ONLY WHILE FALLING

        Collider2D[] colliders = Physics2D.OverlapCircleAll(enemyCheck.position, enemyCheckRadius, whatIsEnemy); //COLLECT ALL COLLIDERS AROUND THE DAMAGE POINT THAT ARE OF TYPE ENEMY

        foreach (var enemy in colliders)
        {
            Enemy newEnemy = enemy.GetComponent<Enemy>(); //GET THE ENEMY SCRIPT FROM THE COLLIDER

            if(newEnemy) //IF IT EXISTS
            {
                newEnemy.Hit(); //DAMAGE THE ENEMY
                Jump(); //AND PERFORM A JUMP
            }
        }
    }

    public void Push(Vector2 direction, float duration  = 0) => StartCoroutine(PushRoutine(direction, duration));
    

    private IEnumerator PushRoutine(Vector2 direction, float duration = 0)
    {
        canMove = false; //DISABLE PALYER'S ABILITY TO MOVE SO THAT HE CAN BE MOVED

        rb.velocity = Vector2.zero; //REMOVE ANY CURRENT VELOCITYTO NOT INTERFERE WITH THE PUSH
        rb.AddForce(direction, ForceMode2D.Impulse); //ADD AND IMPULSE FORCE TO THE RIGID BODY IN THE SAID DIRECTION AND MODE

        yield return new WaitForSeconds(duration); //WAIT FOR THE PLAYER TO BE PUSHED AND UNBLOCK HIS MOVEMENT

        canMove = true;
    }

    //ANALIZES IF THE DASH CAN BE PERFORMED
    private void DashButton()
    {
        //IGNORE INPUT IF ANY OF THOSE CONDITIONS IS NOT MET
        if(!GameManager.instance.CanDash || isDashing || Time.time < dashTimer + dashCooldown || dashedAirborne) return;

        if(!isAirborne) dashTimer = Time.time; //IF THE PLAYER IS ON THE GROUND START COOLDOWN NORMALLY
        else dashedAirborne = true; //ELSE BLOCK THE ABILITY TO DASH UNTIL LANDED

        StartCoroutine(DashRoutine()); //START DASHING 
    }
    
    //APPLIES VELOCITY TO THE PLAYER WHILE DASHING
    private void Dash()
    {
        HandleCollision(); //MAKE SURE TO UPDATE COLLISIONS WHILE DASHING
        if(isAirborne) dashedAirborne = true; //IF THE PLAYER BECAME AIRBORNE WHILE DASHING, DET DASHED AIRBORNE VARIABLE
        rb.velocity = new Vector2(dashSpeed * facingDirection, 0);
    }
    
    //MAKES SURE TO BLOCK USER INPUT AND UNBLOCK IT AFTER THE DASH FINNISHES
    private IEnumerator DashRoutine()
    {
        isDashing = true;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }

    //TURNS ON ZERO GRAVITY STATE AFTER DASHING
    private void HandleGravity()
    {
        triggerGravity = false; //MAKE SURE THAT IT TRIGGERS ONLY ONCE AFTER DASH
        if(Time.time < gravityTimer + dashFloatDuration) rb.gravityScale = 0.0f; //IF THE TIMER DIDN'T EXCEED EXPECTED TIME, KEEP GRAVITY AT 0
        else rb.gravityScale = gravityScale; //TURN IT BACK TO STARTING POSITION OTHERWISE
            
    }
    
    //KNCOKS THE PLAYER BACK AND BLOCKS INPUT
    public void Knockback()
    {
        if(isKnocked || !gameObject.activeSelf) return; //IGNORE COMMAND IF THE PLAYER IS KNOCKED ALREADY OR INACTIVE IN HIERARCHY

        StartCoroutine(KnockbackRoutine()); //START BLOCKING INPUT

        rb.velocity = new Vector2(knockbackPower.x * -facingDirection, knockbackPower.y); //APPLY APROPRIATE VELOCITY

    }

    //MAKES SURE TO BLOCK PLAYER'S INPUT ON BEING KNOCKED AND UNBLOCK IT AFTER IT ENDS
    private IEnumerator KnockbackRoutine()
    {
        isKnocked = true;

        yield return new WaitForSeconds(knockbackDuration);

        isKnocked = false;
    }

    //EACH FRAME DECIDES WHEATHER TO HANDLE LANDING OR UPDATE ISAIRBORNE VARIABLE
    private void UpdateAirbornestatus()
    {
        if (isGrounded && isAirborne) HandleLanding(); //IF THE PLAYER IS ON THE GROUND AND ISAIRBORNE IS SET TO TRUE, THAT MEANS THE PLAYER IS LANDING IN CURRENT FRAME 
            
        if (!isGrounded && !isAirborne)  BecomeAirborne(); //MAKE IS AIRBORNE TRUE IF IT'S NOT ALREADY TRUE AND THE PLAYER IS OFF THE GROUND
    }

    //HANDLES PLAYER BECOMING AIRBORNE
    private void BecomeAirborne()
    {
        if(isDashing) dashedAirborne = true; //IF THE PLAYER BECAME AIRBORNE WHILE DASHING, THAT MEANS THEY CAN'T DASH AGAIN UNTIL TOUCHING THE GROUND
        isAirborne = true; //BECOME AIRBORNE
        if(rb.velocity.y < 0) ActivateCoyoteJump(); //ALLOW COYOTE JUMP IF THE PLAYER HAS FALLEN FROM THE LEDGE
    }

    //COORDINATES VARIABLES ON LANDING AND CHECKS FOR BUFFER JUMP
    private void HandleLanding()
    {
        isAirborne = false; //PLAYER IS NO LONGER AIRBORNE
        canDoubleJump = true; //PLAYER CAN DOUBLE JUMP IF HE JUMPS NOW
        dashedAirborne = false; //PLAYER CAN DASH AGAIN IF HE DASHED WHILE AIRBORNE
        
        AttemptBufferJump(); //CHECK IF PLAYER REQUESTED BUFFER JUMP WHILE AIRBORN AND NAILED THE TIMING
    }

    //PROCESSES USER'S INPUT
    private void HandleInput()
    {
        //SET INPUT VARIABLES
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

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
        if(isAirborne) bufferJumpPressed = Time.time;
    }

    //IF THE PLAYER BUFFERED A JUMP, ALLOW HIM TO JUMP AGAIN
    private void AttemptBufferJump()
    {
        //IF THE TIMING OF THE PLAYER WAS CORRECT
        if(Time.time < bufferJumpPressed + bufferJumpWindow)
        {
            bufferJumpPressed = Time.time - 1; //MAKE SURE TO RESET THE TIMER
            Jump(); //AND MAKE THE PLAYER JUMP
        }
    }  

    private void ActivateCoyoteJump() => coyoteJumpPressed = Time.time; //START COUNTER FOR COYOTE JUMP

    private void CancelCoyoteJump() =>  coyoteJumpPressed = Time.time - 1; //MAKE SURE THE TIMER OF COYOTE JUMP DOES NOT FIT THE WINDOW
    #endregion

    //ANALIZES WHAT KIND OF JUMP CAN BE PERFORMED
    private void JumpButton()
    {
        bool coyoteJumpAvalible = Time.time < coyoteJumpPressed + coyoteJumpWindow; //IF THE PLAYER TIMED PRESSING SPACE, ENABLE COYOTE JUMP

        //CHOOSE CORRECT TYPE OF JUMP
        if(isGrounded || coyoteJumpAvalible) Jump(); 
        else if(isWallDetected && !isGrounded) WallJump(); 
        else if(canDoubleJump) DoubleJump();

        CancelCoyoteJump(); //MAKE SURE THAT AFTER ANY TYPE OF JUMP PLAYER CANNOT COYOTE JUMP 
    }

    private void Jump() => rb.velocity = new Vector2(rb.velocity.x, jumpForce); //GIVE PLAYER VELOCITY TO JUMP

    //PERFORMS THE DOUBLE JUMP
    private void DoubleJump()
    {   
        if(!GameManager.instance.CanDoubleJump) return; //DON'T ALLOW DOUBLE JUMP IF THE ABILITY IS NOT UNLOCKED
        iswallJumping = false; //REENABLE USER'S INPUT BACK IF DOUBLE JUMPING TO ALLOW CORRECTION
        canDoubleJump = false; //DON'T ALLOW TO DOUBLE JUMP AGAIN
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce); //APPLY APROPRIATE VELOCITY
    }

    //PERFORMS THE WALL JUMP
    private void WallJump()
    {
        if(!GameManager.instance.CanWallJump) return; //DON'T ALLOW WALL JUMP IF THE ABILITY IS NOT UNLOCKED

        canDoubleJump = true; //MAKE SURE THE PLAYER CAN DOUBLE JUMP AFTER WALL JUMPING
        rb.velocity = new Vector2(wallJumpForce.x * -facingDirection, wallJumpForce.y); //APPLY APROPRIATE VELOCITY
       
        Flip(); //FLIP THE CHARACTER BECAUSE WALL JUMP CAN BE PERFORMED ONLY WHILE FACING THE WALL

        StopAllCoroutines(); //MAKE SURE PREVIOUS WALL JUMP COROUTINE ENDS
        StartCoroutine(WallJumpRoutine()); //BLOCK USERS INPUT FOR A SHORT DUARTION
    }

    //MAKES SURE TO DISABLE USER'S INPUT WHILE WALL JUMPING AND ENABLE IT AFTERWARDS
    private IEnumerator WallJumpRoutine()
    {
        iswallJumping = true;

        yield return new WaitForSeconds(wallJumpDuration);

        iswallJumping = false;
    }

    //DECIDES WHEATHER TO PERFORM WALL SLIDE 
    private void HandleWallSlide()
    {
        bool canWallSlide = isWallDetected && rb.velocity.y < 0; //ALLOW PLAYER TO SLIDE IF HE'S FACING THE WALL
        
        float yModifier = yInput < 0 ? 1 : .05f; //IF THE USER IS PRESSING S MAKE SLIDING DOWN FASTER

        if(!GameManager.instance.CanWallJump || !canWallSlide) return; //DON'T ALLOW WALL SLIDE IF ABILITY IS NOT UNLOCKED OR THE PLAYER IS NOT AGAINST THE WALL

        dashedAirborne = false; //REENABLE DASH IF PLAYER DASHED  WHILE AIRBORNE

        rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * yModifier); //SLOW DOWN THE PLAYER ON Y AXIS SO MIMIC SLIDING
    }

    //UPDATES COLLISIONS VARIABLES
    private void HandleCollision()
    {
        //DRAW A LINE FROM THE CENTER OF THE PLAYER TO THE BOTTOM OF THE HITBOX AND RETURN TRUE IF THIS LINE CUTS THROUGH GROUND
        isGrounded = Physics2D. Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);

        //DRAW A LINE FROM THE CENTER OF THE PLAYER TO THE CURRENTLY FACING EDGE OF THE HITBOX AND RETURN TRUE IF THIS LINE CUTS THROUGH GROUND (WALL)
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDirection, wallCheckDistance, whatIsGround);
    }

    //UPDATES ANIMATOR WITH DATA REQUIRED TO CHANGE ANIMATIONS
    private void HandleAnimator()
    {
        anim.SetFloat("speed", Mathf.Abs(rb.velocity.x));
    }

    //APPLIES VELOCITY TO THE RIGID BODY
    private void HandleMovement()
    {
        if(isWallDetected || iswallJumping) return; //IGNORE INPUT IF THE PLAYER IS FACING THE WALL OR WALL JUMPING

        rb.velocity = new Vector2(xInput * speed, rb.velocity.y); //APPLY APROPRIATE VELOCITY
    }

    //DECIDES IF FLIP SHOULD BE PERFORMED BASED ON USER INPUT AND CURRENT ROTATION
    private void HandleFlip()
    {
        if(xInput < 0 && facingright || xInput > 0 && !facingright) Flip();
    }

    //UPDATES DIRECTION VARIABLES AND FLIPS THE PLAYER
    private void Flip()
    {
        facingDirection *= -1; //UPDATE FACING DIRECTION
        transform.Rotate(0, 180, 0); //ROTATE THE PLAYER
        facingright = !facingright; //UPDATE FACING RIGHT VARIABLE
    }

    //USED TO FREEZE ANIMATION WHILE GOING THROUGH DOOR BETWEEN SCENES
    public void StopResumeAnim()
    {
        anim.enabled = !anim.enabled;
    }

    //VISUALIZES DISTANCE OF COLLISIONS
    private void OnDrawGizmos() 
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDirection), transform.position.y));
        Gizmos.DrawWireSphere(enemyCheck.position, enemyCheckRadius);
    }

}
