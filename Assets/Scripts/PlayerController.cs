using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Player properties
    [Header("Player Properties")]
    [Tooltip("Set the movement speed for the player")]
    public float walkSpeed = 10f;

    //Gravity is implemented through script.
    public float gravity = 20;
    //Jumping speed
    public float jumpSpeed = 15f;
    //How powerful the double jump will be
    public float doubleJumpSpeed = 10f;
    public float tripleJumpSpeed = 5f;

    //this variable define the direction of the wall jump:
    //horizontal direction
    public float xWallJumpSpeed = 15f;
    //vertical direction
    public float yWallJumpSpeed = 15f;
    //How far up the wall the player can run
    public float wallRunAmount = 8f;
    //The effect that the wall slide has on gravity
    public float wallSlideAmount = 0.1f;


    //player ability toggles
    //This allows different abilities to be switched on and off
    [Header("Player Abilities")]
    public bool canDoubleJump;
    public bool canTripleJump;
    public bool canWallJump;
    public bool canJumpAfterWallJump;
    public bool canWallRun;
    public bool canMultipleWallRun;
    public bool canWallSlide;

    //player state
    //if this is set to true then we know that the player is jumping. This bool can be used for multiple contexts, one of them being the animation system if there is a specific animation to player when the player is jumping.
    [Header("Player State")]
    public bool isJumping;
    public bool isDoubleJumping;
    public bool isTripleJumping;
    public bool isWallJumping;
    public bool isWallRunning;
    public bool isWallSliding;

    //input flags
    //[Header("Input Flags")]
    //the bool is set to true when the button is first pressed
    private bool _startJump;

    //this is set to true once the jump button is released.
    private bool _releaseJump;

    //Private variables
    //Vector2 input will be received from the input system and translated with vector2 move direction.
    private Vector2 _input;
    private Vector2 _moveDirection;

    //reference to the character controller 2d script
    private CharacterController2D _characterController;

    private bool _ableToWallRun = true;

    //Animator controller
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //If the player is not wall jumping the use player inputs
        if (!isWallJumping)
        {
            //The move direction on the x is equal to the input direction on the x. (left and right movement)
            _moveDirection.x = _input.x;
            //move direction on the x and multiply it by the walk speed. 
            _moveDirection.x *= walkSpeed;

            //Player rotation handle: Figures out the move direction and rotate the player accordingly

            //If move direction is less that 0 then the player is moving to the left so the player needs flipping
            if (_moveDirection.x < 0)
            {
                transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            }

            //The opposite it that if the player movement is more that zero reset the player rotation.
            else if (_moveDirection.x > 0)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
        }

        //Debug.Log(_moveDirection.y);

        //if below is true then we execute one block of code, if it is false execute another block.

        //this if block counts if the player is on the ground
        //if loop allows the player to utilise a light press of the button for a half jump
        if (_characterController.below) //On the ground
        {
            //When on the ground reset the y movement to 0. If this is not reset if the player falls from multiple ledges then they receive unnatural gravity acceleration. This does potentially cause another bug though. If the ray cast is too large, then it will collide with level geometry before the player leaving them hanging in the air. Ray cast size of 0.1 or 0.2 is probably best to maintain accurate collision.
            _moveDirection.y = 0f;

            //If we are not jumping and we are on the ground then is jumping should be false.
            isJumping = false;

            //clear flags for in air abilities
            isDoubleJumping = false;
            isTripleJumping = false;
            isWallJumping = false;

            //check that the jump button was pressed this frame
            if (_startJump)
            {
                _animator.SetBool("Is Jumping", true);
                //as soon as it has begun, set the boolean to false, it has done it's job
                _startJump = false;
                //move direction on the y is set to the jump speed
                _moveDirection.y = jumpSpeed;
                //is jumping will equal true
                isJumping = true;
                //calls the disable ground check method when the character jumps
                _characterController.DisableGroundCheck();
                _ableToWallRun = true;
            }
        }
        else //If the player is in the air
        {

            _animator.SetBool("Is Jumping", false);
            //the player letting go of the jump button
            if (_releaseJump)
            {
                //if player let go set the bool to false once the code has triggered
                _releaseJump = false;
                //is the player still moving upwards?
                if (_moveDirection.y > 0)
                {
                    //if it is, set the move direction to a 0.5  float
                    _moveDirection.y *= 0.5f;
                }

            }

            //pressed jump button in air
            //Using the is startjump boolean to check that the jump button has been pressed.
            if (_startJump)
            {

                //triple jump
                //Check that the ability is enabled, check the tripple jump before the double jump because if the double jump fires first then the tripple jump will be activated immediatly.. Additional jumps can be added using this method, but they need to be added before the previous jumps check.
                if(canTripleJump && (!_characterController.left && !_characterController.right))
                {
                    
                    if (isDoubleJumping && !isTripleJumping)
                    {
                        //Move direction on the y axis equals tripple jump speed.
                        _moveDirection.y = tripleJumpSpeed;
                        isTripleJumping = true;
                    }
                }


                //double jump
                //Check the double jump. Has the player already double jumped? is double jumping false?
                if (canDoubleJump && (!_characterController.left  && !_characterController.right))
                {
                    if (!isDoubleJumping)
                    {

                        //Move direction on the y axis equals double jump speed.
                        _moveDirection.y = doubleJumpSpeed;
                        isDoubleJumping = true;
                    }
                }

                //wall jump

                //If the wall jump ability is toggled and they player has an object to the lef or right then wall jump should be enabled
                if (canWallJump && (_characterController.left || _characterController.right))
                {
                    //Figure out which direction the player is moving in and which direction the wall is in:
                    if (_moveDirection.x <= 0 && _characterController.left)
                    {
                        //movementspeeds and sprite rotations
                        _moveDirection.x = xWallJumpSpeed;
                        _moveDirection.y = yWallJumpSpeed;
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }
                    else if (_moveDirection.x >= 0 && _characterController.right)
                    {
                        _moveDirection.x = -xWallJumpSpeed;
                        _moveDirection.y = yWallJumpSpeed;
                        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    }

                    //isWallJumping = true;

                    StartCoroutine("WallJumpWaiter");

                    if (canJumpAfterWallJump)
                    {
                        isDoubleJumping = false;
                        isTripleJumping = false;
                    }
                }


                //Clears the jump press when the jump has been detected
                _startJump = false;
            }

            //Wall running
            if (canWallRun && (_characterController.left || _characterController.right))
            {
                if (_input.y > 0 && _ableToWallRun)
                {
                    _moveDirection.y = wallRunAmount;

                    if (_characterController.left)
                    {
                        _moveDirection.x = walkSpeed;
                        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    }
                    else if (_characterController.right)
                    {
                        _moveDirection.x = -walkSpeed;
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    }

                    StartCoroutine("WallRunWaiter");
                }

            }
            else
            {
                if (canMultipleWallRun)
                {
                    StopCoroutine("WallRunWaiter");
                    _ableToWallRun = true;
                    isWallRunning = false;
                }
            }

            GravityCalculations();
        }

        //Pass the values to the character controller and the move method that is there.
        _characterController.Move(_moveDirection * Time.deltaTime);
    }


    //Gravity method created as there are several abilities which affect gravity.
    void GravityCalculations()
    {
        //Check the move direction of y is greater than 0, if it is then the player is still moving upwards.
        if (_moveDirection.y > 0f && _characterController.above)
        {
            //Set the move direction to 0 if there is an object above.
            _moveDirection.y = 0f;
        }

        //Wall slide gravity calculation. If the player is in contact with a wall and the wall slide ability is enabled then the player will slide down the wall.
        if (canWallSlide && (_characterController.left || _characterController.right))
        {
            if (_characterController.hitWallThisFrame)
            {
                _moveDirection.y = 0f;
            }
        
            //If the player is moving down the wall then the wall slide amount is applied to the gravity calculation.
            if (_moveDirection.y <= 0)
            { 
            _moveDirection.y -= (gravity * wallSlideAmount) * Time.deltaTime;
            }

            //If the player is moving up the wall then the normal gravity calculation is applied.
            else
            {
                _moveDirection.y -= gravity * Time.deltaTime;
            }
        }
        // Normal gravity calculation sits inside the else block. Basically, if the player is not in contact with a wall then normal gravity will be applied.
        else
        {
            // Move direction on the y and multiply it by the gravity and time delta time.
            _moveDirection.y -= gravity * Time.deltaTime;
        }
        
    }

    //Input Methods
    //The new input system fires an event when an input is detected. This method will be fired when the input event is received.


    public void OnMovement(InputAction.CallbackContext context)
    {
        //When the even fires the input variable equals context.readvalue , which represents any input recied which is direction based.
        _input = context.ReadValue<Vector2>();
        if(_input != Vector2.zero)
        {
           // _animator.SetFloat("moveX", _input.x);
           // _animator.SetFloat("moveY", _input.y);
           
            _animator.SetFloat("Speed", Mathf.Abs(_input.x));
        }

        else 

        if(_input == Vector2.zero)
        {
            _animator.SetFloat("Speed", 0);
        } 
        
    }

    public void OnJump (InputAction.CallbackContext context)
    //This can give us information about the button press itself. Button presses have three different phases. the first being when the press is started, the second when the button is held down and the third when the button is released/ cancelled.

    //The following if statement checks the context of the button press in order to look for the start and finish of the press.

    //This if and else block checks what state the button press is in.
    {
        if (context.started)
        {

            //if we start the jump then the start jump needs to be set as true.
            _startJump = true;
            //clears the boolean
            _releaseJump = false;
        }
        else if (context.canceled)
        {
            //if we end the jump then the end needs to be set as true.
            _releaseJump = true;
            //When the jump button is released the start button is reset.
            _startJump = false;
        }

    }

    //coroutines
    IEnumerator WallJumpWaiter()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(0.5f);
        isWallJumping = false;
    }

    IEnumerator WallRunWaiter()
    {
        isWallRunning = true;
        yield return new WaitForSeconds(0.5f);
        isWallRunning = false;
        if (!isWallJumping)
        { 
        _ableToWallRun = false;
        }

    }
}