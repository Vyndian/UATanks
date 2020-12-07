using UnityEngine;

public class Player_SpawnPoint : MonoBehaviour {

    #region Fields
    // Public fields --v


    // Serialized private fields --v

    // The playerTank prefab that this spawn point will use to instantiate the player if this
    // spawn point is chosen to be the player's original spawn point.
    [SerializeField] private GameObject playerPrefab;


    [Header("Component variables")]
    // The Tranform on this gameObject.
    [SerializeField] private Transform tf;


    // Private fields --v

    // A STATIC variable for whether the player has already been spawned once.
    private static bool playerSpawned = false;

    // A reference to the GM.
    private GameManager gm;
    #endregion Fields


    #region Unity Methods
    // Awake is performed before Start().
    public void Awake()
    {
        // Set variables --v

        // If tf is null,
        if (tf == null)
        {
            // then get it off the gameobject.
            tf = transform;
        }
    }

    // Called before the first frame.
    public void Start()
    {
        // Save a reference to the GM.
        gm = GameManager.instance;

        // Add this spawn point to the GM's list.
        gm.player_SpawnPoints.Add(this);
    }

    // Called every frame.
    public void Update()
    {

    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Spawns the player on this spawn point. This should only be called once via the GM.
    public void SpawnPlayer()
    {
        // If the player has not yet been spawned,
        if (!playerSpawned)
        {
            // then set playerSpawned to true.
            playerSpawned = true;

            // Spawn the player with this spawner as the parent.
            GameObject player = Instantiate(playerPrefab, tf.position, Quaternion.identity, tf);

            // Have the player face the same way as the spawner.
            player.transform.rotation = tf.rotation;
        }
    }
    #endregion Dev-Defined Methods
}
