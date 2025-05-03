using UnityEngine;
using System.Collections;

public class PlatformMover : MonoBehaviour
{
    public GameObject platform; // reference to the platform to move

    public GameObject[] myWaypoints; // array of all the waypoints

    [Range(0.0f, 10.0f)] // create a slider in the editor and set limits on moveSpeed
    public float moveSpeed = 5f; // platform move speed
    public float waitAtWaypointTime = 1f; // how long to wait at a waypoint before moving to next waypoint

    public bool loop = true; // should it loop through the waypoints

    // private variables
    Rigidbody2D rb;
    int _myWaypointIndex = 0; // used as index for myWaypoints
    float _moveTime;
    bool _moving = true;

    // Use this for initialization
    void Start()
    {
        rb = platform.GetComponent<Rigidbody2D>();

        // Disable gravity for Rigidbody2D if you don't want it to fall
        if (rb != null)
        {
            rb.gravityScale = 0f;
        }

        _moveTime = 0f;
        _moving = true;
    }

    // game loop
    void Update()
    {
        // if beyond _moveTime, then start moving
        if (Time.time >= _moveTime)
        {
            Movement();
        }
    }

    void Movement()
    {
        // if there are waypoints and the platform is moving
        if ((myWaypoints.Length != 0) && (_moving))
        {
            // Move the platform using Rigidbody2D.MovePosition
            Vector2 targetPosition = myWaypoints[_myWaypointIndex].transform.position;
            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime));

            // if the platform is close enough to the waypoint, move to the next waypoint
            if (Vector2.Distance(myWaypoints[_myWaypointIndex].transform.position, rb.position) <= 0.1f)
            {
                _myWaypointIndex++;
                _moveTime = Time.time + waitAtWaypointTime; // Set time to wait at waypoint
            }

            // If we've gone past the last waypoint, either loop or stop
            if (_myWaypointIndex >= myWaypoints.Length)
            {
                if (loop)
                    _myWaypointIndex = 0; // Reset to first waypoint
                else
                    _moving = false; // Stop movement if not looping
            }
        }
    }
}
