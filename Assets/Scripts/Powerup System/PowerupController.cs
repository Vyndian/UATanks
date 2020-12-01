using System.Collections.Generic;
using UnityEngine;

// Requires that a TankData script accompany this script.
[RequireComponent(typeof(TankData))]

public class PowerupController : MonoBehaviour {

    #region Fields
    // Keeps track of each Powerup on this tank.
    public List<Powerup> powerups;

    // Reference to the TankData on this tank.
    [SerializeField] private TankData data;
    #endregion Fields


    #region Unity Methods
    // Called before the first frame.
    public void Start()
    {
        // Initialize our list of Powerups.
        powerups = new List<Powerup>();

        // If data is null,
        if (data == null)
        {
            // then get the TankData off of this tank.
            data = GetComponent<TankData>();
        }
    }

    // Called every frame.
    public void Update()
    {
        // Create a temporary list to hold the Powerups that need to be removed from powerups.
        List<Powerup> expiredPowerups = new List<Powerup>();

        // Iterate through each Powerup in powerups.
        foreach (Powerup powerup in powerups)
        {
            // Subtract from the timer/duration of each.
            powerup.powerupDuration -= Time.deltaTime;

            // If that powerup is now expired,
            if (powerup.powerupDuration <= 0)
            {
                expiredPowerups.Add(powerup);
            }
        }

        // Iterate through the list of expired Powerups.
        foreach (Powerup powerup in expiredPowerups)
        {
            // Deactivate that powerup.
            powerup.OnDeactivate(data);

            // Remove that powerup from the list of active Powerups.
            powerups.Remove(powerup);
        }

        // exporedPowerups is local, and will therefore expire at the end of the Update function.
        // To show how to clear a list, though, the next line is how to do so (commented out since it is not neccessary).
        //expiredPowerups.Clear();
    }
    #endregion Unity Methods


    #region Dev-Defined Methods
    // Adds a powerup to this tank.
    public void AddPowerup(Powerup powerup)
    {
        // Activate the Powerup.
        powerup.OnActivate(data);

        // If the powerup is not permanent,
        if (!powerup.isPermanent)
        {
            // then add it to the list of Powerups that are active and will eventually expire.
            powerups.Add(powerup);
        }
    }
    #endregion Dev-Defined Methods
}
