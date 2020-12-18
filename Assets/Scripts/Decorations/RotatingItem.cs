using UnityEngine;

public class RotatingItem : MonoBehaviour {

    #region Fields
    [Header("Levers")]
    // The Vector3 to rotate this item by each second.
    [SerializeField] private Vector3 spin = new Vector3(50, 35, 50);


    [Header("Object & Component References")]
    // The Transform of this gameObject.
    [SerializeField] private Transform tf;
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
        // Rotate the item.
        tf.Rotate(spin * Time.deltaTime);
    }
    #endregion Unity Methods


    #region Dev-Defined Methods

    #endregion Dev-Defined Methods
}
