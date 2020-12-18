using UnityEngine;

public class TankCannon : MonoBehaviour {

    #region Fields
    [Header("Levers")]
    // The AudioClip file to be played when the cannon fires a shell.
    [SerializeField] private AudioClip feedback_ShellFiring;


    [Header("Object & Component References")]
    // References the TankData on this tank object.
    [SerializeField] private TankData data;

    // References the prefab used for instantiating shells when the tank shoots.
    [SerializeField] private GameObject prefab_ShellProjectile;

    // References the transform on the projectile spawn point beloning to the cannon's barrel.
    [SerializeField] private Transform projSpawnPnt_tf;

    // Reference to the GM.
    private GameManager gm;
    #endregion Fields


    #region Unity Methods
    // Awake is performed before Start().
    public void Awake()
    {
        // Set variables --v

        // If data is not set,
        if (data == null)
        {
            data = GetComponent<TankData>();
        }
    }

    // Called before the first frame.
    public void Start()
    {
        // Set up the gm reference.
        gm = GameManager.instance;
    }

    // Called every frame.
    public void Update()
    {

    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Fires a shell from the cannon in the direction the cannon is facing.
    // If a damage multiplier is provided (by the assassin), damage will be adjusted accordingly.
    public void Fire(float speed, float damageMultiplier = 1.0f)
    {
        // Check if the tank can shoot again yet.
        // If we have reached the time necessary to shoot again,
        if (Time.time >= data.time_ShellReady)
        {
            // then we can fire. First, determine the next time the shell can fire again.
            data.time_ShellReady = Time.time + data.fireRate;

            // Instantiate a shell.
            GameObject shell = GameObject.Instantiate
                (
                    prefab_ShellProjectile,
                    projSpawnPnt_tf.position,
                    projSpawnPnt_tf.rotation
                );

            // Get the shell's behavior script.
            ShellBehavior shellBehavior = shell.GetComponent<ShellBehavior>();

            // Set the shell's damage.
            shellBehavior.damage = data.shellDamage * damageMultiplier;

            // Set the shell's firedBy to this tank's TankData.
            shellBehavior.firedBy = data;

            // Add force to the shell, firing it away from the cannon at speed.
            shell.GetComponent<Rigidbody>().AddForce(transform.forward * speed);

            // Play sound clip of the cannon firing.
            AudioSource.PlayClipAtPoint(feedback_ShellFiring, gm.audioPoint, gm.volume_SFX);
        }
    }
    #endregion Dev-Defined Methods
}
