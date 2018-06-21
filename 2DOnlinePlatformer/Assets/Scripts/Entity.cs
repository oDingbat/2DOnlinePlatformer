using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour {

	[Space(10)][Header("Vitals")]
	public int		healthCurrent = 10;				// The amount of health this entity currently has
	public int		healthMax = 10;					// The maximum amount of health this entity can have
	public bool		isDead = false;					// Is this entity dead?
	public float	timeOfDeath;					// The time at which this entity last died
	public bool		isInvincible;					// Is this entity invincible?

	public void Die (PlayerController killer) {
		if (isDead == false) {
			isInvincible = false;
			isDead = true;
			timeOfDeath = Time.time;
			healthCurrent = 0;
			OnDie(killer);
		}
	}

	public void Revive () {
		isDead = false;
		timeOfDeath = 0;
		healthCurrent = healthMax;
		OnRevive();
	}

	public abstract void OnDie(PlayerController killer);
	public abstract void OnRevive();

}
