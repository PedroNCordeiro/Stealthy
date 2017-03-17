using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

	public int visionDistance;
	public LayerMask safeLayerMask;
	public LayerMask dangerousLayerMask;

	// The enemy will look at his surroundings
	// In practice, this will change the layer masks of all non-blocking objects within its vision to 'DangerFloor'
	private void CheckVision(int horizontal, int vertical)
	{
		RaycastHit2D hit;

		boxCollider.enabled = false;
		for (int i = 0; i <= visionDistance; i++) {
			Vector2 endPosition = new Vector2 (transform.position.x, transform.position.y - i);
			hit = Physics2D.Linecast (transform.position, endPosition, safeLayerMask);

			if (hit.transform.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
				return;
			}

			hit.transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
			hit.transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;
		}
		boxCollider.enabled = true;
	}
		
	private void VisionWhileMoving(int horizontal, int vertical)
	{
		RaycastHit2D hit;

		boxCollider.enabled = false;


		// Add a new floor tile to the enemy vision
		Vector2 newFloorInVision = new Vector2 (transform.position.x + (horizontal * visionDistance), transform.position.y + (vertical * visionDistance));
		hit = Physics2D.Linecast (transform.position, newFloorInVision, safeLayerMask);

		if (hit.transform == null) {
			return;
		}

		else if (hit.transform.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
			return;
		}

		hit.transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
		hit.transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;




		// The floor tile behind the player is not dangerous anymore
		// We will put its layer mask to 'Floor' again
		Vector2 floorBehind = new Vector2 (transform.position.x - horizontal, transform.position.y - vertical);
		hit = Physics2D.Linecast (transform.position, floorBehind, dangerousLayerMask);
		if (hit.transform == null) {
			return;
		}
		hit.transform.gameObject.layer = LayerMask.NameToLayer ("Floor");
		hit.transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.red;


		boxCollider.enabled = true;
	}

	private void Patrol()
	{
		RaycastHit2D hit;
		vertical = -1;
		if (endedMove) {
			Move (horizontal, vertical, out hit);

			// Add 1 floor to enemy vision radius
			VisionWhileMoving(horizontal, vertical);
		}
	}

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		vertical = -1;
		CheckVision (horizontal, vertical);
	}

	void Update()
	{
		Patrol ();
	}
}
