using UnityEngine;

public class InputController : MonoBehaviour {

    #region Fields
    // Public fields --v

    // The Enum definition for input types.
    public enum InputScheme { WASD, arrowKeys };

    // An instance of InputScheme for this InputController component.
    public InputScheme input = InputScheme.WASD;

    // Serialized private fields --v

    // The TankMotor on this gameObject.
    [SerializeField] private TankMotor motor;

    // The TankData on this gameObject.
    [SerializeField] private TankData data;

    // Private fields --v

    #endregion Fields

    #region Unity Methods
    // Called before the first frame.
    public void Start()
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
    public void Update()
    {
        // Depending on input type, get player's current input and call the appropriate function.
        switch (input)
        {
            // In the case that the input is set to WASD,
            case InputScheme.WASD:
                // If player is pressing Spacebar,
                if (Input.GetKey(KeyCode.Space))
                {
                    // Attempt to fire the cannon.
                    motor.Shoot(data.shellSpeed);
                }

                // If player is pressing W,
                if (Input.GetKey(KeyCode.W))
                {
                    // Move the tank forward.
                    motor.Move(data.moveSpeed);
                }
                // Else, if the player is pressing S,
                else if (Input.GetKey(KeyCode.S))
                {
                    // Move the tank backward.
                    motor.Move(-data.moveSpeed);
                }

                // If player is pressing A,
                if (Input.GetKey(KeyCode.A))
                {
                    // Turn the tank left.
                    motor.Turn(-data.turnSpeed);
                }
                // Else, if player is pressing D,
                else if (Input.GetKey(KeyCode.D))
                {
                    // Turn the tank right.
                    motor.Turn(data.turnSpeed);
                }
                break;

            // In the case that the input is set to arrowKeys,
            case InputScheme.arrowKeys:
                // If player is pressing right control,
                if (Input.GetKey(KeyCode.RightControl))
                {
                    // Attempt to fire the cannon.
                    motor.Shoot(data.shellSpeed);
                }

                // If player is pressing upArrow,
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    // Move the tank forward.
                    motor.Move(data.moveSpeed);
                }
                // Else, if the player is pressing downArrow,
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    // Move the tank backward.
                    motor.Move(-data.moveSpeed);
                }

                // If player is pressing leftArrow,
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    // Turn the tank left.
                    motor.Turn(-data.turnSpeed);
                }
                // Else, if player is pressing rightArrow,
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    // Turn the tank right.
                    motor.Turn(data.turnSpeed);
                }
                break;
        }
    }
    #endregion Unity Methods

    #region Dev-Defined Methods

    #endregion Dev-Defined Methods
}
