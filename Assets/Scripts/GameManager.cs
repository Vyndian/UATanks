using UnityEngine;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour {

    #region Fields
    // Public fields --v

    // The single instance of GameManager allowed.
    public static GameManager instance;

    // A two-dimensional array holding all the Room scripts, each associated with a room tile in the grid.
    // The "x, y" coordinates of each element represent their position in the rows, columns of the room grid.
    public Room[,] grid;

    // A list of the currently ALIVE players' TankData Components.
    public List<TankData> player_tanks;

    // A list of the currently ALIVE AIs' TankData Components.
    public List<TankData> ai_tanks;

    // A list of all Player_SpawnPoints.
    public List<Player_SpawnPoint> player_SpawnPoints;

    // The number of rooms expected to be created.
    public int numRooms_Expected;

    // The number of rooms that have been created so far.
    public int numRooms_Created;

    // Serialized private fields --v

    // Private fields --v

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
    }

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

            // Spawn AIs
        }
    }
    #endregion Dev-Defined Methods
}
