using UnityEngine;

public class PowerupSpawner : MonoBehaviour {

    #region Fields
    // Definition for an enum describing the choices for how Pickups might be chosen when spawning them.
    private enum SpawnMethod { Random, Sequential, PingPing, AlwaysFirst };


    [Header("Levers")]
    // The delay between when one pickup is picked up and another pickup is spawned.
    [SerializeField] private float spawnDelay = 7.0f;


    [Header("Gears")]
    // Enum for the three choices of how pickups may be chosen.
    [SerializeField, Tooltip("The next Pickup chosen can be chosen from the array completely randomly," +
        "or go in order (0-1-2-0-1-2), or PingPong (0-1-2-1-0), or always choose the first in the array.")]
        private SpawnMethod spawnMethod = SpawnMethod.Random;

    // The next time that a Pickup is allowed to spawn.
    private float nextSpawnTime;

    // The spawned Pickup. Null when there is no Pickup on the spawn point.
    private GameObject spawnedPickup;

    // The next index to be used for the Sequential method of spawning Pickups.
    private int nextIndex = 0;

    // The number to add to nextIndex for DetermineNextIndex. Flips occasionaly if using PingPong method.
    private int addTo_NextIndex = 1;


    [Header("Object & Component References")]
    // The Transform of this gameObject.
    [SerializeField] private Transform tf;


    [Header("Prefab References")]
    // An array of all possible Pickup prefabs that this spawner can spawn.
    // Putting the same prefab into the array multiple times increases its chances of being randomly chosen.
    [SerializeField] private GameObject[] pickupPrefabs;
    #endregion Fields


    #region Unity Methods
    // Awake is performed before Start().
    public void Awake()
    {
        // Set variables --v

        // If tf is null,
        if (tf == null)
        {
            // then set it up.
            tf = transform;
        }
    }

    // Called before the first frame.
    public void Start()
    {
        // Spawn a pickup.
        SpawnPickup();
    }

    // Called every frame.
    public void Update()
    {
        // If there is currently no Pickup spawned,
        if (spawnedPickup == null)
        {
            // and if it is time to spawn another Pickup,
            if (Time.time >= nextSpawnTime)
            {
                // then spawn the Pickup.
                SpawnPickup();
            }
            // Else, it is not time to spawn a new Pickup yet. Do nothing.
        }
        // Else, the Pickup object still exists. Postpone the nextSpawnTime.
        else
        {
            // Delay the timer by the emount of time taken between frames.
            nextSpawnTime += Time.deltaTime;
        }
    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Spawns a new Pickup. How the next Pickup in the array is chosen depends on the spawnMethod.
    // This also sets the new nextSpawnTime.
    private void SpawnPickup()
    {
        // If there is already a Pickup spawned,
        if (spawnedPickup != null)
        {
            // then do nothing and return. Do not instantiate a second Pickup.
            return;
        }

        // An int to hold the appropriate index depending on firstPickup.
        int index = 0;

        // Act according to the chosen spawnMethod.
        switch (spawnMethod)
        {
            // In the case that the spawnMethod is Random,
            case SpawnMethod.Random:
                // then generate a random number that will represent a random index of the pickupPrefabs array.
                index = Random.Range(0, (pickupPrefabs.Length));
                break;


            // In the case that the spawnMethod is Sequential or PingPong,
            case SpawnMethod.Sequential:
            case SpawnMethod.PingPing:
                // then verify that the nextIndex of the array is valid. If so,
                if (pickupPrefabs[nextIndex] != null)
                {
                    // then use that index.
                    index = nextIndex;
                }
                // Else, the nextIndex is invalid.
                else
                {
                    // Find a new nextIndex before saving.
                    DetermineNextIndex();
                    index = nextIndex;
                }

                // Find the nextIndex, now that the current one has been used.
                DetermineNextIndex();
                break;


            // In the case that the spawnMethod is AlwaysFirst,
            case SpawnMethod.AlwaysFirst:
                // then nothing stricly needs to be done, as index defaults to 0.
                // Set it equal to 0 anyway, just to be sure.
                index = 0;
                break;
        }

        // Spawn a Pickup from the array using the appropriate index.
        spawnedPickup = Instantiate(pickupPrefabs[index], tf.position, Quaternion.identity);

        // Set its parent.
        spawnedPickup.transform.parent = tf;

        // Set the next time a Pickup is allowed to spawn.
        nextSpawnTime = Time.time + spawnDelay;
    }

    // Determines the next index for the Sequential method.
    private void DetermineNextIndex()
    {
        // Increment the nextIndex (or decrement, if using PingPong and current working backwards).
        nextIndex += addTo_NextIndex;

        // If using PingPong,
        if (spawnMethod == SpawnMethod.PingPing)
        {
            // and if the new nextIndex is one too high,
            if (nextIndex == pickupPrefabs.Length)
            {
                // then flip addTo_NextIndex.
                addTo_NextIndex = -1;

                // Set decrease nextIndex by 2 (one to go back to the last index, another to start a downward path).
                nextIndex -= 2;
            }
            // Else, if the new nextIndex is one too low,
            else if (nextIndex == -1)
            {
                // then flip addToNextIndex.
                addTo_NextIndex = 1;

                // Set nextIndex to 1 in order to start it back on the upward path.
                nextIndex = 1;
            }
        }

        // If the nextIndex is now too high,
        if (nextIndex >= pickupPrefabs.Length)
        {
            // then set nextIndex back to the start (0).
            nextIndex = 0;
        }
    }

    // Used to draw editor-only graphics to help see gameObjects with no meshes.
    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(tf.position, 0.4f);
    }
    #endregion Dev-Defined Methods
}
