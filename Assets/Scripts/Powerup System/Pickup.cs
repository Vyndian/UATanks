using UnityEngine;

// “Sound effects obtained from https://www.zapsplat.com“

public class Pickup : MonoBehaviour {

    #region Fields
    // Public fields --v


    // Serialized private fields --v

    // The audio clip that will be played when the Powerup is picked up.
    [SerializeField] private AudioClip feedback;

    // The Powerup that is to be picked up.
    [SerializeField] private Powerup powerup;

    // The Transform on this gameObject.
    [SerializeField] private Transform tf;

    // The volume to play the feedback clip at.
    [SerializeField, Range(0.0f, 1.0f)] private float feedbackVolume = 1.0f;


    // Private fields --v
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
                // then play that feedback sound as the gameObject's point.
                AudioSource.PlayClipAtPoint(feedback, tf.position, feedbackVolume);
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
    #endregion Unity Methods


    #region Dev-Defined Methods

    #endregion Dev-Defined Methods
}
