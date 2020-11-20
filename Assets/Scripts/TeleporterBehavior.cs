// Unfinished.

using UnityEngine;

public class TeleporterBehavior : MonoBehaviour {

    #region Fields
    // Public fields --v

    // Serialized private fields --v

    // A reference for the transform of the other teleporter in the set. Should be set in prefab already.
    [SerializeField] private Transform teleportLocation;

    // Private fields --v

    #endregion Fields

    #region Unity Methods
    // Called before the first frame.
    void Start()
    {
        // Set variables --v

        
    }

    // Called every frame.
    void Update()
    {
        
    }
    #endregion Unity Methods


    #region Callback Methods
    // Called when the rigidbody on this gameObject comes in contact with another rigidbody or collider.
    public void OnCollisionEnter(Collision collision)
    {
        // Attempt to get the TankData from the collider (will be null if not a tank).
        TankData tank = collision.gameObject.GetComponent<TankData>();

        // If the collision was with a tank (tank is not null),
        if (tank != null)
        {
            // then teleport the tank to the other pad.
            tank.gameObject.transform.position = teleportLocation.position;
        }
    }
    #endregion Callback Methods


    #region Dev-Defined Methods
    
    #endregion Dev-Defined Methods
}
