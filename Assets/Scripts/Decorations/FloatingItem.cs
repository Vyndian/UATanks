using UnityEngine;

public class FloatingItem : MonoBehaviour {

    #region Fields
    // Public fields --v


    // Serialized private fields --v

    // The duration that the item moves up or down before switching directions.
    [SerializeField] private float duration = 2.0f;

    // The speed at which the item floats up and down.
    [SerializeField, Tooltip("per second")] private float speed = 0.2f;

    // Whether or not the floating item should start floating downward instead of upward.
    [SerializeField] private bool startFloatingDownwards = false;


    [Header("Component Variables")]
    // The Transform on this gameObject.
    [SerializeField] private Transform tf;


    // Private fields --v

    // The time that the item last switched the direction is was floating in.
    private float timeLastSwitched = 0.0f;

    // The direction the item is currently floating. 1 for up, -1 for down.
    private int direction = 1;
    #endregion Fields


    #region Unity Methods
    // Awake is performed before Start().
    public void Awake()
    {
        // Set variables --v

        // If tf is null,
        if (tf == null)
        {
            // then set it.
            tf = transform;
        }

        // If startFloatingDownwards is ture,
        if (startFloatingDownwards)
        {
            // then flip the direction.
            direction *= -1;
        }
    }

    // Called before the first frame.
    public void Start()
    {

    }

    // Called every frame.
    public void Update()
    {
        tf.position += new Vector3(0, (speed * direction * Time.deltaTime), 0);

        // If enough time has passed,
        if ((timeLastSwitched + duration) < Time.time)
        {
            // then flip the direction.
            direction *= -1;

            // Save the new timeLastSwitched.
            timeLastSwitched = Time.time;
        }
    }
    #endregion Unity Methods


    #region Dev-Defined Methods

    #endregion Dev-Defined Methods
}
