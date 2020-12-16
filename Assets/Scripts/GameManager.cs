using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    #region Fields
    [Header("Singleton")]
    // The single instance of GameManager allowed.
    public static GameManager instance;


    [Header("Psuedo-Scene GameObjects")]
    // References the Game gameObject where all the game-related objects are.
    public GameObject game;

    // References the StartMenu gameObject.
    public GameObject startMenu;

    //References the OptionsMenu gameObject.
    public GameObject optionsMenu;

    // References the GameOver gameObject.
    public GameObject gameOver;

    // References the PauseMenu gameObject.
    public GameObject pauseMenu;


    [Header("Procedural Generation")]
    // A two-dimensional array holding all the Room scripts, each associated with a room tile in the grid.
    // The "x, y" coordinates of each element represent their position in the rows, columns of the room grid.
    public Room[,] grid;

    // The number of rooms expected to be created.
    [HideInInspector] public int numRooms_Expected;

    // The number of rooms that have been created so far.
    [HideInInspector] public int numRooms_Created;


    [Header("Options")]
    // The player's chosen volume level for music.
    public float volume_Music = 80.0f;

    // The player's chosen volume level for sound effects.
    public float volume_SFX = 80.0f;

    // Holds the currently selected number of players.
    public int numPlayers = 1;

    // The current value for which method should be used to determine the seed for Random.
    public MapGenerator.RandomSeedMethod randomSeedMethod = MapGenerator.RandomSeedMethod.DateTime;

    // The current value for the random seed that was manually entered by the player.
    public int manualSeed_Value;


    [Header("Tank Lists")]
    // A list of the currently ALIVE players' TankData Components.
    public List<TankData> player_tanks;

    // A list of the currently ALIVE AIs' TankData Components.
    public List<TankData> ai_tanks;


    [Header("Tank Spawning")]
    // The prefabs from which to randomly choose which kind of enemy tank to spawn next.
    // NOTE: For Caravan/Guard duo, the prefab must include both! Find the prefab with both.
    [SerializeField] private GameObject[] aiTank_Prefabs;

    // The number of AI tanks that the game will attempt to spawn (as long as there are enough spawn points).
    [SerializeField] private int numEnemiesToSpawn = 4;

    // A list of all Player_SpawnPoints.
    public List<Player_SpawnPoint> player_SpawnPoints;

    // A list of all AI_SpawnPoints.
    public List<AI_SpawnPoint> ai_SpawnPoints;


    [Header("Powerups")]
    // A list of all powerups current in the level waiting to be picked up.
    public List<Powerup> spawnedPowerups;


    [Header("Audio")]
    // The point at which audio clips should be played (will equal the main AudioSource/Listener's location).
    public Vector3 audioPoint;

    // The audio clip that will play when shells fired from this tank explode (where the shell hits).
    public AudioClip feedback_ShellExplosionOnImpact;

    // The audio clip that will play when tanks explode (when they die).
    public AudioClip feedback_TankExplosion;

    // The audio clip that plays the Start menu music.
    public AudioClip musicClip_StartMenu;

    // The audio clip that plays in the background during the game.
    public AudioClip musicClip_GameBGM;

    // The audio clip that plays in the background during the GameOver screen.
    public AudioClip musicClip_GameOver;

    // The audio clip that plays when a player's pointer is pressed down on a button.
    public AudioClip feedback_PointerDown;

    // The audio clip that plays when a player's pointer is released from a button.
    public AudioClip feedback_PointerUp;

    // References the main audio source, attached to the AudioSource prefab centered in the game.
    public AudioSource main_AudioSource;




    [Header("Player Preference Keys")]
    // The key used to access the PlayerPreferences for music volume.
    public string key_MusicVolume = "MUSIC_VOLUME";

    // The key used to access the PlayerPreferences for SFX volume.
    public string key_SFXVolume = "SFX_VOLUME";

    // The key used to access the PlayerPreferences for the number of players.
    public string key_NumPlayers = "NUM_PLAYERS";


    [Header("Score")]
    // These scores are set by the appropriate player's TankData once they lose their last life.
    // The score achieved by Player1
    public int score_Player1;

    // The score achieved by Player2.
    public int score_Player2;


    [Header("Cameras")]
    // The X and Y values of the camera for Player1.
    [SerializeField] private Vector2 viewportRectPosition_Player1 = new Vector2(0, (float)0.5);

    // The X and Y values of the camera for Player2.
    [SerializeField] private Vector2 viewportRectPosition_Player2 = new Vector2(0, 0);

    // The H and W that needs to be applied to the Camera on both players.
    [SerializeField] private Vector2 viewportRectSize = new Vector2(1, (float)0.5);
    #endregion Fields

    #region Unity Methods
    // Runs when created, before Start().
    public void Awake()
    {
        // If no other instance of GameManager currently exists,
        if (instance == null)
        {
            // then save the instance as this instance of GameManager.
            instance = this;
        }
        // Else, there is already another GM. Log an error and destroy this gameObject.
        else
        {
            // Log the error.
            Debug.LogError("ERROR: There can be only 1 GameManager.");
            // Destroy this gameObject.
            Destroy(gameObject);
        }

        // Initialize these lists.
        player_tanks = new List<TankData>();
        ai_tanks = new List<TankData>();
        player_SpawnPoints = new List<Player_SpawnPoint>();
        ai_SpawnPoints = new List<AI_SpawnPoint>();
        spawnedPowerups = new List<Powerup>();
    }

    // Called before the first frame.
    public void Start()
    {
        // Set variables --v

        // If this var is null,
        if (main_AudioSource == null)
        {
            // then log an error.
            Debug.LogError("ERROR: Main audio source not found.");
        }
        // Else, the main audio source is set up correctly.
        else
        {
            ChangeMainAudio(musicClip_StartMenu, volume_Music);
        }

        // Apply the player's preferences.
        // If there is currently a key for music volume,
        if (PlayerPrefs.HasKey(key_MusicVolume))
        {
            // then set apply that preference.
            volume_Music = PlayerPrefs.GetFloat(key_MusicVolume);
        }

        // If there is currently a key for SFX volume,
        if (PlayerPrefs.HasKey(key_SFXVolume))
        {
            // then set apply that preference.
            volume_SFX = PlayerPrefs.GetFloat(key_SFXVolume);
        }

        // If there is currently a key for number of players,
        if (PlayerPrefs.HasKey(key_NumPlayers))
        {
            // then set apply that preference.
            numPlayers = PlayerPrefs.GetInt(key_NumPlayers);
        }
    }

    // Called every frame.
    public void Update()
    {
        // If the player just pressed down the escape key,
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // then toggle pause.
            TogglePause();
        }
    }

    // Called when this Monobehavior is being destroyed.
    public void OnDestroy()
    {
        // Save the current settings to player preferences.
        SavePreferences();
    }
    #endregion Unity Methods

    #region Dev-Defined Methods
    // Spawns the player is a random Player_SpawnPoint.
    public void Players_RandomSpawn()
    {
        // If player_SpawnPoints is not empty or null,
        if (player_SpawnPoints != null && player_SpawnPoints.Count != 0)
        {
            // then shuffle/randomize that list.
            ShuffleList<Player_SpawnPoint>(player_SpawnPoints);

            // Tell the first spawn point in the shuffled list to spawn its player.
            // Save a reference to its InputController.
            InputController player = player_SpawnPoints[0].SpawnPlayer();

            // Set that player as Player1.
            player.GetComponent<TankData>().playerNumber = 1;

            // If numPlayers is 2,
            if (numPlayers == 2)
            {
                // then get a reference to that player's Camera.
                Camera cam = player.GetComponentInChildren<Camera>();
                
                // Set this camera's Viewport Rect appropriately for Player1.
                cam.rect = new Rect(viewportRectPosition_Player1, viewportRectSize);

                // Tell the next spawn point to spawn their player.
                InputController player2 = player_SpawnPoints[1].SpawnPlayer();

                // Set the new player as Player2.
                player2.GetComponent<TankData>().playerNumber = 2;

                // Set the new player to use arrow keys for input.
                player2.input = InputController.InputScheme.arrowKeys;

                // Get the new player's Camera.
                Camera cam2 = player2.GetComponentInChildren<Camera>();

                // Set that tank's camera's Viewport Rect appropriately for Player2.
                cam2.rect = new Rect(viewportRectPosition_Player2, viewportRectSize);
            }
        }
        // Else, it was either empty or null.
        else
        {
            // Do nothing.
        }
    }

    // Respawn the player in a random player_SpawnPoint, or at the one provided (if provided).
    public void Player_Respawn(Transform player, Player_SpawnPoint spawnPoint = null)
    {
        print("Player_Respawn() called.");
        // If spawnPoint is null (no specific spawnPoint was provided),
        if (spawnPoint == null)
        {
            // then determine which spawnPoint to use by determining
            // a random index for the list of player_SpawnPoints.
            int randIndex = UnityEngine.Random.Range(0, player_SpawnPoints.Count);

            // Set spawnPoint accordingly.
            spawnPoint = player_SpawnPoints[randIndex];
        }

        // Get and save the spawnPoint's Transform.
        Transform spawnPoint_tf = spawnPoint.transform;

        // Move the player to the spawn point's location.
        player.position = spawnPoint_tf.position;

        // Assign the spawnPoint as the tank's new parent.
        player.parent = spawnPoint_tf;
    }

    // Called when a room finishes being created.
    public void RoomCreated()
    {
        // Increment the number of rooms created.
        numRooms_Created++;

        // If the number created has reached the number expected,
        if (numRooms_Created >= numRooms_Expected)
        {
            // then spawn the player.
            Players_RandomSpawn();

            // Spawn the AI tanks.
            AITank_RandomSpawns();
        }
    }

    // Randomize the elements in the ai_SpawnPoints list.
    // This is known as the Fisher-Yates shuffle.
    // Retrieved from https://answers.unity.com/questions/773285/pick-a-memeber-form-the-list-only-once.html
    private void ShuffleList<T>(List<T> list)
    {
        // Iterate through the list.
        for (int i = 0; i < list.Count; i++)
        {
            // Get a random int representing an index between the current iteration and the end of the list.
            int j = UnityEngine.Random.Range(i, list.Count);

            // Create a temp obj holding the value of the current element in the iteration through the list.
            T t = list[i];

            // Set the current element of the list equal to the element at the randomly determined index.
            list[i] = list[j];

            // Set the element at the randomly determined index to what used to be in the current iteration.
            list[j] = t;
        }
    }

    // Use the same method to shuffle an array.
    private void ShuffleArray<T>(T[] array)
    {
        // Iterate through the array.
        for (int i = 0; i < array.Length; i++)
        {
            // Get a random int representing an index between the current iteration and the end of the array.
            int j = UnityEngine.Random.Range(i, array.Length);

            // Create a temp obj holding the value of the current element in the iteration through the array.
            T t = array[i];

            // Set the current element of the array equal to the element at the randomly determined index.
            array[i] = array[j];

            // Set the element at the randomly determined index to what used to be in the current iteration.
            array[j] = t;
        }
    }

    // Spawn the AI tanks in random ai spawn points.
    private void AITank_RandomSpawns()
    {
        // Spawn AIs. First, randomize the list of AI spawn points.
        ShuffleList(ai_SpawnPoints);

        // Then randomize the array of AI tank prefabs.
        ShuffleArray(aiTank_Prefabs);

        // Track how many tanks have been spawned.
        int numTanksSpawned = 0;

        // Iterate through the ai Spawn Points list.
        foreach (AI_SpawnPoint spawnPoint in ai_SpawnPoints)
        {
            // If we still need to spawn more tanks, and if the next AI Tank Prefab is not null,
            if (numTanksSpawned < numEnemiesToSpawn && aiTank_Prefabs[numTanksSpawned] != null)
            {
                // then tell the spawn point to instantiate the next AI tank prefab.
                spawnPoint.SpawnAITank(aiTank_Prefabs[numTanksSpawned]);

                // Increment the number of tanks spawned so far.
                numTanksSpawned++;
            }
        }
    }

    // Activates the Game gameObject and disables the rest.
    public void ShowGame()
    {
        // Disable all gameObjects not necessary for the game.
        startMenu.SetActive(false);
        optionsMenu.SetActive(false);
        gameOver.SetActive(false);

        // Activate the Game gameObject.
        game.SetActive(true);

        // Change the BGM to the game BGM clip.
        // Also adjust the volume. This track is very loud, and players will not want to be blasted.
        ChangeMainAudio(musicClip_GameBGM, (float)(volume_Music * 0.75));
    }

    // Activates the StartMenu gameObject and disables the rest.
    public void ShowStartMenu()
    {
        // Disable all gameObjects not necessary for the Start menu.
        game.SetActive(false);
        optionsMenu.SetActive(false);
        gameOver.SetActive(false);

        // Activate the Start menu.
        startMenu.SetActive(true);

        // Change the BGM to the Start menu clip.
        ChangeMainAudio(musicClip_StartMenu, volume_Music);
    }

    // Activates the OptionsMenu gameObject and disables the rest.
    public void ShowOptionsMenu()
    {
        // Disable all gameObjects not necessary for the Options menu.
        game.SetActive(false);
        startMenu.SetActive(false);
        gameOver.SetActive(false);

        // Activate the Start menu.
        optionsMenu.SetActive(true);

        // Change the BGM to the Start menu clip (The Options menu is just an extension of the Start menu).
        ChangeMainAudio(musicClip_StartMenu, volume_Music);
    }

    // Activates the GameOver gameObject and disables the rest.
    public void ShowGameOver()
    {
        // Disable all gameObjects not necessary for the Options menu.
        game.SetActive(false);
        startMenu.SetActive(false);
        optionsMenu.SetActive(false);

        // Activate the Start menu.
        gameOver.SetActive(true);

        // Change the BGM to the GameOver music clip.
        ChangeMainAudio(musicClip_GameOver, volume_Music);
    }

    // Saves the current settings into player preferences.
    public void SavePreferences()
    {
        // Save the preference for the music volume.
        PlayerPrefs.SetFloat(key_MusicVolume, volume_Music);

        // Save the preference for the SFX volume.
        PlayerPrefs.SetFloat(key_SFXVolume, volume_SFX);

        // Save the preference for the number of players.
        PlayerPrefs.SetInt(key_NumPlayers, numPlayers);
    }

    // Changes the main camera's audio source's clip and volume.
    public void ChangeMainAudio(AudioClip clip, float volume)
    {
        // Change the volume first.
        main_AudioSource.volume = volume;

        // Change the clip.
        main_AudioSource.clip = clip;

        // If the audio source is not playing at the moment,
        if (!main_AudioSource.isPlaying)
        {
            // then play the clip.
            main_AudioSource.Play();
        }
    }

    // Play the sound for when a player's pointer is pressed over a button.
    public void PointerDownFeedback()
    {
        // Play the feedback for pointer down.
        AudioSource.PlayClipAtPoint(feedback_PointerDown, audioPoint, (float)(volume_SFX * 0.6));
    }

    // Play the sound for when a player's pointer is released from a button.
    public void PointerUpFeedback()
    {
        // Play the feedback for pointer up.
        AudioSource.PlayClipAtPoint(feedback_PointerUp, audioPoint, (float)(volume_SFX * 0.75));
    }

    // Toggles whether or not the game is paused.
    public void TogglePause()
    {
        // If the actual game is currently running,
        if (game.activeSelf)
        {
            // and if the game is not already paused,
            if (Time.timeScale == 1)
            {
                // then pause the game.
                PauseGame();
            }
            // Else, the game is active AND already paused.
            else
            {
                // Resume the game.
                ResumeGame();
            }
        }
        // Else, the game is not running.
        else
        {
            // Do nothing.
        }
    }

    // Pauses the game.
    private void PauseGame()
    {
        // Activate the pause menu.
        pauseMenu.SetActive(true);

        // Pauses the game by setting the time scale to 0.
        Time.timeScale = 0;
    }

    // Resumes the game.
    private void ResumeGame()
    {
        // If the pause menu is active,
        if (pauseMenu.activeSelf)
        {
            // then deactivate it.
            pauseMenu.SetActive(false);
        }

        // Resume the game by setting the time scale back to 1.
        Time.timeScale = 1;
    }

    // Restarts the game by reloading the Main scene.
    public void RestartGame()
    {
        // Ensure the time scale is at 1.
        Time.timeScale = 1;

        // Restart the game by reloading the Main scene.
        SceneManager.LoadScene("Main");
    }

    // Quits the game.
    public void QuitGame()
    {
        // Ends the application.
        Application.Quit();
    }
    #endregion Dev-Defined Methods
}
