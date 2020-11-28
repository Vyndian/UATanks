using UnityEngine;
using System.Collections.Generic;

// Require a TankMotor script on the same object as this script.
[RequireComponent(typeof(TankMotor))]

// For the Tracker AI.
// TODO: Combine all AI sample controllers into one script (once finished with Finite STate Machines lecture).
public class SampleAIController2 : MonoBehaviour {

    #region Fields
    // Public fields --v


    // Serialized private fields --v

    [Header("Tracker: Pursuit & Flee Variables")]
    // The actual enum var for the AttackMode this tank is in.
    [SerializeField] private AttackMode attackMode = AttackMode.Chase;

    // The Transform of the target this tank will Chase.
    [SerializeField] private Transform tracker_Target;

    // The distance at which the TrackerAI is close enough to the player.
    [SerializeField] private float chasing_CloseEnough = 7.0f;

    // The distance the Tracker should flee towards when running away from the target (player).
    [SerializeField] private float fleeDistance = 1.0f;

    [Header("Component References")]
    // The transform on this gameObject.
    [SerializeField] private Transform tf;

    // The TankMotor on this gameObject.
    [SerializeField] private TankMotor motor;

    // The TankData on this gameObject.
    [SerializeField] private TankData data;


    // Private fields --v

    // A STATIC int representing the index of the next player to be targeted.
    // The index is for the GM's list of alive players.
    static private int playerToTarget = 0;

    // The enum blueprint for which AttackMode the tank is in.
    private enum AttackMode {  Chase, Flee };

    // The square of chasing_CloseEnough. Determined by maths at Awake.
    private float chasing_CloseEnough_Squared;
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

        // Square the tracker's closeEnough var and set it.
        chasing_CloseEnough_Squared = chasing_CloseEnough * chasing_CloseEnough;
    }

    // Called before the first frame.
    public void Start()
    {
        
    }

    // Called every frame.
    public void Update()
    {
        // This cannot be done at Start or earlier because the players need time to set up.
        // If this var is null,
        if (tracker_Target == null)
        {
            // Get the transform component from the next player tank in the GM's list.
            tracker_Target = GameManager.instance.player_tanks[playerToTarget].gameObject.transform;

            // Advance to the next player so that all the tanks don't focus on one player.
            NextPlayerToTarget();
        }

        // If set to Chase mode,
        if (attackMode == AttackMode.Chase)
        {
            // then rotate towards the target.
            motor.RotateTowards(tracker_Target.position);

            // If not already close enough to the target,
            if (Vector3.SqrMagnitude(tracker_Target.position - tf.position) > chasing_CloseEnough_Squared)
            {
                // then move forward.
                motor.Move(data.moveSpeed_Forward);
            }
        }

        // If set to Flee mode,
        if (attackMode == AttackMode.Flee)
        {
            // Find the vector away from the target by finding the vector TO the target * -1.
            Vector3 vectorAwayFromTarget = -1 * (tracker_Target.position - tf.position);

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
    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Advances the int representing the index of the next alive player to target, according to the GM's list.
    private void NextPlayerToTarget()
    {
        // Get a temp reference to the list for readability and processing speeds.
        List<TankData> playerList = GameManager.instance.player_tanks;

        // If we're not already at the end of the player list,
        if (playerToTarget < playerList.Count - 1)
        {
            // then add one to the index var.
            playerToTarget++;
        }
        // Else, we are at the end of the player list.
        else
        {
            // Set the var to 0.
            playerToTarget = 0;
        }
    }
    #endregion Dev-Defined Methods
}
