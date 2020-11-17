using UnityEngine;

public class TankMotor : MonoBehaviour {

    #region Fields
    // Public fields --v

    // Serialized private fields --v

    // Private fields --v

    // References the CharacterMovement component.
    private CharacterController characterController;
    // References the transform on this gameObject. Used to prevent multiple unnecessary GetComponent calls.
    private Transform tf;
    // References the TankData on this tank.
    private TankData data;
    #endregion Fields

    #region Unity Methods
    // Called before the first frame.
    public void Start()
    {
        // Set variables --v

        // Get the CharacterController component from this object.
        characterController = gameObject.GetComponent<CharacterController>();
        // Get the transform component from this object.
        tf = transform;
        // Get the TankData on this tank.
        data = GetComponent<TankData>();
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
        // Vector3 for our rotational data. Vector3.up is rotating right by 1 degree. Multoply by speed and time.
        // (transform.Rotate() does not automatically account for time like SimpleMove() does).
        Vector3 rotateVector = Vector3.up * speed * Time.deltaTime;

        // Rotate the tank locally (not in world space) with Space.Self. Pass in the rotateVector.
        tf.Rotate(rotateVector, Space.Self);
    }
    #endregion Dev-Defined Methods
}
