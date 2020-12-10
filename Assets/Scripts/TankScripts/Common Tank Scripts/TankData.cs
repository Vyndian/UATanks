using UnityEngine;

public class TankData : MonoBehaviour {

    #region Fields
    // Reference for the GM.
    private GameManager gm;

    // Whether or not this is a player tank or AI. Initialized as false as default.
    [HideInInspector] public bool isPlayer = false;


    [Header("Score")]
    // This player's current score.
    public int currentScore = 0;

    // The amount of point awarded to the player that kills this tank.
    public int pointsValue = 100;

    // The minimum amount of points this tank would be work if killed.
    // pointsValue cannot be lowered below this number.
    [SerializeField] private readonly int minPointsValue = 20;

    // The amount of score that the player loses every time they die.
    [SerializeField] private readonly int pointsLostPerDeath = 30;

    // The ScoreData that will serve as an element on the High Scores list.
    private ScoreData scoreData;


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
    public float fireRate = 2.3f;

    // The time that the tank may fire its next shell. At time of shooting, calculated as Time.time + shootDelay.
    public float time_ShellReady = 0f;

    // The speed at which shell projectiles are fired from the tank cannons.
    public float shellSpeed = 1500f;

    // The forward speed this tank had at the start of the game.
    [HideInInspector] public float originalSpeed_Forward = 3.0f;


    [Header("Health & Damage")]
    // The maxHealth of the tank.
    public float maxHealth = 100f;

    // The currentHealth of the tank.
    public float currentHealth;

    // The amount of lives that this tank starts the game with (not including the first life).
    [SerializeField] private int maxLives = 2;

    // The amount of lives that this tank has left (not including the life they are currently using).
    private int remainingLives;

    // The percentage of this tank's health that, if it falls below the threshold, the tank will flee.
    [Range(1, 100)]
    public int fleeThreshold_Percentage = 50;

    // The amount of damage dealt by shells fired from this tank.
    [Tooltip("The amount of damage dealt by shells fired from this tank.")]
        public float shellDamage = 10f;


    [Header("Object References")]
    // The CameraPosition object, childed to the cannon, where this player's camera should go.
    [SerializeField] private GameObject cameraPosition;


    [Header("Component variables")]
    // The Transform on this gameObject.
    [SerializeField] private Transform tf;
    #endregion Fields

    #region Unity Methods
    // Performed before Start.
    public void Awake()
    {
        // Set variables --v

        // Set currentHealth to maxHealth.
        currentHealth = maxHealth;

        // Set remainingLives to maxLives.
        remainingLives = maxLives;

        // Set the original forward speed to the speed when the game starts.
        originalSpeed_Forward = moveSpeed_Forward;
        
        // If tf is null,
        if (tf == null)
        {
            // then set it.
            tf = transform;
        }
    }

    // Called before the first frame.
    public void Start()
    {
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
        // If this in an enemy tank,
        if (!isPlayer)
        {
            // Add to the score of the player that killed this tank.
            killedBy.ChangeScore(pointsValue);

            // Play audio clip of the tank exploding.
            AudioSource.PlayClipAtPoint(gm.feedback_TankExplosion, tf.position, gm.volume_SFX);

            // Destroy this tank.
            Destroy(gameObject);
        }
        // Else, this is a player tank.
        else
        {
            // Lower this player's score.
            ChangeScore(-pointsLostPerDeath);

            // If the player has lives/respawns remaining,
            if (remainingLives > 0)
            {
                // then consume a life/respawn.
                remainingLives--;

                // Fully heal the player.
                Repair(maxHealth);

                // Respawn the player in a random spawn point.
                gm.Player_Respawn(tf);
            }
            // Else, the player just lost their last life. Game Over for them.
            else
            {
                // If this player is the last one alive,
                if (gm.player_tanks.Count == 1)
                {
                    // then transition to the GameOver screen.
                    // TODO: GameOver.
                }
                // Else, there is at least one other player.
                else
                {
                    // Remove this player from the game.
                    Destroy(this);
                }
            }
        }
    }

    // Change the score for this player by the amount provided.
    public void ChangeScore(int change)
    {
        // Apply the change.
        currentScore += change;

        // If the new score is less than 0,
        if (currentScore < 0)
        {
            // then set it to 0.
            currentScore = 0;
        }
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


