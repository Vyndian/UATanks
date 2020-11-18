using UnityEngine;

public class TankData : MonoBehaviour {

    #region Fields
    // Public fields --v

    // The speed at which this tank will move.
    public float moveSpeed = 3f;

    // The speed at which this tank will rotate.
    public float turnSpeed = 180f;

    // The speed at which the tank's cannon rotates side-to-side.
    public float cannon_turnSpeed = 180f;

    // The speed at which the tank's cannon rotates up-and-down.
    public float cannon_elevateSpeed = 180f;

    // The maximum allowed degree of X rotation the tank's cannon can be elevated UP.
    public float maximumCannonElevateUp = -28f;

    // The maximum allowed degree of X rotation the tank's cannon can be elevated DOWN.
    public float maximumCannonElevateDown = 12f;

    // The aount of delay necessary between firing each shell.
    public float shootDelay = 5f;

    // The time that the tank may fire its next shell. At time of shooting, calculated as Time.time + shootDelay.
    public float time_ShellReady = 0f;

    // The speed at which shell projectiles are fired from the tank cannons.
    public float shellSpeed = 1500f;

    // Serialized private fields --v

    // Private fields --v

    #endregion Fields

    #region Unity Methods
    // Called before the first frame.
    public void Start()
    {
        // Set variables --v


    }

    // Called every frame.
    public void Update()
    {

    }
    #endregion Unity Methods

    #region Dev-Defined Methods

    #endregion Dev-Defined Methods
}
