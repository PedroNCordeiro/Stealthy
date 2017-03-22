using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingObject {

	[Range(1f, float.MaxValue)]
	public float potionOfInvisilibityDuration;
	public float invisibilityTime;
	public bool isInvisible = false;

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
			if (!Move (horizontal, vertical, out hit)) {
				// Did we hit a blocking object?
				if (hit.transform.gameObject.tag == "BlockingObject") {
					Crate crate = hit.transform.GetComponent<Crate>() as Crate;
					if (crate.endedMove) {
						crate.Move (horizontal, vertical, out hit);
					}
				}
			}
		}
	}
		
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "Enemy") {
			StopCoroutine (SmoothMovementCoRoutine);
			StartCoroutine (SmoothMovementBackCoRoutine);
		}

		// Check if GameOver
		else if (other.gameObject.layer == LayerMask.NameToLayer ("DangerFloor") && !isInvisible) {
			Die ();
		} else if (other.gameObject.tag == "Potion") {
			Destroy (other.gameObject);
			isInvisible = true;
			StartCoroutine (RemainInvisible (potionOfInvisilibityDuration));
		}
	}

	// Make the player invisible for a given duration
	private IEnumerator RemainInvisible(float duration)
	{
		invisibilityTime = duration;
		while (invisibilityTime > float.Epsilon) {
			invisibilityTime -= Time.deltaTime;
			yield return null;
		}

		isInvisible = false;
	}

	public void Die()
	{
		Destroy (gameObject);
	}

}
