using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirEnemyController : MonoBehaviour
{
    public GameObject enemy;
    public GameObject player;

    public enum AirMovementState
    {
        Stop,
        Dash,
        Float,
        MoveTowards,
        Move,
        Patrol,
        Animated,
    }

    public enum CollisionBehaviour
    {
        None,
        Rebound,
        Fall,
        Explode,
        Disappear,
    }

    public AirMovementState airMovementState;
    public CollisionBehaviour collisionBehaviour;
    private Rigidbody2D _rigidbody;

    //generic variables
    public bool usePhysics = true;
    public float thrust = 10f;

    //reference to the character controller 2d script
    private CharacterController2D _characterController;

    //move towards
    public bool autoTargetPlayer = true;
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
        _rigidbody = gameObject.GetComponent<Rigidbody2D>();

        if (autoTargetPlayer)
        {
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (airMovementState.Equals(AirMovementState.Stop))
        {
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;
        }
        else if (airMovementState.Equals(AirMovementState.Dash))
        {

        }
        else if (airMovementState.Equals(AirMovementState.Float))
        {

        }
        else if (airMovementState.Equals(AirMovementState.MoveTowards))
        {
            MoveTowards(target);
        }
        else if (airMovementState.Equals(AirMovementState.Move))
        {
            OnMove(transform.right);
        }
        else if (airMovementState.Equals(AirMovementState.Patrol))
        {

        }
        else if (airMovementState.Equals(AirMovementState.Animated))
        {

        }
    }

    public void OnMove(Vector3 moveDirection)
    {
        if (usePhysics)
        {
            _rigidbody.AddForce(moveDirection * thrust);
        }
        else
        {
            _rigidbody.MovePosition(transform.position + (moveDirection * thrust * Time.deltaTime));
        }
    }

    public void MoveTowards(Transform target)
    {
        if (target == null) return;

        // Calculate the direction to the target
        Vector3 direction = (target.position - transform.position).normalized;

        // Move in the calculated direction
        OnMove(direction);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
