using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    #region Fields
    // Public fields --v

    // The single instance of GameManager allowed.
    public static GameManager instance;

    // A list of ALL Players' TankData components (dead or alive).
    public List<TankData> allPlayers;

    // A list of the currently ALIVE players' TankData Components.
    public List<TankData> alivePlayers;

    // A list of ALL AIs' TankData components (dead or alive).
    public List<TankData> allAIs;

    // A list of the currently ALIVE AIs' TankData Components.
    public List<TankData> aliveAIs;

    // Serialized private fields --v

    // Private fields --v

    #endregion Fields

    #region Unity Methods
    // Runs when created, before Start().
    public void Awake()
    {
        // If no other instance of GameManager currently exists,
        if (instance == null)
        {
            // then save the instance as this instance of GameManager.
            instance = this;
        }
        // Else, there is already another GM. Log an error and destroy this gameObject.
        else
        {
            // Log the error.
            Debug.LogError("ERROR: There can be only 1 GameManager.");
            // Destroy this gameObject.
            Destroy(gameObject);
        }
    }

    // Called before the first frame.
    public void Start()
    {
        // Set variables --v


    }

    // Called every frame.
    public void Update()
    {

    }
    #endregion Unity Methods

    #region Dev-Defined Methods
    // Assign a camera to the tank that called this method.
    // Currently only works with main camera.
    public void AssignCamera(Transform newParent)
    {
        Transform mainCamTf = Camera.main.transform;
        mainCamTf.parent = newParent;
        mainCamTf.position = newParent.position;
        mainCamTf.rotation = newParent.rotation;


    }
    #endregion Dev-Defined Methods
}
