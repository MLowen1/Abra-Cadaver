using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;

public class GroundEnemyController : MonoBehaviour
{
    public GameObject enemy;

    public enum GroundMovementState
    {
        Stop,
        MoveForward,
        Jumping,
        Patrolling,
        Dash,
    }

    public GroundMovementState groundMovementState;
    //public bool isGrounded;
    public float moveSpeed = 1f;
    //Gravity is implemented through script.
    public float gravity = 20f;

    public float jumpSpeed = 8f;

    public bool autoTurn = true;

    private Vector3 _moveDirection = Vector3.zero;
    private bool _isFacingLeft;

    //reference to the character controller 2d script
    private CharacterController2D _characterController;

    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
        groundMovementState = GroundMovementState.MoveForward; // Ensure an active state on start
        _isFacingLeft = false; // Assume the enemy faces right by default
    }

    void Update()
    {
        if (_characterController.below)
        {
            _moveDirection.y = 0f;

            if (groundMovementState == GroundMovementState.Stop)
            {
                _moveDirection = Vector3.zero;
            }

            else if (groundMovementState == GroundMovementState.MoveForward)
            {
                if (autoTurn)
                {
                    if (_characterController.left && _isFacingLeft)
                    {
                        Turn();
                    }

                    else if (_characterController.right && !_isFacingLeft)
                    {
                        Turn();
                    }

                }


                if (_isFacingLeft)
                {
                    _moveDirection.x = -moveSpeed;
                }

                else
                {
                    _moveDirection.x = moveSpeed;
                }
            }

            else if (groundMovementState == GroundMovementState.Jumping)
            {

            }

            else if (groundMovementState == GroundMovementState.Patrolling)
            {

            }

            else if (groundMovementState == GroundMovementState.Dash)
            {

            }
        }

        _moveDirection.y -= gravity * Time.deltaTime;
        _characterController.Move(_moveDirection * Time.deltaTime);

    }

    public void Turn()
    {
        if (_isFacingLeft)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            _isFacingLeft = false;
        }

        else if (!_isFacingLeft)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            _isFacingLeft = true;
        }

    }
}
