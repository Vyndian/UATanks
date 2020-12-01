using UnityEngine;

public class CheatController : MonoBehaviour {

	// Holds a reference to the PowerupController on this tank.
	public PowerupController powCon;

	// The powerup that this cheat controller can add to the tank.
	public Powerup cheatPow;

	// Use this for initialization
	void Start () {

		// If this var is null,
		if (powCon == null)
        {
			// then get it from the gameObject.
			powCon = GetComponent<PowerupController>();
        }
	}
	
	// Update is called once per frame
	public void Update () {
		print("update");
		// If this is the first frame that P was pressed down,
		if (Input.GetKeyDown(KeyCode.P))
        {
			print("cheat keys pressed");
			// then add the cheat Powerup to the tank.
			powCon.AddPowerup(cheatPow);
        }
	}
}
