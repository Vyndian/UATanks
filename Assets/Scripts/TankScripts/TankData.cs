using UnityEngine;

public class TankData : MonoBehaviour {

    #region Fields
    // Public fields --v

    // Whether or not this is a player tank or AI. Initialized as false as default.
    [HideInInspector] public bool isPlayer = false;

    [Header("Score")]
    // This player's current score.
    public int currentScore = 0;

    // The amount of point awarded to the player that kills this tank.
    public int pointsValue = 100;

    // The minimum amount of points this tank would be work if killed. Cannot be lowered below this number.
    [SerializeField] private readonly int minPointsValue = 20;

    [Header("Time/Speeds")]
    // The speed at which this tank will move forward.
    [Tooltip("Must be positive")] public float moveSpeed_Forward = 3f;

    // The speed at which this tank will move in reverse.
    [Tooltip("Must be negative")] public float moveSpeed_Reverse = -1.0f;

    // The speed at which this tank will rotate.
    public float turnSpeed = 150f;

    // The speed at which the tank's cannon rotates side-to-side.
    public float cannon_turnSpeed = 180f;

    // The speed at which the tank's cannon rotates up-and-down.
    public float cannon_elevateSpeed = 180f;

    // The maximum allowed degree of X rotation the tank's cannon can be elevated UP.
    public float maximumCannonElevateUp = -28f;

    // The maximum allowed degree of X rotation the tank's cannon can be elevated DOWN.
    public float maximumCannonElevateDown = 12f;

    // The aount of delay necessary between firing each shell.
    public float shootDelay = 2.3f;

    // The time that the tank may fire its next shell. At time of shooting, calculated as Time.time + shootDelay.
    public float time_ShellReady = 0f;

    // The speed at which shell projectiles are fired from the tank cannons.
    public float shellSpeed = 1500f;

    [Header("Health & Damage")]
    // The maxHealth of the tank.
    public float maxHealth = 100f;

    // The currentHealth of the tank.
    public float currentHealth;

    // The percentage of this tank's health that, if it falls below the threshold, the tank will flee.
    [Range(1, 100)]
    public int fleeThreshold_Percentage = 50;

    // The amount of damage dealt by shells fired from this tank.
    [Tooltip("The amount of damage dealt by shells fired from this tank.")] public float shellDamage = 10f;

    // The forward speed this tank had at the start of the game.
    [HideInInspector] public float originalSpeed_Forward = 3.0f;

    // Serialized private fields --v

    [Header("Object References")]
    // The CameraPosition object, childed to the cannon, where this player's camera should go.
    [SerializeField] private GameObject cameraPosition;

    // Private fields --v

    // Reference for the GM.
    private GameManager gm;
    #endregion Fields

    #region Unity Methods
    // Performed before Start.
    public void Awake()
    {
        // Set the original forward speed to the speed when the game starts.
        originalSpeed_Forward = moveSpeed_Forward;
    }

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

            // Put this TankData on the GM's list of players.
            gm.player_tanks.Add(this);

            // Set camera.
            AssignCamera();
        }
        // Else, this is an AI. Leave isPlayer as false.
        else
        {
            // Put this TankData on the GM's list of AIs.
            gm.ai_tanks.Add(this);
        }
    }

    // Called every frame.
    public void Update()
    {

    }

    // Called when this script is being destroyed.
    public void OnDestroy()
    {
        // If this is a player,
        if (isPlayer)
        {
            // Iterate through the GM's list of alive players.
            foreach (TankData data in gm.player_tanks)
            {
                // If the current iteration matches this player's TankData,
                if (data == this)
                {
                    // then remove it from the list.
                    gm.player_tanks.Remove(data);

                    // We are done, so return. Otherwise, will get an unumeration error.
                    return;
                }
            }
        }
        // Else, this is an AI.
        else
        {
            // Iterate through the GM's list of alive AIs.
            foreach (TankData data in gm.ai_tanks)
            {
                // If the current iteration matches this AI's TankData,
                if (data == this)
                {
                    // then remove it from the list.
                    gm.ai_tanks.Remove(data);

                    // We are done, so return. Otherwise, will get an unumeration error.
                    return;
                }
            }
        }
    }
    #endregion Unity Methods

    #region Dev-Defined Methods
    // The tank takes damage equal to the prescribed amount.
    public void TakeDamage(float damage, TankData dealtBy)
    {
        // Apply the damage.
        currentHealth -= damage;

        // If tank is dead,
        if (currentHealth <= 0)
        {
            // then kill the tank.
            Death(dealtBy);
        }
    }

    // The tank repairs/heals by the prescribed amount.
    public void Repair(float healing)
    {
        // Apply the healing/repair.
        currentHealth += healing;

        // Ensure the health is not higher than the maximum.
        currentHealth = Mathf.Min(currentHealth, maxHealth);
    }

    // Kill the tank.
    public void Death(TankData killedBy)
    {
        // Add to the score of the player that killed this tank.
        killedBy.ChangeScore(pointsValue);

        // Destroy this tank.
        Destroy(gameObject);
    }

    // Change the score for this player by the amount provided.
    public void ChangeScore(int change)
    {
        // Apply the change.
        currentScore += change;
    }

    // Change the amount of points a tank would get from killing this tank. +change adds, -change subtracts.
    public void ChangePointsValue(int change)
    {
        // Apply the change.
        pointsValue += change;

        // If the new amount is less than the minimum,
        if (pointsValue < minPointsValue)
        {
            // then set pointsValue equal to the minimum instead.
            pointsValue = minPointsValue;
        }
    }

    // Assign a camera to this player.
    private void AssignCamera()
    {
        // then call AssignCamera on the GM.
        GameManager.instance.AssignCamera(cameraPosition.transform);
    }
    #endregion Dev-Defined Methods
}


