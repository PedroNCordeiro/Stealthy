using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

	private Vector2 blockingObjectPosition = Vector2.zero;

	public int visionDistance;

	// Checks if there is a blocking object on enemy's sight
	private bool BlockingObjectInVision(RaycastHit2D[] hits)
	{
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				return false;
			} else if (hits [i].transform.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
				blockingObjectPosition = new Vector2 (hits [i].transform.position.x, hits [i].transform.position.y);
				return true;
			}
		}
		return false;
	}

	// All objects in enemy's sight will change their layer to 'DangerousLayer'
	private void MarkObjectsAsDangerous(RaycastHit2D[] hits)
	{
		for (int i = 0; i < hits.Length; i++) {
			hits [i].transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
			hits [i].transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;
		}
	}

	private void CheckObjectsWithinVision(out RaycastHit2D[] hits)
	{
		float distance = visionDistance - 1;
		Vector2 origin = new Vector2 ((transform.position.x + horizontal), (transform.position.y + vertical));
		Vector2 direction = new Vector2 (horizontal, vertical);

		hits = Physics2D.RaycastAll (origin, direction, distance);
	}

	private void UpdateFloorBelow(Vector2 floorPosition, out RaycastHit2D[] hits)
	{
		Vector2 direction = new Vector2 (horizontal, vertical);

		hits = Physics2D.RaycastAll (floorPosition, direction, 0);

		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				break;
			}
			hits[i].transform.gameObject.layer = LayerMask.NameToLayer ("Floor");
			hits[i].transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
		}
	}

	// The enemy will look at his surroundings
	// In practice, this will change the layer masks of all non-blocking objects within its vision to 'DangerFloor'
	// All objects within this vision range are also modified graphically
	private void Look(int horizontal, int vertical)
	{		
		RaycastHit2D[] hits;

		boxCollider.enabled = false;

		Vector2 origin = new Vector2 ((transform.position.x), (transform.position.y));
		Vector2 direction = new Vector2 (horizontal, vertical);

		// Firstly, we will remove the floor below the enemy as DangerFloor
		// Since the enemy can no longer see it
		//UpdateFloorBelow(origin, out hits);

		hits = Physics2D.RaycastAll (origin, direction, 0);

		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				break;
			}
			hits[i].transform.gameObject.layer = LayerMask.NameToLayer ("Floor");
			hits[i].transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
		}

		boxCollider.enabled = true;


		// Then we'll want to check all objects within his vision range
		//CheckObjectsWithinVision(out hits);
		float distance = visionDistance - 1;
		origin = new Vector2 ((transform.position.x + horizontal), (transform.position.y + vertical));
		direction = new Vector2 (horizontal, vertical);

		hits = Physics2D.RaycastAll (origin, direction, distance);


		// All objects are gonna be checked if they are blocking further vision (objects from the layer 'BlockingLayer')
		// If so, we will store the first blocking object's position
		// And mark all objects until that point as Dangerous
		// (Dangerous objects are objects seen by the enemy)

		if (BlockingObjectInVision (hits)) {
			float distanceToBlockingObject = Vector2.Distance (origin, blockingObjectPosition);
			if (distanceToBlockingObject == 0) {
				return;
			}
			hits = Physics2D.RaycastAll (origin, direction, distanceToBlockingObject - 1);
		}

		MarkObjectsAsDangerous (hits);
	}

	protected override IEnumerator SmoothMovement(Vector3 end)
	{
		yield return StartCoroutine (base.SmoothMovement (end));
		Look(horizontal, vertical);
	}

	private void ChangePatrolDirection(int horizontal, int vertical)
	{
		if (horizontal == 1) {
			this.horizontal = 0;
			this.vertical = 1;
		} else if (vertical == 1) {
			this.vertical = 0;
			this.horizontal = -1;
		} else if (horizontal == -1) {
			this.horizontal = 0;
			this.vertical = -1;
		} else if (vertical == -1) {
			this.vertical = 0;
			this.horizontal = 1;
		}

		Look(this.horizontal, this.vertical);

		// We flag endedMove so that the enemy continues patrolling given the new direction
		endedMove = true;
	}
		
	private void Patrol()
	{
		RaycastHit2D hit;

		if (!Move (horizontal, vertical, out hit)) {
			ChangePatrolDirection (horizontal, vertical);
		}
	}

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		horizontal = 1;

		Look(horizontal, vertical);
	}

	void Update()
	{
		if (endedMove) {
			endedMove = false;
			Patrol ();
		}
	}
}
