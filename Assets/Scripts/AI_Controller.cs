using UnityEngine;

public class AI_Controller : MonoBehaviour {

    #region Fields
    // Public fields --v

    // Serialized private fields --v

    // The TankMotor on this gameObject.
    [SerializeField] private TankMotor motor;

    // The TankData on this gameObject.
    [SerializeField] private TankData data;

    // Private fields --v

    #endregion Fields

    #region Unity Methods
    // Called before the first frame.
    void Start()
    {
        // Set variables --v

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
    }

    // Called every frame.
    void Update()
    {
        // For Project 1, AI tanks need only shoot at a delay (set within TankData). No movement or aiming.
        // Shooting will fail until the delay has been satisfied.
        motor.Shoot(data.shellSpeed);
    }
    #endregion Unity Methods

    #region Dev-Defined Methods

    #endregion Dev-Defined Methods
}
