using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyController : MonoBehaviour
{
    public GameObject enemy;

    //Gravity is implemented through script.
    public float gravity = 20;

    private Vector2 _moveDirection;

    //reference to the character controller 2d script
    private CharacterController2D _characterController;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = gameObject.GetComponent<CharacterController2D>();
        Debug.Log("Start method called");
    }

    // Update is called once per frame
    void Update()
    {
        GravityCalculations();
        Patrol();
    }

    private void ApplyGravity()
    {
        Vector3 position = enemy.transform.position;
        position.y += _moveDirection.y * Time.deltaTime;
        enemy.transform.position = position;
    }

    // Enemy patrolling
    private void Patrol()
    {
        // Move the enemy object
        enemy.transform.position += new Vector3(0.1f, 0, 0);
    }

    void GravityCalculations()
    {

        //Check the move direction of y is greater than 0, if it is then the player is still moving upwards.
        if (_moveDirection.y > 0f && _characterController.above)
        {
            //Set the move direction to 0 if there is an object above.
            _moveDirection.y = 0f;
        }

        _moveDirection.y -= gravity * Time.deltaTime;
    }
}
