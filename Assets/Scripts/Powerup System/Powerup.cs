using UnityEngine;

[System.Serializable]
public class Powerup {

    #region Fields
    // Can modify the tank's speed.
    public float speedModifier;

	// Can modify the tank's health.
	public float healthModifier;

	// Can modify the tank's max health.
	public float maxHealthModifier;

	// Can modify the tank's fire rate.
	public float fireRateModifier;

	// Can modify the tank's damage dealt.
	public float damageModifier;

	// How long this duration is meant to last.
	public float powerupDuration;

	// Whether or not this powerup is meant to make permanent changes.
	public bool isPermanent;
	#endregion Fields


	#region Dev-Defined Methods
	// We call this when this object is activated.
	public void OnActivate(TankData target)
    {
		// Apply the speed multiplier.
		target.moveSpeed_Forward += speedModifier;

		// Apply the health modifier.
		target.currentHealth += healthModifier;

		// Apply the max health modifier.
		target.maxHealth += maxHealthModifier;

		// Apply the fire rate modifier.
		target.fireRate += fireRateModifier;

		// Apply the damage modifier.
		target.shellDamage += damageModifier;
	}

	// We call this when this object is deactivated.
	public void OnDeactivate(TankData target)
    {
		// Undo the changes made to the tank's speed.
		target.moveSpeed_Forward -= speedModifier;

		// Undo the changes made to the tank's speed.
		target.currentHealth -= healthModifier;

		// Undo the changes made to the tank's speed.
		target.maxHealth -= maxHealthModifier;

		// Undo the changes made to the tank's speed.
		target.fireRate -= fireRateModifier;

		// Undo the changes made to the tank's damage.
		target.shellDamage -= damageModifier;
	}
	#endregion Dev-Defined Methods
}
