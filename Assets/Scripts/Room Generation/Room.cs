using UnityEngine;

public class Room : MonoBehaviour {

    #region Fields
    [Header("Object & Component References")]
    // References to the Animators on the doors on this room.
    public Animator doorNorth;
    public Animator doorSouth;
    public Animator doorEast;
    public Animator doorWest;
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
