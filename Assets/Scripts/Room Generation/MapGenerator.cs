using UnityEngine;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour {

    #region Fields
    // Public fields --v


    // Serialized private fields --v

    // All of the Room prefabs that the game can use to generate the game map.
    [SerializeField] private GameObject[] gridPrefabs;

    // The number of rows of rooms that should be generated.
    [SerializeField] private int numRows;

    // The number of columns of rooms that should be generated.
    [SerializeField] private int numColumns;

    // The seed that will be used for RNGs if useSeed is true.
    [SerializeField] private int mapSeed_Manual = 123;

    // Enum for which method should be used for seeding Random.
    [SerializeField] private RandomSeedMethod randomSeedMethod = RandomSeedMethod.DateTime;


    [Header("Component variables")]
    // The Transform on this gameObject.
    [SerializeField] private Transform tf;


    // Private fields --v

    // Enum definition for the different methods available for seeding Random for map generation.
    private enum RandomSeedMethod { DateTime, Manual, MapOfTheDay };

    // A reference to the GameManager.
    private GameManager gm;

    // The hieght of the room prefabs.
    private float roomHeight = 50.0f;

    // The width of the room prefabs.
    private float roomWidth = 50.0f;

    // A randomized list of all the elements in the array of roomPrefabs.
    // Randomizing the array into a list, and then stepping through that new in order, is a way of
    // choosing a random element of the array every time, but never getting the same one twice.
    // This list is cleared and re-randomized every time the list has been stepped through completely.
    private List<GameObject> randomizedGridPrefabs = new List<GameObject>();

    // The current index of the randomized grid prefab that is to be used.
    private int currentIndex = 0;

    // The map seed to use for the "MapOfTheDay".
    private int mapSeed_MapOfTheDay;
    #endregion Fields


    #region Unity Methods
    // Awake is performed before Start().
    public void Awake()
    {
        // Set variables --v

        // Get a reference to the GameManager.
        gm = GameManager.instance;

        // If tf is null,
        if (tf == null)
        {
            // then set it.
            tf = transform;
        }

        // Initialize the mapSeed_MapOfTheDay.
        mapSeed_MapOfTheDay = DateToInt(DateTime.Now.Date);
    }

    // Called before the first frame.
    public void Start()
    {
        // If gm is still not set up correctly,
        if (gm == null)
        {
            // then set it up.
            gm = GameManager.instance;
        }

        // Tell the GM how many rooms in total are expected to be created.
        gm.numRooms_Expected = numColumns * numRows;

        // If the randomSeedMethod is set to DateTime,
        if (randomSeedMethod == RandomSeedMethod.DateTime)
        {
            // then set the random seed using the current DateTime.
            UnityEngine.Random.InitState(DateToInt(DateTime.Now));
        }
        // Else, if the randomSeedMethod is set to Manual,
        else if (randomSeedMethod == RandomSeedMethod.Manual)
        {
            // then set the random seed using the manually-entered mapSeed.
            UnityEngine.Random.InitState(mapSeed_Manual);
        }
        // Else, the randomSeedMethod is set to MapOfTheDay.
        else if (randomSeedMethod == RandomSeedMethod.MapOfTheDay)
        {
            // Set the random seed using the manually-entered mapSeed.
            UnityEngine.Random.InitState(mapSeed_MapOfTheDay);
        }

        // Initialize the list of randomized grid prefabs.
        RandomizePrefabList();

        // Generate the grid of rooms.
        GenerateGrid();
    }

    // Called every frame.
    public void Update()
    {
        // As soon as the GM's list of enemy tanks is not empty,
        // (this serves as a marker that the world is mostly built and objects can be safely referenced).
        if (gm.ai_tanks != null && gm.ai_tanks.Count != 0)
        {
            // then tell the gm to spawn the player in a random player spawn point.
            gm.Player_RandomSpawn();
        }
    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Randomize the elements in the gridPrefabs array and store them in the randomizedGridPrefabs list.
    // This is known as the Fisher-Yates shuffle.
    // Retrieved from https://answers.unity.com/questions/773285/pick-a-memeber-form-the-list-only-once.html
    private void RandomizePrefabList()
    {
        // Clear the list.
        randomizedGridPrefabs.Clear();

        // For each element in the original array,
        foreach (GameObject obj in gridPrefabs)
        {
            // add that element into the list.
            // They will still be in the original, non-random order.
            randomizedGridPrefabs.Add(obj);
        }

        // Iterate through the list.
        for (int i = 0; i < randomizedGridPrefabs.Count; i++)
        {
            // Get a random int representing an index between the current iteration and the end of the list.
            int j = UnityEngine.Random.Range(i, randomizedGridPrefabs.Count);

            // Create a temp GameObject holding the value of the current element in the iteration through the list.
            GameObject t = randomizedGridPrefabs[i];

            // Set the current element of the list equal to the element at the randomly determined index.
            randomizedGridPrefabs[i] = randomizedGridPrefabs[j];

            // Set the element at the randomly determined index to what used to be in the current iteration.
            randomizedGridPrefabs[j] = t;
        }
    }

    // Increments the currentIndex while assuring that it points to a valid GameObject in the randomized list.
    private void NextIndex()
    {
        // Increment the current index.
        currentIndex++;

        // If the new index is now too high,
        if (currentIndex >= randomizedGridPrefabs.Count)
        {
            // then set the current index to 0.
            currentIndex = 0;

            // Re-randomize the list.
            RandomizePrefabList();
        }
    }

    // Returns a random room tile using the shuffled GameObject tiles put into randomizedGridPrefabs.
    private GameObject RandomRoom()
    {
        // If the current index of the randomized list doesn't get us a valid GameObject,
        if (randomizedGridPrefabs[currentIndex] == null)
        {
            // then generate a new index to work with.
            NextIndex();
        }

        // Save the output with the currentIndex.
        GameObject output = randomizedGridPrefabs[currentIndex];

        // Increment the currentIndex.
        NextIndex();

        // return the output.
        return output;
    }

    // Generates the grid of rooms.
    private void GenerateGrid()
    {
        // Clear out the grid array by making a new array and saving it to the grid.
        gm.grid = new Room[numColumns, numRows];

        // For each row of rooms we are supposed to create,
        for (int i = 0; i < numRows; i++)
        {
            // For each column (run for every row),
            for (int j = 0; j < numColumns; j++)
            {
                // Determine the room's x position.
                float xPosition = roomWidth * j;

                // Determine the room's y position.
                float zPosition = roomHeight * i;

                // Put the positions together in a Vector3.
                Vector3 position = new Vector3(xPosition, 0, zPosition);

                // Instantiate the room at that position.
                InstantiateRoom(position, j, i);
            }
        }

        // Re-seed Random to use the current DateTime.
        UnityEngine.Random.InitState(DateToInt(DateTime.Now));
    }

    // Instantiate a room at the given coordinates.
    private void InstantiateRoom(Vector3 position, int column, int row)
    {
        // Create the new room in the provided coordinates of the grid.
        GameObject tempRoomObj =
            Instantiate(RandomRoom(), position, Quaternion.identity) as GameObject;

        // Set the room's parent.
        tempRoomObj.transform.parent = tf;

        // Give the room a meaningful name.
        tempRoomObj.name = "Room_" + column + "-" + row;

        // Get the Room script off of this room tile.
        Room room = tempRoomObj.GetComponent<Room>();

        // Add the Room script on this room to the grid array in the appropriate spot.
        gm.grid[column, row] = room;

        // Open any doors that need to be opened.
        OpenDoors(room, column, row);
    }

    // Open any doors on this room that need to be opened.
    private void OpenDoors(Room room, int column, int row)
    {
        // If the room is in the first (bottom) row,
        if (row == 0)
        {
            // then open the north door.
            room.doorNorth.SetActive(false);
        }
        // Else, if the room is in the last (top) row,
        else if (row == (numRows - 1))
        {
            // then open the south door.
            room.doorSouth.SetActive(false);
        }
        // Else, the room is in a middle row.
        else
        {
            // Open both the north and south doors.
            room.doorNorth.SetActive(false);
            room.doorSouth.SetActive(false);
        }

        // If the room is in the first (left-most) column,
        if (column == 0)
        {
            // then open the east door.
            room.doorEast.SetActive(false);
        }
        // Else, if the room is in the last (right-most) column,
        else if (column == (numColumns - 1))
        {
            // then open the west door.
            room.doorWest.SetActive(false);
        }
        // Else, the room is in a middle column.
        else
        {
            // Open both the east and west doors.
            room.doorEast.SetActive(false);
            room.doorWest.SetActive(false);
        }
    }

    // Return an integer based on the information within the provided DateTime.
    private int DateToInt(DateTime date)
    {
        // Return the sum of all of these values.
        return date.Year + date.Month + date.Day + date.Hour + date.Minute + date.Second + date.Millisecond;
    }
    #endregion Dev-Defined Methods
}
