using UnityEngine;

// Require a TankMotor script on the same object as this script.
[RequireComponent(typeof(TankMotor))]

public class SampleAIController : MonoBehaviour {

    #region Fields
    // Public fields --v

    // The enum describing all options for handling waypoint navigation once the tank reaches the end.
    public enum LoopType { Stop, Loop, PingPong };

    [Header("Waypoint Info")]
    // The actual enum variable set by designers for how this tank should traverse the waypoints.
    public LoopType loopType;


    // Serialized private fields --v

    // Array of Transforms of the GameObjects being used as waypoints for this AI.
    [SerializeField] private Transform[] waypoints;

    // The amount of "wiggle room" the tank has for being close enough to the waypoint (allowed variance).
    [SerializeField] private float closeEnough = 1.0f;

    [Header("Component References")]
    // The transform on this gameObject.
    [SerializeField] private Transform tf;

    // The TankMotor on this gameObject.
    [SerializeField] private TankMotor motor;

    // The TankData on this gameObject.
    [SerializeField] private TankData data;


    // Private fields --v

    // The index of the current waypoint the AI is moving towards.
    private int currentWaypoint = 0;

    // The square of the closeEnough variable, computed once at awake to save computational effort.
    private float closeEnough_Squared;

    // Whether or not the tank is heading forward through the waypoint array or not (backwards).
    // Used only for the PingPong behavior.
    private bool isGoingForward = true;

    // Whether or not the tank has reached the end of the waypoint system.
    // Used only for the Stop behavior.
    private bool isStopped = false;

    #endregion Fields

    #region Unity Methods
    // Awake is performed before Start().
    void Awake()
    {
        // Set variables --v

        // Square the closeEnough variable once and save it to save the computer effort later.
        closeEnough_Squared = closeEnough * closeEnough;

        // If this var is null,
        if (tf == null)
        {
            // Get the transform component from this object.
            tf = transform;
        }

        // If this var is null,
        if (motor == null)
        {
            // Get the TankMotor component from this object.
            motor = GetComponent<TankMotor>();
        }

        // If this var is null,
        if (data == null)
        {
            // Get the TankData on this tank.
            data = GetComponent<TankData>();
        }
    }

    // Called before the first frame.
    void Start()
    {
        // If waypoints has been left empty,
        if (waypoints.Length == 0)
        {
            // then log an error.
            Debug.LogError(gameObject.name + "'s waypoints array is empty! Shutting down the AI.");
            // Turn off this script to prevent errors.
            this.enabled = false;   
        }
        // Else, the wapoints array is not empty.
        else
        {
            // If the elevation for the next waypoint does not match the tank's elevation,
            if (waypoints[currentWaypoint].transform.position.y != tf.position.y)
            {
                // then level out all of the waypoints' elevations.
                LevelOutWaypoints();
            }
        }
    }

    // Called every frame.
    void Update()
    {
        // If stopped (meaning Tank is set to Stop behavior and has reached the end of the waypoint system),
        if (isStopped)
        {
            // Do nothing.
            // This prevents tank from turning constantly while standing on the final waypoint.
        }
        // Else, if we can rotate towards the current waypoint (done during the if call),
        else if (motor.RotateTowards(waypoints[currentWaypoint].position, data.turnSpeed))
        {
            // then the tank was able to rotate towards the waypoint this frame.
            // Do nothing.
        }
        // Else, the tank was unable to rotate towards the waypoint this frame because it is already facing it.
        else
        {
            // Move forward.
            motor.Move(data.moveSpeed_Forward);
        }

        // If we are close to the waypoint,
        if (Vector3.SqrMagnitude(waypoints[currentWaypoint].position - tf.position) < (closeEnough_Squared))
        {
            // then advance to the next waypoint according to the chosen behavior.
            NextWaypoint_Dispatch();
        }
    }
    #endregion Unity Methods

    #region Dev-Defined Methods
    // Chooses the next waypoint to head towards based on the chosen behavior (loopType).
    private void NextWaypoint_Dispatch()
    {
        // Based on the chosen loop type, perform a certain action.
        switch (loopType)
        {
            // If set to Stop,
            case LoopType.Stop:
                // then perform the "next waypoint" actions for the LoopType.Stop behavior.
                NextWaypoint_Stop();
                break;

            // If set to Loop,
            case LoopType.Loop:
                // then perform the "next waypoint" actions for the LoopType.Loop behavior.
                NextWaypoint_Loop();
                break;

            // If set to PingPong,
            case LoopType.PingPong:
                // then perform the "next waypoint" actions for the LoopType.PingPong behavior.
                NextWaypoint_PingPong();
                break;
        } // currentWaypoint has been set.

        // If the elevation for the next waypoint does not match the tank's elevation,
        if (waypoints[currentWaypoint].transform.position.y != tf.position.y)
        {
            // then level out all of the waypoints' elevations.
            LevelOutWaypoints();
        }
    }

    // Performs the "next waypoint" actions for the LoopType.Stop behavior.
    private void NextWaypoint_Stop()
    {
        // If NOT already at the end,
        if (currentWaypoint < waypoints.Length - 1)
        {
            // then add 1 to go tot he next waypoint in the list.
            currentWaypoint++;
        }
        // Else, already at the end.
        else
        {
            // Tank should not stop.
            isStopped = true;
        }
    }

    // Performs the "next waypoint" actions for the LoopType.Loop behavior.
    private void NextWaypoint_Loop()
    {
        // If NOT already at the end,
        if (currentWaypoint < waypoints.Length - 1)
        {
            // then add 1 to the currentWaypoint.
            currentWaypoint++;
        }
        // Else, already at the end.
        else
        {
            // Reset the current waypoint index to 0.
            currentWaypoint = 0;
        }
    }

    // Performs the "next waypoint" actions for the LoopType.PingPong behavior.
    private void NextWaypoint_PingPong()
    {
        // If going forward,
        if (isGoingForward)
        {
            // and if NOT already at the end,
            if (currentWaypoint < waypoints.Length - 1)
            {
                // then add 1 to the currentWaypoint.
                currentWaypoint++;
            }
            // Else, already at the end (going forward).
            else
            {
                // Subtract 1 from the currentWaypoint.
                currentWaypoint--;

                // Flip the direction.
                isGoingForward = false;
            }
        }
        // Else, tank is currently heading backwards through the waypoints.
        else
        {
            // If NOT already at the beginning,
            if (currentWaypoint > 0)
            {
                // then subtract 1 from currenWaypoint.
                currentWaypoint--;
            }
            // Else, already back to the beginning of the array.
            else
            {
                // Add 1 to the index.
                currentWaypoint++;

                // Flip the direction.
                isGoingForward = true;
            }
        }
    }

    // Sets all of the appropriate waypoints to the current elevation.
    private void LevelOutWaypoints()
    {
        // Iterate through the waypoints array.
        foreach (Transform waypoint in waypoints)
        {
            // Set each transform's y equal to this tank's y.
            // This prevents the tank from attempting to move into the ground or above it.
            // First, get the position of the waypoint.
            Vector3 p = waypoint.position;

            // Change the y to match.
            p.y = tf.position.y;

            // Set the waypoint's position to the modified value.
            waypoint.position = p;
        }
    }
    #endregion Dev-Defined Methods
}
