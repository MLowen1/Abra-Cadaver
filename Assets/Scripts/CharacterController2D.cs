using System.Collections;
using UnityEngine;
using GlobalTypes;
using System.Drawing;

public class CharacterController2D : MonoBehaviour
{
    //This will be how far beneath the player the ray casts will be protected. This may vary depending on the scene.
    public float rayCastDistance = 0.2f;

    //The layer mask allows certain layers to be filtered.
    public LayerMask layerMask;

    public float slopeAngleLimit = 45f;
    public float downForceAdjustment = 1.2f;
    public float coyoteTime = 0.2f;

    //flags (simple boolean which tell whether the player is in contact with anything around them. True then there is something below and false then there is nothing below.)
    public bool below;
    public bool left;
    public bool right;
    public bool above;

    //this keeps track of the ground type the player is standing on:
    public GroundType groundType;

    //boolean is set to true at the exact frame the player hits the ground:
    public bool hitGroundThisFrame;
    //boolean is set to true at the exact frame the player hits a wall:
    public bool hitWallThisFrame;

    private Vector2 _moveAmount;
    private Vector2 _currentPosition;
    private Vector2 _lastPosition;

    private Rigidbody2D _rigidbody;
    private CapsuleCollider2D _capsuleCollider;

    //Vector 2 array, to store multiple vector 2's which are ray-cast origin positions. This array is 3 positions in size. pos 1 will be slightly to the left of the player pos 2 in the middle beneath and pos 3 slightly to the right.
    private Vector2[] _rayCastPosition = new Vector2[3];
    //This is an array of ray-cast 2d's the ray-cast hit is generated when a ray-cast is done and it will give information about the object that is hit with the ray.
    private RaycastHit2D[] _rayCastHits = new RaycastHit2D[3];

    private bool _disableGroundCheck;

    private Vector2 _slopeNormal;
    private float _slopeAngle;

    private bool _inAirLastFrame;
    private bool _noSideCollisionsLastFrame;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();
        _capsuleCollider = gameObject.GetComponent<CapsuleCollider2D>();
    }

    //The fixed update loop is tied to physics simulations and it can only run a set number of times per second whereas the update loop can actually be frame rate dependant so it may fire more or less frequently depending on the computer running the program.

    // Update is called once per frame
    void Update()
    {
        //At the beginning of the loop before any calculations are done we need to see if the player is in the air or not. InAirLast frame is the opposite of the below flag.
        _inAirLastFrame = !below;

        // If no side collisions were detected last frame, then the player is not in contact with a wall and the boolean is set to true.
        _noSideCollisionsLastFrame = (!left && !right);

        //Figure out the players position at the beginning of the frame.
        _lastPosition = _rigidbody.position;
        //The current position is equal to the last position plus a move amount. The move amount is going to be fed through the player controller depending on the input.

        //Using trigonometry and the Tan function we can adjust the player direction. As the angle is known and also the direction of movement on the x (adjacent) we need to find the opposite for the movement y axis. We use the mathf function and provide it with the angle and the x movement to work out how much movement should be in the y axis.
        //if the slop angle is not equal to zero, there is no point working this out on a flat surface, so it is ruled out unless the player is on a slope.
        if (_slopeAngle != 0 && below == true)
        {
            //If both of the above conditions are met then there are two more conditions that must be met. If the move amount is positive, the player is travelling to the right, and the downward slope is greater than 0, or then if the player is travelling to the left and the slop angle is less than 0.
            if((_moveAmount.x > 0f && _slopeAngle > 0f) || (_moveAmount.x < 0f && _slopeAngle < 0f))
            {
                //If either of those conditions are met then a movement adjustment needs to be applied on the y axis. Mathf.Abs gives an absolute value to work with. It works out the opposite of the triangle for the downward movement. The slope angle is in degrees but Mathf uses radiens so Deg2rad is used to convert that value. 
                _moveAmount.y = -Mathf.Abs(Mathf.Tan(_slopeAngle * Mathf.Deg2Rad) * _moveAmount.x);
                //Increases the downward motion by about 20% to avoid bouncing on downward slopes with higher velocity. It may be needed to be adjusted depending on the size of raycast and angle of the slopes.
                _moveAmount.y *= downForceAdjustment;
            }

        }

        _currentPosition = _lastPosition + _moveAmount;
        //Get a reference to the rigid body and move the position. Because the move position is a part of the rigidbody it will actually perform physics calculations as it translates and will calulate interactions with ther objects.
        _rigidbody.MovePosition(_currentPosition);
        //Zero out the move amount because the player has moved by that amount.
        _moveAmount = Vector2.zero;

        //Only check groundcheck if diasble ground check is false. if we're not checking the ground we just skip this entire method.
        if (!_disableGroundCheck)
        {
            CheckGrounded();
        }

        CheckOtherCollisions();

        //It the player is now on the ground but was in the air last frame, then set hit ground to be true.
        if (below && _inAirLastFrame)
        {
            hitGroundThisFrame = true;
        } 
        else
        {
            hitGroundThisFrame = false;
        }

        if (right || left && _noSideCollisionsLastFrame)
        {
            hitWallThisFrame = true;
        }
        else
        {
            hitWallThisFrame = false;
        }
    }

    //Gets the move amount which will be called from a regular update loop which can happen more frequenly than a fixed update.
    public void Move(Vector2 movement)
    {
        //This is added using += because the moveamount needs to be an acumalitive amount from every time this fires. 
        _moveAmount += movement;
        Debug.Log("Moving");
    }

    private void CheckGrounded()
    {

        //new version of raycast, is much simpler. Use RaycastHit2d raycast to hold the results of the raycast. Instead of using a raycast use a capsule cast, which projects a capsule into the scene which can be used to detect other things that overlap the capsule. It's liek a raycast however in  the shape of a capsule. This will cover completely beneith the player without having to use multiple raycasts.

        RaycastHit2D hit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical,
           0f, Vector2.down, rayCastDistance, layerMask);

        //If the capsule cast hit an object which has a collider then work out the ground type:
        if (hit.collider)
        {
            //Work out the ground type:
            groundType = DetermineGroundType(hit.collider);

            _slopeNormal = hit.normal;
            //We want to convert the normal into a representation of an angle so using the signedangle function from the vector2 library. Slop ange equals vector 2 . signed angle. This works out the angle between two vector2's. The first ange it is fed is the slop normal, the second is the Vector2 up. This will calculate tha angle between the up vector2 and the normal of the slope.
            _slopeAngle = Vector2.SignedAngle(_slopeNormal, Vector2.up);

            //Check that the surface doesnt exceed the slope limit, it it does, then treeat the player as if they are nt grounded.
            if (_slopeAngle > slopeAngleLimit || _slopeAngle < -slopeAngleLimit)
            {
                below = false;
            }
            else
            {
                below = true;
            }
        }
        else
        {
            if (coyoteTime > 0f)
            {
                //check to see if there is a coyoteTime (ie it is greater than 0) and then fires a coroutine. The coroutine can then simply be used to add a delay to the below flag being checked.Its basically exactly the same code but is executed after a slight delay.
                StartCoroutine("CoyoteTimeDelay");
            }
            else
            {
                //clears the ground type that the player is standing on if in the air.
                groundType = GroundType.None;
                below = false;
            }
        }
    }

    //After checking that the player is grounded, contact ith other level geometry is checked. Check left and right use a box cast whereas above uses a capule cast, similar to the grounded check.
    private void CheckOtherCollisions()
    {
        //check left
        RaycastHit2D leftHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, Vector2.left,
            rayCastDistance * 2f, layerMask);

        if (leftHit.collider)
        {
            left = true;
        }
        else
        {
            left = false;
        }


        //check right
        RaycastHit2D rightHit = Physics2D.BoxCast(_capsuleCollider.bounds.center, _capsuleCollider.size * 0.75f, 0f, Vector2.right,
            rayCastDistance * 2f, layerMask);

        if (rightHit.collider)
        {
            right = true;
        }
        else
        {
            right = false;
        }

        //check above
        RaycastHit2D aboveHit = Physics2D.CapsuleCast(_capsuleCollider.bounds.center, _capsuleCollider.size, CapsuleDirection2D.Vertical,
           0f, Vector2.up, rayCastDistance, layerMask);

        if (aboveHit.collider)
        {
            above = true;
        }
        else
        {
            above = false;
        }
    }

    private void DrawDebugRays(Vector2 direction, UnityEngine.Color color)
    {
        for (int i = 0; i < _rayCastPosition.Length; i++)
        {
            Debug.DrawRay(_rayCastPosition[i], direction * rayCastDistance, color);
        }
    }

    //When the player jumps the and moves up there may be one or two frames where the raycasts are hitting the ground as the player moves up. So as soon as we hit jump we are going to toggle off ground detection on the character controller for a split second so that we dont get any false reprting on wheter the below flag is true.

    //This method disables the ground check temporarily whenever this is called.
    public void DisableGroundCheck()
    {
        //Set below to false
        below = false;
        //Set a flag to tell the code to stop ground checking. If the flag is true the disable the ground check temporarity.
        _disableGroundCheck = true;
        //start a coroutine. Coroutines allow us to do things with timers. So if we want something to happen say after a fraction of a second, we can use a coroutine for that.
        StartCoroutine("EnableGroundCheck");
    }

    //An Enumerator is a list of static values.
    IEnumerator EnableGroundCheck()
    {
        //Set to one tenth of a second, which should give plenty of time to leave the ground before enabling the ground check. This could be increased if needed. 
        yield return new WaitForSeconds(0.1f);
        _disableGroundCheck = false;
    }

    IEnumerator CoyoteTimeDelay()
    {
        yield return new WaitForSeconds(coyoteTime);
        groundType = GroundType.None;
        below = false;
    }

    //private method to return a ground type which will be passed a colllider2d.
    private GroundType DetermineGroundType(Collider2D collider)
    {
        //If the object is a special ground type then it needs to return what type it is otherwise it need to  return that it is normal ground. The if statement will try to get the groundeffector component.
        if (collider.GetComponent<GroundEffector>())
        {
            //Method looks at the ground effector and finds out what ground type it is through returning ground type
            GroundEffector groundEffector = collider.GetComponent<GroundEffector>();
            return groundEffector.groundType;
        }
        else
        {
            //if the ground we are in contact with doesnt have the component then we know its bog standard level geometry.
            return GroundType.LevelGeometry;
        }
    }
}