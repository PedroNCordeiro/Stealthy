using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour {

	private SpriteRenderer spriteRenderer;

	public Sprite laserFloor;
	public Sprite destructedFloor;

	// Will change the floor sprite if the player used the laser
	// The floor won't be destructed but damaged
	public void DamageFloorByLaser()
	{
		spriteRenderer = GetComponent<SpriteRenderer> ();

		spriteRenderer.sprite = laserFloor;
	}

	// Will change the floor sprite if someone stepped on the damaged floor
	// The floor will be destructed
	public void DestructFloor()
	{
		spriteRenderer.sprite = destructedFloor;
	}
}
