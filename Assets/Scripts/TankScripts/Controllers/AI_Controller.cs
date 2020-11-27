using UnityEngine;

public class AI_Controller : MonoBehaviour {

    #region Fields
    // Enum definitions.
    // The definition for the enum for the overall state of the tank AI.
    public enum AIState { Chase, ChaseAndFire, CheckForFlee, Flee, Rest };

    // The definition for the enum for whether this tank heals/repairs per second or per tick.
    public enum RepairType { PerSecond, PerTick };


    [Header("AI variables")]
    // The state of this tank AI.
    public AIState aiState = AIState.Chase;

    // The radius at which the tank can sense other tanks.
    [SerializeField] private float aiSenseRadius;

    // The square of aiSenseRadius, mathed out in Awake.
    private float aiSenseRadius_Squared;

    // The time that the tank entered this state.
    private float timeStateEntered;


    [Header("Repair/Healing variables")]
    // The method of which this tank heals, either per second or per tick.
    public RepairType repairType = RepairType.PerSecond;

    // The amount of time between each tick, is repair/healing is done that way.
    [SerializeField, Tooltip("Time between each tick (in seconds) for healing purposes, if repairType == PerTick")]
    private float tickLength = 1.0f;

    // The amount of HP healed per second when in rest state for this tank.
    [SerializeField, Tooltip("Either the amount healed per second or per tick, depending on the Repair Type.")]
    private float restingHealRate;

    // The time that the last tick occurred. Initialized at 0 so it won't be null for the first check.
    private float timeLastTick = 0.0f;


    [Header("Chase variables")]
    // The Transform of the target this tank will Chase.
    [SerializeField] private Transform target;


    [Header("Obstacle Avoidance variables")]
    // The multiplier for the original raycast's length when
    // firing raycasts at 45 deg angles to determine which direction to turn. 1 for the same, 2 for double, etc.
    [SerializeField] private float raycastLength_Modifier = 1.2f;

    // The amounce of time this tank will stay in avoidance stage 2.
    [SerializeField] private float avoidanceTime = 2.0f;

    // The current stage of avoidance the tank is in.
    // Value of 0 indicates that tank is in its normal move state (not avoiding obstacles).
    private int avoidanceStage = 0;

    // The direction the tank should turn when avoiding obstacles. -1 for left, 1 for right.
    private int avoidance_TurnDirection = -1;

    // The time that the tank will exit avoidance stage 2.
    private float avoidanceStage2_ExitTime;


    [Header("Flee variables")]
    // The amount of time the tank should flee before it starts attempting obstacle avoidance (still while fleeing).
    // This allows the tank to get partially turned around before trying to avoid obstacles it doesn't need to.
    [SerializeField] private float duration_FleeBeforeAvoiding = 3.0f;

    // The duration the tank will flee before checking again if it is safe to rest. In seconds.
    [SerializeField] private float duration_FleeBeforeChecking = 30.0f;

    // The distance ahead of the tank that the tank chooses as a spot to flee towards.
    [SerializeField] private float fleeDistance = 1.0f;

    // A decimal representing the threshold of percentage of health that the tank will flee.
    // Determined at Start based on value held in data.
    private float fleeThreshold_Decimal;

    [Header("Component References")]
    // The transform on this gameObject.
    [SerializeField] private Transform tf;

    // The TankMotor on this gameObject.
    [SerializeField] private TankMotor motor;

    // The TankData on this gameObject.
    [SerializeField] private TankData data;

    // The TankCannon on this gameObject.
    [SerializeField] private TankCannon cannon;

    #endregion Fields

    #region Unity Methods
    // Performed before Start.
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
            // Get the TankMotor on this gameObject.
            motor = gameObject.GetComponent<TankMotor>();
        }

        // If this var is null,
        if (data == null)
        {
            // Get the TankData on this gameObject.
            data = gameObject.GetComponent<TankData>();
        }

        // If this var is null,
        if (cannon == null)
        {
            // Get the TankCannon on this gameObject.
            cannon = gameObject.GetComponent<TankCannon>();
        }

        // Get the square of aiSenseRadius and save it.
        aiSenseRadius_Squared = aiSenseRadius * aiSenseRadius;
    }

    // Called before the first frame.
    public void Start()
    {
        // Take the fleethresholdPercentage from data and save it as a decimal here.
        // Must be in Start so that data has had time to initialize its values.
        fleeThreshold_Decimal = data.fleeThreshold_Percentage / 100;
    }

    // Called every frame.
    public void Update()
    {
        // Act depending on the current asState.
        switch (aiState)
        {
            // In the case that the state is Chase,
            case AIState.Chase:
                // and if currently performing obstacle avoidance,
                if (avoidanceStage != 0)
                {
                    // then perform avoidance protocalls.
                    DoAvoidance();
                }
                // Else, not avoiding obstacles.
                else
                {
                    // Perform Chase protocalls.
                    DoChase();
                }

                // Check for transitions.
                // If the tank's health as fallen to or below the theshhold for fleeing,
                if (data.currentHealth <= (fleeThreshold_Decimal * data.maxHealth))
                {
                    // then change state to CheckForFlee.
                    ChangeState(AIState.CheckForFlee);
                }
                // Else, tank is above the flee threshold. If target is within firing range,
                else if (Vector3.SqrMagnitude(target.position - tf.position) <= aiSenseRadius)
                {
                    // then change the state to ChaseAndFire.
                    ChangeState(AIState.ChaseAndFire);
                }
                    break;


            // In the case that the state is ChaseAndFire,
            case AIState.ChaseAndFire:
                // and if currently performing obstacle avoidance,
                if (avoidanceStage != 0)
                {
                    // then perform avoidance protocalls.
                    DoAvoidance();
                }
                // Else, not avoiding obstacles.
                else
                {
                    // Perform ChaseAndFire protocalls by chasing and firing.
                    DoChase();
                    // Firing delay automated by the TankCannon itself in conjunction with TankData.
                    cannon.Fire(data.shellSpeed);
                }

                // Check for transition conditions.
                // If the tank's health as fallen to or below the theshhold for fleeing,
                if (data.currentHealth <= (fleeThreshold_Decimal * data.maxHealth))
                {
                    // then change state to CheckForFlee.
                    ChangeState(AIState.CheckForFlee);
                }
                // Else, tank is above the flee threshold. If target is NOT within firing range,
                else if (Vector3.SqrMagnitude(target.position - tf.position) > aiSenseRadius)
                {
                    // then change the state to Chase.
                    ChangeState(AIState.Chase);
                }
                break;


            // In the case that the state is CheckForFlee,
            case AIState.CheckForFlee:
                // then perform CheckForFlee protocalls.
                CheckForFlee();

                // Test for transition conditions.
                // If target is within range of the tank's senses,
                if (Vector3.SqrMagnitude(target.position - tf.position) <= aiSenseRadius)
                {
                    // then change the state to Flee.
                    ChangeState(AIState.Flee);
                }
                // Else, the tank cannot sense the target.
                else
                {
                    // Change state to Rest.
                    ChangeState(AIState.Rest);
                }
                break;


            // In the case that the state is Flee,
            case AIState.Flee:
                // and if currently performing obstacle avoidance,
                if (avoidanceStage != 0)
                {
                    // then perform avoidance protocalls.
                    DoAvoidance();
                }
                // Else, not avoiding obstacles.
                else
                {
                    // Perform Flee protocalls.
                    DoFlee();
                }

                // Test for transition conditions.
                // If enough time has passed since entering Flee state,
                if ((timeStateEntered + duration_FleeBeforeChecking) <= Time.time)
                {
                    // then change the state back to CheckForFlee.
                    ChangeState(AIState.CheckForFlee);
                }
                break;


            // In the case that the state is Rest,
            case AIState.Rest:
                // then perform Rest protocalls.
                DoRest();

                // Test for transtion conditions.
                // If reached max health while resting,
                if (data.currentHealth >= data.maxHealth)
                {
                    // then change state back to Chase.
                    ChangeState(AIState.Chase);
                }
                // Else, if the target gets within sense range while resting,
                else if (Vector3.SqrMagnitude(target.position - tf.position) <= aiSenseRadius)
                {
                    // then set state back to Flee.
                    ChangeState(AIState.Flee);
                }
                break;
        }
    }
    #endregion Unity Methods

    #region Dev-Defined Methods
    // Change the state that the tank AI is in.
    public void ChangeState(AIState newState)
    {
        // Change the state of this tank's AI accordingly.
        aiState = newState;

        // Save the time that this change was made.
        timeStateEntered = Time.time;
    }

    // Perform chase protocalls.
    private void DoChase()
    {
        // Rotate a bit towards the target.
        motor.RotateTowards(target.position);

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

    private int DetermineTurnDirection(Transform obstacle, float speed)
    {
        // The distance these rays should cast to. Double the speed of the original ray that found an obstacle.
        float raySpeed = speed * raycastLength_Modifier;

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

    // Checks if the tank should flee from the target or rest.
    private void CheckForFlee()
    {
        // TODO: Finish CheckForFlee()
    }

    // The tank will flee from the target.
    private void DoFlee()
    {
        // If the tank has been fleeing long enough to care about avoiding obstacles,
        if ((timeStateEntered + duration_FleeBeforeAvoiding) <= Time.time)
        {
            // then perform avoidance protocalls while fleeing.
            // Check if we can move forward for 1 second without hitting an obstacle.
            // Pass in "data.moveSpeed_Forward" because that is how far the tank can move in 1 second.
            if (CanMoveForward(data.moveSpeed_Forward))
            {
                // Flee the target.
                Flee();
            }
            // Else, there is an obstacle in the way within 1 second's travel.
            else
            {
                // Enter obstacle avoidance mode.
                avoidanceStage = 1;
            }
        }
        // Else, the tank does not yet care about obstacle avoidance.
        else
        {
            // Flee the target.
            Flee();
        }
    }

    // The tank will flee from the target. Always called by DoFlee(), either after checking for obstacles or not.
    private void Flee()
    {
        // Find the vector away from the target by finding the vector TO the target * -1.
        Vector3 vectorAwayFromTarget = -1 * (target.position - tf.position);

        // Normalize that vector (gives it a magnitude of 1).
        vectorAwayFromTarget.Normalize();

        // Multiply the normalized vector * the designer-chosen fleeDistance.
        vectorAwayFromTarget *= fleeDistance;

        // Add that value to the tank's current position to determine the place we are traveling to.
        Vector3 fleePosition = tf.position + vectorAwayFromTarget;

        // Now rotate towards that position.
        motor.RotateTowards(fleePosition);

        // Move forward.
        motor.Move(data.moveSpeed_Forward);
    }

    // The tank rests, during which time it will heal its health.
    private void DoRest()
    {
        // If the repair type is set to PerSecond,
        if (repairType == RepairType.PerSecond)
        {
            // then raise the tank's health (repair) by restingHealRate per second.
            data.Repair(restingHealRate * Time.deltaTime);
        }
        // Else, the repair type must be set to PerTick.
        else
        {
            // If enough time has passed since the last tick to tick again,
            if ((timeLastTick + tickLength) <= Time.time)
            {
                // then it is time to tick again.
                // Heal/repair the tank by exactly restingHealRate.
                data.Repair(restingHealRate);

                // Set the new time of last tick.
                timeLastTick = Time.time;
            }
        }
    }
    #endregion Dev-Defined Methods
}
