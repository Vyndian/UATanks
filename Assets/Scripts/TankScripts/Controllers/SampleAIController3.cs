using UnityEngine;
using System;

// Require a TankMotor script on the same object as this script.
[RequireComponent(typeof(TankMotor))]

public class SampleAIController3 : MonoBehaviour {

    #region Fields
    // Public fields --v


    // Serialized private fields --v

    // The amounce of time this tank will stay in avoidance stage 2.
    [SerializeField] private float avoidanceTime = 2.0f;

    [Header("Tracker: Pursuit & Flee Variables")]
    // The enum for the attack mode.
    [SerializeField] private AttackMode  attackMode = AttackMode.Chase;

    // The Transform of the target this tank will Chase.
    [SerializeField] private Transform tracker_Target;

    [Header("Component References")]
    // The transform on this gameObject.
    [SerializeField] private Transform tf;

    // The TankMotor on this gameObject.
    [SerializeField] private TankMotor motor;

    // The TankData on this gameObject.
    [SerializeField] private TankData data;


    // Private fields --v

    // The current stage of avoidance the tank is in.
    // Value of 0 indicates that tank is in its normal move state (not avoiding obstacles).
    private int avoidanceStage = 0;

    // The time that the tank will exit avoidance stage 2.
    private float avoidanceStage2_ExitTime;

    // The enum definition for the attack mode.
    private enum AttackMode { Chase };

    // The direction the tank should turn when avoiding obstacles. -1 for left, 1 for right.
    private int avoidance_TurnDirection = -1;
    #endregion Fields


    #region Unity Methods
    // Awake is performed before Start().
    public void Awake()
    {
        // Set variables --v

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
    public void Start()
    {

    }

    // Called every frame.
    public void Update()
    {
        // If in Chase mode,
        if (attackMode == AttackMode.Chase)
        {
            // and if we are in obstacle avoidance mode,
            if (avoidanceStage != 0)
            {
                // then perform avoidance protocalls.
                DoAvoidance();
            }
            // Else, we are NOT in obstacle avoidance mode.
            else
            {
                // Perform chase protocalls.
                DoChase();
            }
        }
    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Perform obstacle avoidance based on which stage the tank is in.
    private void DoAvoidance()
    {
        // If the tank is in stage 1,
        if (avoidanceStage == 1)
        {
            // Then perform stage 1 obstacle avoidance.
            ObstacleAvoidance_Stage1();
        }
        // Else, if tha tank is in stage 2,
        else if (avoidanceStage == 2)
        {
            // Then perform stage 2 obstacle avoidance.
            ObstacleAvoidance_Stage2();
        }
        // Else, we can't move forward.
        else
        {
            // Back to ObstacleAvoidance stage 1.
            avoidanceStage = 1;
        }
    }

    // Perform obstacle avoidance for stage 1, turning until there is no obstacle blocking.
    private void ObstacleAvoidance_Stage1()
    {
        // Turn according to avoidance_TurnDirection. 1 for right, -1 for left.
        motor.Turn(avoidance_TurnDirection * data.turnSpeed);

        // If moving forward 1 second is now possible,
        if (CanMoveForward(data.moveSpeed_Forward))
        {
            // then move on to stage 2.
            avoidanceStage = 2;

            // Set the amount of time the tank will stay in stage 2.
            avoidanceStage2_ExitTime = avoidanceTime;
        }
        // Else, this will be performed again in the next frame.
    }

    // Perform obstacle avoidance for stage 2, moving forward for "avoidanceTime" seconds or until blocked again.
    private void ObstacleAvoidance_Stage2()
    {
        // If moving forward 1 second is now possible,
        if (CanMoveForward(data.moveSpeed_Forward))
        {
            // then move forward.
            motor.Move(data.moveSpeed_Forward);

            // Subtract from the exitTime timer.
            avoidanceStage2_ExitTime -= Time.deltaTime;

            // If the tank has now spent the appropriate amount of time in stage 2,
            if (avoidanceStage2_ExitTime <= 0)
            {
                // then return to chase mode by entering stage 0.
                avoidanceStage = 0;
            }
        }
        // Else, the tank cannot move forward for 1 second (there is an obstacle blocking).
        else
        {
            // Go back to stage 1 of avoidance.
            avoidanceStage = 1;
        }
    }

    // Perform chase protocalls.
    private void DoChase()
    {
        // Rotate a bit towards the target.
        motor.RotateTowards(tracker_Target.position);

        // Check if we can move forward for 1 second without hitting an obstacle.
        // Pass in "data.moveSpeed_Forward" because that is how far the tank can move in 1 second.
        if (CanMoveForward(data.moveSpeed_Forward))
        {
            // Move forward.
            motor.Move(data.moveSpeed_Forward);
        }
        // Else, there is an obstacle in the way within 1 second's travel.
        else
        {
            // Enter obstacle avoidance mode.
            avoidanceStage = 1;
        }
    }

    // Checks if the tank can move forward to its current target (or if an obstacle is blocking).
    private bool CanMoveForward(float speed)
    {
        // Create a raycast variable to hold the "out" in the later raycast.
        RaycastHit hit;

        // Cast the ray forward to where the tank could move in 1 second's time.
        // If it hit something (an obstacle),
        if (Physics.Raycast(tf.position, tf.forward, out hit, speed))
        {
            // then the raycast hit something. If that something was NOT the player,
            if (!hit.transform.gameObject.GetComponent<InputController>())
            {
                // Get the bounds of the obstacle hit.
                Bounds bounds = hit.transform.GetComponentInChildren<Renderer>().bounds;

                // Determine which direction the tank should turn to most efficiently evade the obstacle.
                avoidance_TurnDirection = DetermineTurnDirection(hit.transform, speed);

                // Return false (we can NOT move forward for 1 second because of an obstacle).
                return false;
            }
            // Else, it was the player.
            else
            {
                // The player is not an obstacle, so the tank can move forward. Return true.
                return true;
            }
        }
        // Else, the way is clear.
        else
        {
            // Return true (we CAN move forward for 1 second without hitting an obstacle).
            return true;
        }
    }

    // Determines if the tank should turn left or right to most easily avoid the obstacle.
    // This is a simple, non-comprehensive solution.
    private int DetermineTurnDirection(Transform obstacle, float speed)
    {
        // The distance these rays should cast to. Double the speed of the original ray that found an obstacle.
        float raySpeed = 2 * speed;

        // Holds the hit info for the raycasts.
        RaycastHit hit;

        // The angle at which the raycast should be cast.
        // First, set it to a 45 degree angle to the tank's left.
        Vector3 rayAngle = (tf.forward - tf.right).normalized;

        // Send out a raycast at that angle. If it hits something,
        if (Physics.Raycast(tf.position, rayAngle, out hit, raySpeed))
        {
            // and if that something is not itself or the original obstacle,
            if (hit.transform != tf && hit.transform != obstacle)
            {
                // Then turning left will be faster.
                // Return -1.
                return -1;
            }
            // Else, the raycast hit either this tank or the original obbstacle.
            else
            {
                // Do nothing.
            }
        }
        // Else, the raycast hit nothing.
        else
        {
            // Turning left will be faster.
            // Return -1.
            return -1;
        }

        // The ray must have hit either itself or the original obstacle. Adjust to send new ray to the right.
        // Set the rayAngle to 45 degrees to the right.
        rayAngle = (tf.forward + tf.right).normalized;

        // Send out a raycast at that angle. If it hits something,
        if (Physics.Raycast(tf.position, rayAngle, out hit, raySpeed))
        {
            // and if that something is not itself or the original obstacle,
            if (hit.transform != tf && hit.transform != obstacle)
            {
                // Then turning right will be faster.
                // Return 1.
                return 1;
            }
            // Else, the raycast hit either this tank or the original obbstacle.
            else
            {
                // Do nothing.
            }
        }
        // Else, the raycast hit nothing.
        else
        {
            // Turning right will be faster.
            // Return 1.
            return 1;
        }

        // Both rays must have hit either the tank or the original obstacle.
        // Return -1 to turn left as default.
        return -1;
    }
    #endregion Dev-Defined Methods
}
