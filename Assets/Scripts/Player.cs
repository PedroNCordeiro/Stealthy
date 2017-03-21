using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingObject {

	public Collider2D otherCollider;

	protected override void Start ()
	{
			base.Start ();
			GameController.singleton.AddPlayer (this);
	}

	// Update is called once per frame
	void Update () {

		horizontal = (int)Input.GetAxisRaw ("Horizontal");
		vertical = (int)Input.GetAxisRaw ("Vertical");

		if (horizontal != 0) {
			vertical = 0;
		}

		RaycastHit2D hit;

		if ((horizontal != 0 || vertical != 0) && endedMove) {
			endedMove = false;
			if (!Move (horizontal, vertical, out hit)) {
				// Did we hit a blocking object?
				if (hit.transform.gameObject.tag == "BlockingObject") {
					Crate crate = hit.transform.GetComponent<Crate>() as Crate;
					if (crate.endedMove) {
						crate.endedMove = false;
						if (!crate.Move (horizontal, vertical, out hit)) {
							crate.endedMove = true;
						}
					}
				}
				endedMove = true;
			}
		}

	}
		
	private void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.tag == "BlockingObject") {
			// Kinematic rigidbodyes are not to be considered
			if (coll.rigidbody == null || coll.rigidbody.isKinematic) {
				return;
			}
			coll.rigidbody.velocity = new Vector2(horizontal, vertical);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		otherCollider = other;

		if (other.gameObject.tag == "BlockingObject") {
			Debug.Log ("Found crate!");
		}
		// Check if GameOver
		if (other.gameObject.layer == LayerMask.NameToLayer("DangerFloor")) {
			Die ();
		}
	}

	public void Die()
	{
		Destroy (gameObject);
	}

}
