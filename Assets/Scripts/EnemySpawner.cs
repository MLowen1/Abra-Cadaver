using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;

    private float timer = 0f;
    private float interval = 1f;
    private int tickCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Increment the timer
        timer += Time.deltaTime;

        // Check if the interval has passed
        if (timer >= interval)
        {
            // Reset the timer
            timer = 0f;

            // Increment the tick count
            tickCount++;

            // Check if it's time to instantiate an enemy object
            if (tickCount % 5 == 0)
            {
                InstantiateEnemy();
            }
        }
    }

    // Instantiate an enemy object
    private void InstantiateEnemy()
    {
        GameObject.Instantiate(enemy, transform.position, Quaternion.identity);
        Debug.Log("Enemy instantiated");
    }
}
