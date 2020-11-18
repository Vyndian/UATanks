﻿using UnityEngine;

// Require a TankData script on the same object as this script. One will be placed automatically with a TankMotor.
[RequireComponent(typeof(TankData))]

public class TankMotor : MonoBehaviour {

    #region Fields
    // Public fields --v

    // Serialized private fields --v

    // References the transform on the cannon of this tank.
    [SerializeField] private Transform cannon_tf;

    // References the prefab used for instantiating shells when the tank shoots.
    [SerializeField] private GameObject prefab_ShellProjectile;

    // References the transform on the projectile spawn point beloning to the cannon's barrel.
    [SerializeField] private Transform projSpawnPnt_tf;

    // Private fields --v

    // References the CharacterMovement component.
    private CharacterController characterController;

    // References the transform on this tank. Used to prevent multiple unnecessary GetComponent calls.
    private Transform tf;

    // References the TankData on this tank.
    private TankData data;
    #endregion Fields

    #region Unity Methods
    // Called before the first frame.
    public void Start()
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
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Moves the tank either forward or backward.
    public void Move(float speed)
    {
        // Create a vector to hold our speed data, equaling the direction of the tank times the speed.
        Vector3 speedVector = tf.forward * speed;

        // Call SimpleMove(), passing in our speedVector. Meters/Second conversion is automatic within SimpleMove().
        characterController.SimpleMove(speedVector);
    }

    // Rotates the tank either left or right.
    public void Turn(float speed)
    {
        // Vector3 for our rotational data. Vector3.up is rotating right by 1 degree. Multiply by speed and time.
        // (transform.Rotate() does not automatically account for time like SimpleMove() does).
        Vector3 rotateVector = Vector3.up * speed * Time.deltaTime;

        // Rotate the tank locally (not in world space) with Space.Self. Pass in the rotateVector.
        tf.Rotate(rotateVector, Space.Self);
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

    // Fires a shell from the cannon in the direction the cannon is facing.
    public void Shoot(float speed)
    {
        // Check if the tank can shoot again yet.
        // If we have reached the time necessary to shoot again,
        if (Time.time >= data.time_ShellReady)
        {
            // then we can fire. First, determine the next time the shell can fire again.
            data.time_ShellReady = Time.time + data.shootDelay;

            // Instantiate a shell.
            GameObject currentShell =  GameObject.Instantiate
                (
                    prefab_ShellProjectile,
                    projSpawnPnt_tf.position,
                    projSpawnPnt_tf.rotation
                );

            // Add force to the shell, firing it away from the cannon at speed.
            currentShell.GetComponent<Rigidbody>().AddForce(transform.forward * speed);
        }
    }
    #endregion Dev-Defined Methods
}