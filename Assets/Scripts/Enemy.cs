using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

	public int visionDistance;


	// The enemy will look at his surroundings
	// In practice, this will change the layer masks of all non-blocking objects within its vision to 'DangerFloor'
	private void CheckVision(int horizontal, int vertical)
	{
		if (visionDistance <= 0) {
			return;
		}

		RaycastHit2D[] hits;
		Vector2 origin = new Vector2 (transform.position.x + horizontal, transform.position.y + vertical); 
		Vector2 end = new Vector2 (transform.position.x + (horizontal * visionDistance), transform.position.y + (vertical * visionDistance));

		boxCollider.enabled = false;

		hits = Physics2D.RaycastAll (origin, end, (float)(visionDistance - 1.5));
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				break;
			}
			else if (hits[i].transform.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
				break;
			}

			hits[i].transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
			hits[i].transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;
		}
		boxCollider.enabled = true;
	}
		
	// Enemy vision is updated when he finishes moving
	// Looks 1 floor further
	private void VisionWhileMoving(int horizontal, int vertical)
	{		
		RaycastHit2D[] hits;

		boxCollider.enabled = false;

		Debug.Log ("Horizontal: " + horizontal);
		Debug.Log ("Vertical: " + vertical);
		hits = Physics2D.RaycastAll (transform.position, new Vector2 (transform.position.x + horizontal, transform.position.y + vertical), (float)0.5);

		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				break;
			}
			hits[i].transform.gameObject.layer = LayerMask.NameToLayer ("Floor");
			hits[i].transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
		}

		// Add a new floor tile to the enemy vision
		Vector2 newFloorInVision = new Vector2 (transform.position.x + (horizontal * visionDistance), transform.position.y + (vertical * visionDistance));
		Debug.Log ("Origin: " + new Vector2 (transform.position.x + horizontal, transform.position.y + vertical));
		Debug.Log ("End: " + newFloorInVision);

		hits = Physics2D.RaycastAll (new Vector2 (transform.position.x + horizontal, transform.position.y + vertical), newFloorInVision, (float)0.5);

		// Check for blocking objects
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				return;
			} else if (hits [i].transform.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
				Debug.Log ("Found a blocking layer");
				return;
			}
		}
		// No blocking object found
		for (int i = 0; i < hits.Length; i++) {
			hits[i].transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
			hits[i].transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;
		}
		boxCollider.enabled = true;
	}

	protected override IEnumerator SmoothMovement(Vector3 end)
	{
		yield return StartCoroutine (base.SmoothMovement (end));
		VisionWhileMoving(horizontal, vertical);
	}

	private void Patrol()
	{
		RaycastHit2D hit;
		//vertical = -1;

		Move (horizontal, vertical, out hit);
	}

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		vertical = -1;
		horizontal = 0;
		//horizontal = 1;

		//CheckVision (horizontal, vertical);
		VisionWhileMoving(horizontal, vertical);
	}

	void Update()
	{
		/*if (endedMove) {
			endedMove = false;
			Patrol ();
		}*/
	}
}
