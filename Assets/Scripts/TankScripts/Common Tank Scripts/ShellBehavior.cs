using UnityEngine;

public class ShellBehavior : MonoBehaviour {

    #region Fields
    // Public fields --v

    // The amount of damage dealt by this shell. Set immediately after instantiation by the tank that fired it.
    // Defaulted to 10 just in case.
    public float damage = 10f;

    // The TankData of the tank that fired this shell.
    public TankData firedBy;

    // The amount of time before a shell destroys itself (even if it hits nothing at all).
    public float maxLifetime = 1.5f;

    // Serialized private fields --v

    // Private fields --v

    // The time at which the shell will self-destruct if it does not hit anything. Determined at Start().
    private float timeToSelfDestruct;

    // References the GM.
    private GameManager gm;

    // Reference to this gameObject's Tranform.
    private Transform tf;

    #endregion Fields

    #region Unity Methods
    // Called before Start() runs.
    public void Awake()
    {
        // Get and save a reference to this gameObject's Transform.
        tf = transform;
    }

    // Called before the first frame.
    void Start()
    {
        // Set variables --v

        // Determine the time at which the shell will self-destruct if no other objects are hit.
        timeToSelfDestruct = Time.time + maxLifetime;

        // Get reference to the GM.
        gm = GameManager.instance;
    }

    // Called every frame.
    void Update()
    {
        // If the shell has run out of lifetime,
        if (Time.time >= timeToSelfDestruct)
        {
            // Play the explosion audio clip.
            PlayExplosionClip();

            // then destroy this shell.
            Destroy(gameObject);
        }
    }
    #endregion Unity Methods


    #region Callback Methods
    // Called when the rigidbody on this shell comes in contact with another rigidbody or collider.
    public void OnCollisionEnter(Collision collision)
    {
        // Attempt to get the TankData from the hit object (will be null if not a tank).
        TankData data = collision.gameObject.GetComponent<TankData>();

        // If the result is NOT null,
        if (data != null)
        {
            // then deal damage to the tank that was hit.
            data.TakeDamage(damage, firedBy);
        }

        // Play audio of the shell exploding.
        PlayExplosionClip();

        // Destroy the shell.
        Destroy(gameObject);
    }
    #endregion Callback Methods


    #region Dev-Defined Methods
    // Plays an explosion audio clip.
    private void PlayExplosionClip()
    {
        // Create a clip of an explosion at the point where the shell was when it expired/hit something.
        AudioSource.PlayClipAtPoint(gm.feedback_ShellExplosionOnImpact, tf.position, gm.volume_SFX);
    }
    #endregion Dev-Defined Methods
}
