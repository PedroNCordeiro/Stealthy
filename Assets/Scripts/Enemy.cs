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

		RaycastHit2D hit;
		Vector2 origin = new Vector2 (transform.position.x + horizontal, transform.position.y + vertical); 

		boxCollider.enabled = false;
		for (int i = 0; i < visionDistance; i++) {
			Vector2 endPosition = new Vector2 (origin.x + horizontal, origin.y + vertical);
			hit = Physics2D.Linecast (origin, endPosition);
			origin = endPosition;

			if (hit.transform.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
				return;
			}

			hit.transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
			hit.transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;
		}
		boxCollider.enabled = true;
	}
		
	// Enemy vision is updated when he finishes moving
	// Looks 1 floor further
	private IEnumerator VisionWhileMoving(int horizontal, int vertical)
	{		
		Debug.Log ("VisionWhileMoving inicio 2");
		RaycastHit2D hit;

		boxCollider.enabled = false;

		// The floor tile below the player is not dangerous anymore
		// We will put its layer mask to 'Floor' again
		hit = Physics2D.Linecast (transform.position, new Vector2(transform.position.x + horizontal, transform.position.y + vertical));
		if (hit.transform == null) {
			Debug.Log ("hit transform is null");
			yield break;
		}
		hit.transform.gameObject.layer = LayerMask.NameToLayer ("Floor");
		hit.transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.white;


		// Add a new floor tile to the enemy vision
		Vector2 newFloorInVision = new Vector2 (transform.position.x + (horizontal * visionDistance), transform.position.y + (vertical * visionDistance));
		hit = Physics2D.Linecast (newFloorInVision, new Vector2(newFloorInVision.x + horizontal, newFloorInVision.y + vertical));

		if (hit.transform == null) {
			yield break;
		}

		else if (hit.transform.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
			yield break;
		}

		hit.transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
		hit.transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;


		boxCollider.enabled = true;
		Debug.Log ("VisionWhileMoving fim");
	}

	protected override IEnumerator SmoothMovement(Vector3 end)
	{
		yield return StartCoroutine (base.SmoothMovement (end));
		yield return StartCoroutine (VisionWhileMoving (horizontal, vertical));
	}

	private void Patrol()
	{
		RaycastHit2D hit;
		vertical = -1;

		Move (horizontal, vertical, out hit);
		Debug.Log ("On Patrol funcion");
	}

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		vertical = -1;

		CheckVision (horizontal, vertical);
	}

	void Update()
	{
		if (endedMove) {
			endedMove = false;
			Patrol ();
		}
	}
}
