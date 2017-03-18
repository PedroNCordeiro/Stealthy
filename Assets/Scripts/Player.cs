using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingObject {

	private void OnTriggerEnter2D(Collider2D other)
	{
		// Check if GameOver
		if (other.gameObject.layer == LayerMask.NameToLayer("DangerFloor")) {
			Destroy (gameObject);
		}
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
			Move (horizontal, vertical, out hit);
		}

	}
}
