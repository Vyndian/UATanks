using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TankData : MonoBehaviour {

    #region Fields
    // Reference for the GM.
    private GameManager gm;

    // Whether or not this is a player tank or AI. Initialized as false as default.
    [HideInInspector] public bool isPlayer = false;

    // The player number of this player (the 1 or 2 in Player, Player2).
    [HideInInspector] public int playerNumber;


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


    [Header("HUD")]
    // Reference to the gameObject holding the HUD canvas.
    [SerializeField] private GameObject HUD;

    // The distance that the HUD elements must be raised for single player.
    [SerializeField] private float HUDoffset_Vertical = 25.0f;

    // References the HealthBar section of the HUD.
    [SerializeField] private GameObject healthBar_GameObject;

    // References the Score section of the HUD.
    [SerializeField] private GameObject score_GameObject;

    // References the LivesRemaining section of the HUD.
    [SerializeField] private GameObject livesRemaining_GameObject;

    // Reference to the Slider for the Health Bar.
    [SerializeField] private Slider healthBar_Slider;

    // Reference to the Text for the player's score.
    [SerializeField] private Text score_Text;

    // Reference to the Text for the player's number of lives remaining.
    [SerializeField] private Text livesRemaining_Text;


    [Header("Misc")]
    // The Camera attached to this player tank.
    public Camera tankCamera;

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

        // If tankCamera is null,
        if (tankCamera == null)
        {
            // then find it in the tank's children.
            tankCamera = GetComponentInChildren<Camera>();
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

            // Activate the HUD.
            HUD.SetActive(true);

            // Update the HUD.
            HUD_Update();

            // If single player,
            if (gm.numPlayers == 1)
            {
                // then raise the HUD elements to fit better for single player.
                RaiseHUDElements();
            }
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
        
    }
    #endregion Unity Methods

    #region Dev-Defined Methods
    // The tank takes damage equal to the prescribed amount.
    public void TakeDamage(float damage, TankData dealtBy)
    {
        // If this damage would kill the tank,
        if ((currentHealth - damage) <= 0)
        {
            // then kill the tank.
            Death(dealtBy);
        }
        // Else, this damage would not kill the tank.
        else
        {
            // Apply the damage.
            currentHealth -= damage;

            // Update the health bar on the HUD.
            HUD_Update_HealthBar();
        }
    }

    // The tank repairs/heals by the prescribed amount.
    public void Repair(float healing)
    {
        print("Repair() called.");
        // Apply the healing/repair.
        currentHealth += healing;
        print("Healing applied.");

        // Ensure the health is not higher than the maximum.
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        print("Healing clamped.");

        // Update the health bar on the HUD.
        HUD_Update_HealthBar();
    }

    // Kill the tank.
    public void Death(TankData killedBy)
    {
        print("Death() called.");
        // Play audio clip of the tank exploding.
        AudioSource.PlayClipAtPoint(gm.feedback_TankExplosion, gm.audioPoint, gm.volume_SFX);

        // Add to the score of the player that killed this tank.
        killedBy.ChangeScore(pointsValue);

        // If this in an enemy tank,
        if (!isPlayer)
        {
            // Remove this tank from the GM's list of AI tanks.
            gm.ai_tanks.Remove(this);

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

                // Update the HUD's "Lives Remaining".
                HUD_Update_LivesRemaining();

                // Respawn the player in a random spawn point.
                gm.Player_Respawn(tf);
            }
            // Else, the player just lost their last life. Game Over for them.
            else
            {
                // Tell the GM this player's final score.
                // If this player was Player1,
                if (playerNumber == 1)
                {
                    // then set the current score to the GM's score for Player1.
                    gm.score_Player1 = currentScore;
                }
                // Else, this must be Player2.
                else
                {
                    // Set the current score to the GM's score for Player2.
                    gm.score_Player2 = currentScore;
                }

                // If this player is the last one alive,
                if (gm.player_tanks.Count == 1)
                {
                    // then remove this tank from the GM's list of players.
                    gm.player_tanks.Remove(this);

                    // Transition to the GameOver screen.
                    gm.ShowGameOver();
                }
                // Else, there is at least one other player.
                else
                {
                    // Remove this tank from the GM's list of players.
                    gm.player_tanks.Remove(this);

                    // Remove this player from the game.
                    Destroy(gameObject);
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

        // Update the HUD's Score.
        HUD_Update_Score();
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

    // Updates all values on the HUD.
    private void HUD_Update()
    {
        // Update the HUD's lives remaining.
        HUD_Update_LivesRemaining();

        // Update the HUD's score.
        HUD_Update_Score();

        // Update the HUD's health bar.
        HUD_Update_HealthBar();
    }

    // Update the HUD's "Lives Left" section.
    private void HUD_Update_LivesRemaining()
    {
        // Update the value visible to players on the HUD to match the correct number of lives remaining.
        livesRemaining_Text.text = remainingLives.ToString();
    }

    // Update the HUD's "Score" section.
    private void HUD_Update_Score()
    {
        // If this is a player,
        if (isPlayer)
        {
            // Update the value visible to players on the HUD to match the correct score.
            score_Text.text = currentScore.ToString();
        }
    }

    // Update the HUD's "Health Bar" section.
    private void HUD_Update_HealthBar()
    {
        // If this is a player,
        if (isPlayer)
        {
            // Update the health bar to match the player's health.
            healthBar_Slider.value = currentHealth / maxHealth;
        }
    }

    // Raises the HUD elements by a variable degree.
    // Necessary for single player, since the HUD elements are so low to work with multiplayer screens.
    private void RaiseHUDElements()
    {
        // Create the offset.
        Vector3 offset = new Vector3(0, HUDoffset_Vertical, 0);

        // Move the health bar.
        healthBar_GameObject.transform.position += offset;

        // Move the score.
        score_GameObject.transform.position += offset;

        // Move the LivesRemaining.
        livesRemaining_GameObject.transform.position += offset;
    }
    #endregion Dev-Defined Methods
}


