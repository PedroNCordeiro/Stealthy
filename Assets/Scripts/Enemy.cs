using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

	public int visionDistance = 4;
	public LayerMask floorLayerMask;


	private void ChangeFloorTags(Vector2 currentPosition)
	{
		RaycastHit2D hit;
		boxCollider.enabled = false;
		for (int i = 1; i <= visionDistance; i++) {
			Vector2 endPosition = new Vector2 (currentPosition.x, currentPosition.y - i);
			hit = Physics2D.Linecast (currentPosition, endPosition, floorLayerMask);
			hit.transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
		}
		boxCollider.enabled = true;
	}

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		ChangeFloorTags (transform.position);
	}
}
