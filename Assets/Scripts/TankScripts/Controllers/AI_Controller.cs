using UnityEngine;

public class AI_Controller : MonoBehaviour {

    #region Fields
    // Public fields --v

    // Serialized private fields --v

    // The TankMotor on this gameObject.
    [SerializeField] private TankMotor motor;

    // The TankData on this gameObject.
    [SerializeField] private TankData data;

    // The TankCannon on this gameObject.
    [SerializeField] private TankCannon cannon;

    // Private fields --v

    #endregion Fields

    #region Unity Methods
    // Performed before Start.
    public void Awake()
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

        // If this var is null,
        if (cannon == null)
        {
            // Get the TankCannon on this gameObject.
            cannon = gameObject.GetComponent<TankCannon>();
        }
    }

    // Called before the first frame.
    public void Start()
    {
        
    }

    // Called every frame.
    public void Update()
    {
        // For Project 1, AI tanks need only shoot at a delay (set within TankData). No movement or aiming.
        // Shooting will fail until the delay has been satisfied.
        cannon.Fire(data.shellSpeed);
    }
    #endregion Unity Methods

    #region Dev-Defined Methods

    #endregion Dev-Defined Methods
}
