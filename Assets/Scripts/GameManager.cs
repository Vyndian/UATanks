using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    #region Fields
    [Header("Singleton")]
    // The single instance of GameManager allowed.
    public static GameManager instance;


    [Header("Psuedo_Scene GameObjects")]
    // References the Game gameObject where all the game-related objects are.
    public GameObject game;

    // References the StartMenu gameObject.
    public GameObject startMenu;

    //References the OptionsMenu gameObject.
    public GameObject optionsMenu;


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
    // The audio clip that will play when shells fired from this tank explode (where the shell hits).
    public AudioClip feedback_ShellExplosionOnImpact;

    // The audio clip that will play when tanks explode (when they die).
    public AudioClip feedback_TankExplosion;

    // The audio clip that plays the Start menu music.
    public AudioClip musicClip_StartMenu;

    // The audio clip that plays in the background during the game.
    public AudioClip musicClip_GameBGM;

    // References the Main camera's audio source.
    public AudioSource main_AudioSource;


    [Header("Player Preference Keys")]
    // The key used to access the PlayerPreferences for music volume.
    public string key_MusicVolume = "MUSIC_VOLUME";

    // The key used to access the PlayerPreferences for SFX volume.
    public string key_SFXVolume = "SFX_VOLUME";

    // The key used to access the PlayerPreferences for the number of players.
    public string key_NumPlayers = "NUM_PLAYERS";

    // The key used to access the PlayerPreferences for the random seed method.
    public string key_RandomSeedMethod = "RANDOM_SEED_METHOD";
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
            // then set it up.
            main_AudioSource = Camera.main.GetComponent<AudioSource>();
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

        // If there is currently a key for RandomSeed method,
        if (PlayerPrefs.HasKey("RANDOM_SEED_METHOD"))
        {
            // then set apply that preference.
            randomSeedMethod = (MapGenerator.RandomSeedMethod)PlayerPrefs.GetInt("RANDOM_SEED_METHOD");
        }
        // Start playing the StartMenu music.
        ChangeMainAudio(musicClip_StartMenu, volume_Music);
    }

    // Called every frame.
    public void Update()
    {

    }

    // Called when this Monobehavior is being destroyed.
    public void OnDestroy()
    {
        // Save the current settings to player preferences.
        SavePreferences();
    }
    #endregion Unity Methods

    #region Dev-Defined Methods
    // Assign a camera to the tank that called this method.
    // Currently only works with main camera.
    public void AssignCamera(Transform newParent)
    {
        Transform mainCamTf = Camera.main.transform;
        mainCamTf.parent = newParent;
        mainCamTf.position = newParent.position;
        mainCamTf.rotation = newParent.rotation;
    }

    // Spawns the player is a random Player_SpawnPoint.
    public void Player_RandomSpawn()
    {
        // If player_SpawnPoints is not empty or null,
        if (player_SpawnPoints != null && player_SpawnPoints.Count != 0)
        {
            // then determine a random number representing an index in that list.
            int randIndex = UnityEngine.Random.Range(0, player_SpawnPoints.Count);

            // Tell that spawn point to spawn its player.
            player_SpawnPoints[randIndex].SpawnPlayer();
        }
        // Else, it was either empty or null.
        else
        {
            print("empty or null");
        }
    }

    // Respawn the player in a random player_SpawnPoint, or at the one provided (if provided).
    public void Player_Respawn(Transform player, Player_SpawnPoint spawnPoint = null)
    {
        // If spawnPoint is null (no specific spawnPoint was provided),
        if (spawnPoint == null)
        {
            // then determine which spawnPoint to use by determing a random index for the list of player_SpawnPoints.
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
            Player_RandomSpawn();

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

        // Activate the Start menu.
        optionsMenu.SetActive(true);

        // Change the BGM to the Start menu clip (The Options menu is just an extension of the Start menu).
        ChangeMainAudio(musicClip_StartMenu, volume_Music);
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

        // Save the preference for the Random seed method.
        PlayerPrefs.SetInt(key_RandomSeedMethod, (int)randomSeedMethod);
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
    #endregion Dev-Defined Methods
}
