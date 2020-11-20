using UnityEngine;

public class TankData : MonoBehaviour {

    #region Fields
    // Public fields --v

    [Header("Score")]
    // This player's current score.
    public int currentScore = 0;

    // The amount of point awarded to the player that kills this tank.
    public int pointsForKilling = 100;

    [Header("Time/Speeds")]
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

    [Header("Health")]
    // The maxHealth of the tank.
    public float maxHealth = 100f;

    // The currentHealth of the tank.
    public float currentHealth;

    // Serialized private fields --v

    [Header("Object References")]
    // The CameraPosition object, childed to the cannon, where this player's camera should go.
    [SerializeField] private GameObject cameraPosition;

    // Private fields --v

    // Reference for the GM.
    private GameManager gm;

    // Whether or not this is a player tank or AI. Initialized as false as default.
    private bool isPlayer = false;

    #endregion Fields

    #region Unity Methods
    // Called before the first frame.
    public void Start()
    {
        // Set variables --v

        // Set currentHealth to maxHealth.
        currentHealth = maxHealth;

        // Set the GameManager reference.
        gm = GameManager.instance;

        // If this is a player tank and not an AI,
        if (GetComponent<InputController>())
        {
            // then set isPlayer to true.
            isPlayer = true;

            // Put this TankData on the GM's list of all players.
            gm.allPlayers.Add(this);

            // Put this TankData on the GM's list of alive players.
            gm.alivePlayers.Add(this);

            // Set camera.
            AssignCamera();
        }
        // Else, this is an AI. Leave isPlayer as false.
        else
        {
            // Put this TankData on the GM's list of all AIs.
            gm.allAIs.Add(this);

            // Put this TankData on the GM's list of alive AIs.
            gm.aliveAIs.Add(this);
        }
    }

    // Called every frame.
    public void Update()
    {

    }

    // Called when this script is being destroyed.
    void OnDestroy()
    {
        // If this is a player,
        if (isPlayer)
        {
            // Iterate through the GM's list of alive players.
            foreach (TankData data in gm.alivePlayers)
            {
                // If the current iteration matches this player's TankData,
                if (data == this)
                {
                    // then remove it from the list.
                    gm.alivePlayers.Remove(data);

                    // We are done, so return. Otherwise, will get an unumeration error.
                    return;
                }
            }
        }
        // Else, this is an AI.
        else
        {
            // Iterate through the GM's list of alive AIs.
            foreach (TankData data in gm.aliveAIs)
            {
                // If the current iteration matches this AI's TankData,
                if (data == this)
                {
                    // then remove it from the list.
                    gm.aliveAIs.Remove(data);

                    // We are done, so return. Otherwise, will get an unumeration error.
                    return;
                }
            }
        }
    }
    #endregion Unity Methods

    #region Dev-Defined Methods
    // Change the tank's health by the amount given. -change for damage, +change for healing/repair.
    public void ChangeHealth(float change, TankData dealtBy)
    {
        // Apply the change.
        currentHealth += change;

        // If tank is dead,
        if (currentHealth <= 0)
        {
            // then kill the tank.
            Death(dealtBy);
        }
        // Else, if the new health is too high,
        else if (currentHealth > maxHealth)
        {
            // then set it back down to maximum.
            currentHealth = maxHealth;
        }
    }

    // Kill the tank.
    public void Death(TankData killedBy)
    {
        // Add to the score of the player that killed this tank.
        killedBy.ChangeScore(pointsForKilling);

        // Destroy this tank.
        Destroy(gameObject);
    }

    // Change the score for this player by the amount provided.
    public void ChangeScore(int change)
    {
        // Apply the change.
        currentScore += change;
    }

    // Assign a camera to this player.
    private void AssignCamera()
    {
        // then call AssignCamera on the GM.
        GameManager.instance.AssignCamera(cameraPosition.transform);
    }
    #endregion Dev-Defined Methods
}


