using UnityEngine;

// “Sound effects obtained from https://www.zapsplat.com“

public class Pickup : MonoBehaviour {

    #region Fields
    [Header("Gears")]
    // The Powerup that is to be picked up.
    [SerializeField] private Powerup powerup;


    [Header("Object & Component References")]
    // The Transform on this gameObject.
    [SerializeField] private Transform tf;

    // Reference to the GM.
    private GameManager gm;


    [Header("Prefab References")]
    // The audio clip that will be played when the Powerup is picked up.
    [SerializeField] private AudioClip feedback;
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

        // Get a reference to the gm.
        gm = GameManager.instance;
    }

    // Called before the first frame.
    public void Start()
    {
        // Add the powerup on this pickup to the GM's list of spawnedPowerups.
        gm.spawnedPowerups.Add(powerup);
    }

    // Called every frame.
    public void Update()
    {

    }

    // Called when a Rigidbody or another collider enters the trigger collider on this gameObject.
    public void OnTriggerEnter(Collider other)
    {
        // Attempt to get the PowerupController from the object that triggered this collider.
        PowerupController powCon = other.gameObject.GetComponent<PowerupController>();

        // If that other other has a PowerupController,
        if (powCon != null)
        {
            // then add this powerup to that powCon.
            powCon.AddPowerup(powerup);

            // If feedback has been set up,
            if (feedback != null)
            {
                // then play that feedback sound at the GM's audioPoint.
                AudioSource.PlayClipAtPoint(feedback, gm.audioPoint, gm.volume_SFX);
            }
            // Else, the feedback was not set up.
            else
            {
                // Log the error.
                Debug.LogError("ERROR: Feedback clip not assigned for the pickup: " + gameObject.name);
            }

            // Destroy this pickup gameObject.
            Destroy(gameObject);
        }
    }

    // Called when this Monobehavior is being destroyed.
    public void OnDestroy()
    {
        // Remove the powerup attached to this pickup from the GM's list.
        gm.spawnedPowerups.Remove(powerup);
    }
    #endregion Unity Methods


    #region Dev-Defined Methods

    #endregion Dev-Defined Methods
}
