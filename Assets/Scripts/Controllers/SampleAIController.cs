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
    }

    // Called every frame.
    void Update()
    {
        // If we can rotate towards the current waypoint (done during the if call),
        if (motor.RotateTowards(waypoints[currentWaypoint].position, data.turnSpeed))
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
            NextWaypoint();
        }
    }

    // Chooses the nexy waypoint to head towards based on the chosen behavior (loopType).
    private void NextWaypoint()
    {
        // Based on the chosen loop type, perform a certain action.
        switch (loopType)
        {
            // If set to Stop,
            case LoopType.Stop:
                // and if NOT already at the end,
                if (currentWaypoint < waypoints.Length - 1)
                {
                    // then add 1 to go tot he next waypoint in the list.
                    currentWaypoint++;
                }
                // Else, already at the end.
                else
                {
                    // Do nothing.
                }
                break;

            // If set to Loop,
            case LoopType.Loop:
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
                break;

            // If set to PingPong,
            case LoopType.PingPong:
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
                    break;
        }
    }
    #endregion Unity Methods

    #region Dev-Defined Methods

    #endregion Dev-Defined Methods
}
