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

		if (horizontal != 0 || vertical != 0) {
			MovePlayer ();
		}

	}

	// The player will try to move if there is not a blocking object in the way
	// If thhere is, and it is a crate, he will push it
	private void MovePlayer()
	{
		RaycastHit2D hit;

		if (endedMove) {
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
		
	private void OnTriggerEnter2D(Collider2D other)
	{
		otherCollider = other;

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
