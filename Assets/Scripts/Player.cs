using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using Unity.Collections;
using UnityEditor.Callbacks;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    private Rigidbody2D _rb;

    private Animator _anim;

    private PlayersAbilityTracker _abilities;

    private void Awake()
    {
        if(!instance)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _rb = GetComponent<Rigidbody2D>();

        _anim = GetComponentInChildren<Animator>();
        
        _abilities = GetComponent<PlayersAbilityTracker>();
    }

    [Header("Movement")]
    public bool canMove;
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _doubleJumpForce;
    [SerializeField] private float _gravityScale;
    private bool _canDoubleJump; 


    [Header("Buffer & Coyote Jump")]
    [SerializeField] private float _bufferJumpWindow = .25f;
    [SerializeField] private float _coyoteJumpWindow = .5f;
    private float _coyoteJumpActivated = -1;
    private float _bufferJumpPressed = -1;


    [Header("Dash")]
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _dashCooldown;
    [SerializeField] private float _dashFloatDuration;
    private float _dashTimer;
    private bool _dashedAirborne;
    private bool _triggerGravity;
    private float _gravityTimer;
    private bool _isDashing = false;


    [Header("Wall Interactions")]
    [SerializeField] private float _wallJumpDuration = .6f;
    [SerializeField] private Vector2 _wallJumpForce;
    private bool _iswallJumping;


    [Header("Knockback")]
    [SerializeField] private float _knockbackDuration = 1;
    [SerializeField] private Vector2 _knockbackPower;
    private bool _isKnocked;


    [Header("Collision")]
    [SerializeField] private float _groundCheckDistance;
    [SerializeField] private float _wallCheckDistance;
    [SerializeField] private LayerMask _whatIsGround;
    private bool _isGrounded;
    private bool _isAirborne;
    private bool _isWallDetected;

    
    private float _xInput;
    private float _yInput;
    private bool _facingright = true;
    private int _facingDirection = 1;

    // Start is called before the first frame update
    void Start()
    {
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift)) Knockback();

        UpdateAirbornestatus();

        if(_isKnocked) return;

        if(_isDashing)
        {
            Dash();
            _triggerGravity = true;
            return;
        }
        else if(_triggerGravity)
        {
            _gravityTimer = Time.time;
        }

        HandleGravity();
        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleCollision();
        HandleAnimator();
    }

    private void DashButton()
    {
        if(!_abilities.canDash || _isDashing || Time.time < _dashTimer + _dashCooldown || _dashedAirborne) return;

        if(!_isAirborne) _dashTimer = Time.time;
        else _dashedAirborne = true;

        StartCoroutine(DashRoutine());
    }


    private void HandleGravity()
    {
        _triggerGravity = false;
        if(Time.time < _gravityTimer + _dashFloatDuration) _rb.gravityScale = 0.0f;
        else _rb.gravityScale = _gravityScale;
            
    }
    
    private void Dash()
    {
        _rb.velocity = new Vector2(_dashSpeed * _facingDirection, 0);
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;

        yield return new WaitForSeconds(_dashDuration);

        _isDashing = false;
    }

    public void Knockback()
    {
        if(_isKnocked || !gameObject.activeSelf) return;

        StartCoroutine(KnockbackRoutine());

        _rb.velocity = new Vector2(_knockbackPower.x * -_facingDirection, _knockbackPower.y);

    }

    private IEnumerator KnockbackRoutine()
    {
        _isKnocked = true;

        yield return new WaitForSeconds(_knockbackDuration);

        _isKnocked = false;
    }

    private void UpdateAirbornestatus()
    {
        if (_isGrounded && _isAirborne)
            HandleLanding();

        if (!_isGrounded && !_isAirborne)
            BecomeAirborne();
    }

    private void BecomeAirborne()
    {
        _isAirborne = true;
        if(_rb.velocity.y < 0)
            ActivateCoyoteJump();
    }

    private void HandleLanding()
    {
        _isAirborne = false;
        _canDoubleJump = true;
        _dashedAirborne = false;
        

        AttemptBufferJump();
    }

    private void HandleInput()
    {
        _xInput = Input.GetAxisRaw("Horizontal");
        _yInput = Input.GetAxisRaw("Vertical");


        if(Input.GetKeyDown(KeyCode.Space)) 
        {
            JumpButton();
            RequestBufferJump();
        }
        if(Input.GetButtonDown("Fire2"))
        {
            DashButton();
        }

    }


    #region Buffer & Coyote Jump
    private void RequestBufferJump()
    {
        if(_isAirborne) _bufferJumpPressed = Time.time;
    }

    private void AttemptBufferJump()
    {
        if(Time.time < _bufferJumpPressed + _bufferJumpWindow)
        {
            _bufferJumpPressed = Time.time - 1;
            Jump();
        }
    }  

    private void ActivateCoyoteJump() => _coyoteJumpActivated = Time.time;

    private void CancelCoyoteJump() =>  _coyoteJumpActivated = Time.time - 1;
    #endregion

    private void JumpButton()
    {
        bool coyoteJumpAvalible = Time.time < _coyoteJumpActivated + _coyoteJumpWindow;
        if(_isGrounded || coyoteJumpAvalible) Jump();
        else if(_isWallDetected && !_isGrounded) WallJump();
        else if(_canDoubleJump) DoubleJump();

        CancelCoyoteJump();
    }

    private void Jump() => _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);

    private void DoubleJump()
    {   
        if(!_abilities.canDoubleJump) return;
        _iswallJumping = false;
        _canDoubleJump = false;
        _rb.velocity = new Vector2(_rb.velocity.x, _doubleJumpForce);
    }

    private void WallJump()
    {
        if(!_abilities.canWallJump) return;
        _canDoubleJump = true;
        _rb.velocity = new Vector2(_wallJumpForce.x * -_facingDirection, _wallJumpForce.y);
       
        Flip();

        StopAllCoroutines();
        StartCoroutine(WallJumpRoutine());
    }

    private IEnumerator WallJumpRoutine()
    {
        _iswallJumping = true;

        yield return new WaitForSeconds(_wallJumpDuration);

        _iswallJumping = false;
    }

    private void HandleWallSlide()
    {
        bool _canWallSlide = _isWallDetected && _rb.velocity.y < 0;
        
        float _yModifier = _yInput < 0 ? 1 : .05f;

        if(!_abilities.canWallJump || !_canWallSlide) return;

        _dashedAirborne = false;

        _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * _yModifier);
    }

    private void HandleCollision()
    {
        _isGrounded = Physics2D. Raycast(transform.position, Vector2.down, _groundCheckDistance, _whatIsGround);
        _isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * _facingDirection, _wallCheckDistance, _whatIsGround);
    }

    private void HandleAnimator()
    {
        _anim.SetFloat("speed", Mathf.Abs(_rb.velocity.x));
    }

    private void HandleMovement()
    {
        if(_isWallDetected || _iswallJumping)return;

        _rb.velocity = new Vector2(_xInput * _speed, _rb.velocity.y);
    }

    private void HandleFlip()
    {
        if(_xInput < 0 && _facingright || _xInput > 0 && !_facingright) Flip();
    }

    private void Flip()
    {
        _facingDirection *= -1;
        transform.Rotate(0, 180, 0);
        _facingright = !_facingright;
    }

    public void StopResumeAnim()
    {
        _anim.enabled = !_anim.enabled;
    }

    private void OnDrawGizmos() 
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - _groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (_wallCheckDistance * _facingDirection), transform.position.y));
    }

}
