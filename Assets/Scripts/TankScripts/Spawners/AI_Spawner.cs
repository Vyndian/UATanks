using UnityEngine;

public class AI_Spawner : MonoBehaviour {

    #region Fields
    // Public fields --v

    // The waypoints that should be set to the spawned tank's waypoints variable.
    // The AI_Controller sets its own list from this array.
    public Transform[] waypoints;


    // Serialized private fields --v

    // The EnemyTank prefab that will be used to instantiate the enemy tank.
    // So that the tank spawns with all of its TankData and AI_Controller variables correctly set,
    // the imported prefab must be set up ahead of time and saves separately from the original EnemyTank.
    [SerializeField, Tooltip("Create a Prefab based on EnemyTank prefab with all variables pre-set " +
        "for TankData, AI_Controller, etc.")]
        private GameObject enemyTank_Prefab;


    [Header("Component variables")]
    // The Tranform on this gameObject.
    [SerializeField] private Transform tf;


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
            // then get it off the gameobject.
            tf = transform;
        }
    }

    // Called before the first frame.
    public void Start()
    {
        // Instantiate an EnemyTank on this spawn point with this spawn point as its parent.
        AI_Controller enemyTank =
            Instantiate(enemyTank_Prefab, tf.position, Quaternion.identity, tf).GetComponent<AI_Controller>();
    }

    // Called every frame.
    public void Update()
    {

    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    
    #endregion Dev-Defined Methods
}
