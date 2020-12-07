using UnityEngine;

public class AI_SpawnPoint : MonoBehaviour {

    #region Fields
    // Public fields --v

    // The waypoints that should be set to the spawned tank's waypoints variable.
    // The AI_Controller sets its own list from this array.
    public Transform[] waypoints;


    // Serialized private fields --v


    [Header("Component variables")]
    // The Tranform on this gameObject.
    [SerializeField] private Transform tf;


    // Private fields --v

    // References the GM.
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

        // Set the gm variable.
        gm = GameManager.instance;

        // Add this spawn point to the GM's list of ai spawn point.
        gm.ai_SpawnPoints.Add(this);
    }

    // Called before the first frame.
    public void Start()
    {

    }

    // Called every frame.
    public void Update()
    {

    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Spawns the provided AI tank.
    public void SpawnAITank(GameObject tank)
    {
        // Instantiate the tank.
        Transform newTank = Instantiate(tank, tf.position, Quaternion.identity, tf).transform;

        // Ensure the tank is facing the right way (Quaternion.identity should do this, but isn't for some reason).
        newTank.rotation = tf.rotation;

        // Put this spawn point as its parent.
        newTank.parent = tf;
    }
    #endregion Dev-Defined Methods
}
