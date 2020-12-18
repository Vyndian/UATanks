using UnityEngine;
using System.Collections.Generic;

// Require a TankData script on the same object as this script. One will be placed automatically with a TankMotor.
[RequireComponent(typeof(TankData))]

// Require a TankCannon script on the same object as this script. One will be placed automatically with a TankMotor.
[RequireComponent(typeof(TankCannon))]

public class TankMotor : MonoBehaviour {

    #region Fields
    [Header("Gears")]
    // References the ai_Tanks on the GM.
    private List<TankData> enemies;

    [Header("Object & Component References")]
    // References the transform on the cannon of this tank.
    [SerializeField] private Transform cannon_tf;

    // References the CharacterMovement component.
    private CharacterController characterController;

    // References the transform on this tank. Used to prevent multiple unnecessary GetComponent calls.
    private Transform tf;

    // References the TankData on this tank.
    private TankData data;
    #endregion Fields


    #region Unity Methods
    // Awake is performed before Start().
    public void Awake()
    {
        // Set variables --v

        // If this var is null,
        if (characterController == null)
        {
            // Get the CharacterController component from this object.
            characterController = gameObject.GetComponent<CharacterController>();
        }

        // If this var is null,
        if (tf == null)
        {
            // Get the transform component from this object.
            tf = transform;
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
        // If the list is null,
        if (enemies == null)
        {
            // then get it from the GM.
            enemies = GameManager.instance.ai_tanks;
        }
    }

    // Called every frame.
    public void Update()
    {
        
    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Moves the tank either forward or backward.
    public void Move(float speed)
    {
        // Create a vector to hold our speed data, equaling the direction of the tank times the speed.
        Vector3 speedVector = tf.forward * speed;

        // Call SimpleMove(), passing in our speedVector. Meters/Second conversion is automatic within SimpleMove().
        characterController.SimpleMove(speedVector);

        // We made noise.
        MakeNoise();
    }

    // Rotates the tank either left or right.
    public void Turn(float speed)
    {
        // Vector3 for our rotational data. Vector3.up is rotating right by 1 degree. Multiply by speed and time.
        // (transform.Rotate() does not automatically account for time like SimpleMove() does).
        Vector3 rotateVector = Vector3.up * speed * Time.deltaTime;

        // Rotate the tank locally (not in world space) with Space.Self. Pass in the rotateVector.
        tf.Rotate(rotateVector, Space.Self);

        // We made noise.
        MakeNoise();
    }

    // Turn the angle of the tank's cannon in relation to its body.
    public void TurnCannon(float speed)
    {
        // Vector3 for the cannon's rotational data. Once again, .up is rotating to the right by one degree,
        // and we need to multiply by speed and time.
        Vector3 rotateVector = Vector3.up * speed * Time.deltaTime;

        // Locally rotate the cannon with this information.
        cannon_tf.Rotate(rotateVector, Space.Self);
    }

    // Change this tank's cannon's elevation (where it is aiming). This equates to a local rotation
    // of the cannon up and down.
    // TODO: Clamp rotation to within bounds. Tried, not working.
    public void ElevateCannon(float speed)
    {
        // Vector3 for the cannon's rotational data. This time, we use .right to rotate up by one degree,
        // and we still need to multiply by speed and time.
        Vector3 rotateVector = Vector3.right * speed * Time.deltaTime;

        // Locally rotate the cannon with this information.
        cannon_tf.Rotate(rotateVector, Space.Self);
    }

    // Used by AI to rotate towards a specific direction.
    // Returns true if the tank can rotate in that direction, and false if it can't (already facing that direction).
    public bool RotateTowards(Vector3 targetPosition)
    {
        // The vector towards the target. Difference between target vector and tank's current vector.
        Vector3 vectorToTarget = targetPosition - tf.position;

        // Determine the Quaternion that would look down the vectorToTarget.
        Quaternion targetRotation = Quaternion.LookRotation(vectorToTarget);

        // If the targetRotation is NOT exactly equal to our current rotation,
        if (targetRotation != tf.rotation)
        {
            // then now we actually rotate a little each frame.
            tf.rotation = Quaternion.RotateTowards(tf.rotation, targetRotation, (data.turnSpeed * Time.deltaTime));

            // We made noise.
            MakeNoise();

            // Return true, we rotated a bit this frame.
            return true;
        }
        // Else, we are already looking the correct direction.
        else
        {
            // Return false.
            return false;
        }
    }

    // The tank makes noise. Check if any enemies can hear.
    private void MakeNoise()
    {
        // If this is not the player,
        if (!data.isPlayer)
        {
            // then return, this only for the player.
            return;
        }

        // Otherwise, the function will run.
        // Iterate through the list of enemies.
        foreach (TankData enemy in enemies)
        {
            // Get the tank's position.
            Vector3 enemyPosition = enemy.transform.position;

            // If this player is within that enemy's hearing radius,
            if (Vector3.SqrMagnitude(tf.position - enemyPosition) <=
                enemy.GetComponent<AI_Controller>().hearingDistance_Squared)
            {
                // then tell the enemy that it hears this player.
                enemy.GetComponent<AI_Controller>().heardPlayer = tf;
            }
        }
    }
    #endregion Dev-Defined Methods
}
