using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

	public int visionDistance;
	public LayerMask safeLayerMask;

	// The enemy will look at his surroundings
	// In practice, this will change the layer masks of all non-blocking objects within its vision to 'DangerFloor'
	private void StayAlert(Vector2 currentPosition)
	{
		RaycastHit2D hit;

		boxCollider.enabled = false;
		for (int i = 0; i <= visionDistance; i++) {
			Vector2 endPosition = new Vector2 (currentPosition.x, currentPosition.y - i);
			hit = Physics2D.Linecast (currentPosition, endPosition, safeLayerMask);

			if (hit.transform.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
				return;
			}

			hit.transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
		}
		boxCollider.enabled = true;
	}

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		StayAlert (transform.position);
	}
}
