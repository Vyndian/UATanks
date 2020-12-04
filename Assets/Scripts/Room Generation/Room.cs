using UnityEngine;
using System.Collections;

public class Room : MonoBehaviour {

    #region Fields
    // Public fields --v

    // References to the doors on this room.
    public GameObject doorNorth;
    public GameObject doorSouth;
    public GameObject doorEast;
    public GameObject doorWest;


    // Serialized private fields --v


    // Private fields --v
    #endregion Fields


    #region Unity Methods
    // Awake is performed before Start().
    public void Awake()
    {
        // Set variables --v


    }

    // Called before the first frame.
    public void Start()
    {
        // Tell the GM that this room has been created.
        GameManager.instance.RoomCreated();
    }

    // Called every frame.
    public void Update()
    {

    }
    #endregion Unity Methods


    #region Dev-Defined Methods

    #endregion Dev-Defined Methods
}
