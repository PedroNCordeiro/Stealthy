using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingObject {

	private bool laserKeyPressed = false;
	private bool hasLaser = false;
	private bool movementInputReady = true;
	private bool itemInputReady = true;
	private float invisibilityTime;
	private Vector2 direction = new Vector2(1, 0); // Keeps the direction the player is facing; Starts looking right

	public float movementInputDelay;
	public float itemInputDelay;
	[Range(1f, float.MaxValue)]
	public float potionOfInvisilibityDuration;
	public bool isInvisible = false;


	protected override void Start ()
	{
			base.Start ();
			GameController.singleton.AddPlayer (this);
	}

	// Update is called once per frame
	void Update () {
		CheckInputs ();
	}

	protected override void OnTriggerEnter2D(Collider2D other)
	{
		base.OnTriggerEnter2D(other);

		// Check if GameOver
		if (other.gameObject.layer == LayerMask.NameToLayer ("DangerFloor") && !isInvisible) {
			Die ();
		}

		else if (other.gameObject.tag == "Finish") {
			GameController.singleton.NextLevel ();
		}

		else if (other.gameObject.tag == "Potion") {
			Destroy (other.gameObject);
			isInvisible = true;
			StartCoroutine (RemainInvisible (potionOfInvisilibityDuration));
		}

		else if (other.gameObject.tag == "Laser") {
			Destroy (other.gameObject);
			hasLaser = true;
		}

	}

	// Reads all item inputs
	private IEnumerator CheckItemInputs()
	{
		if (itemInputReady) {
			// Check if any input this frame input at this frame
			laserKeyPressed = Input.GetKeyDown (KeyCode.Z);
			if (laserKeyPressed && hasLaser) {

				itemInputReady = false;

				UseLaser ();

				yield return new WaitForSeconds (itemInputDelay);

				itemInputReady = true;
			}
		}

		yield return null;
	}

	// Reads all player movement inputs
	private IEnumerator CheckMovementInputs()
	{
		if (movementInputReady) {

			horizontal = (int)Input.GetAxisRaw ("Horizontal");
			vertical = (int)Input.GetAxisRaw ("Vertical");

			if (horizontal != 0) {
				vertical = 0;
			}

			if (horizontal != 0 || vertical != 0) {

				movementInputReady = false;

				// The player only moves if he doesn't change his direction
				if (ChangeInDirection (horizontal, vertical)) {
					SaveDirection (horizontal, vertical);
					yield return new WaitForSeconds (movementInputDelay);
				} else {
					MovePlayer ();
				}

				movementInputReady = true;
			}
		}

		yield return null;
	}

	// Checks all the player inputs
	private void CheckInputs()
	{
		StartCoroutine (CheckItemInputs ());
		StartCoroutine (CheckMovementInputs ());
	}

	// Checks if the player movement input given by <horizontal, vertical> differs from the previous input
	// i.e. Checks if the player changed direction
	private bool ChangeInDirection(int horizontal, int vertical)
	{
		if (horizontal != direction.x || vertical != direction.y) {
			return true;
		}
		return false;
	}

	// Save the direction the player is facing
	private void SaveDirection(int horizontal, int vertical)
	{
		direction = new Vector2 (horizontal, vertical);
	}

	// Checks if there is a blocking object in the position given by <position>
	private bool FindBlockingObjectAtPosition(Vector2 position, out RaycastHit2D[] hits)
	{
		hits = Physics2D.RaycastAll (position, position, 0);
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Encontrei Transform nula!");
				continue;
			}
			if (hits [i].transform.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
				return true;
			}
		}
		return false;
	}

	// Checks if there is an item (potion, laser, etc) in the position given by <position>
	// All items will have a layer between {Floor, DangerousFloor} and will have a specific tag
	// The exit is also considered an item for this function's purpose
	private bool FindItemAtPosition(Vector2 position, out RaycastHit2D[] hits)
	{
		hits = Physics2D.RaycastAll (position, position, 0);
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Encontrei Transform nula!");
				continue;
			}
			if (hits [i].transform.gameObject.layer != LayerMask.NameToLayer ("BlockingLayer") && hits[i].transform.gameObject.tag != "Floor") {
				return true;
			}
		}
		return false;
	}

	// Checks if the player can use the laser in the position given by <position>
	// He can't use it if there is a blocking object there
	// Or if there is an object above that floor
	private bool CanUseLaser(Vector2 position, out RaycastHit2D[] hits)
	{
		if (FindBlockingObjectAtPosition (position, out hits) || FindItemAtPosition(position, out hits)) {
			return false;
		}
		return true;
	}

	// Uses the laser in the floor in front of the player
	// Unless there is something there
	private void UseLaser()
	{
		RaycastHit2D[] hits;
		Vector2 floorPosition = new Vector2 (transform.position.x + direction.x, transform.position.y + direction.y);

		if (CanUseLaser(floorPosition, out hits)) {
			for (int i = 0; i < hits.Length; i++) {
				if (hits [i].transform == null) {
					Debug.Log ("Encontrei uma transform nula!");
					continue;
				}
				hits [i].transform.gameObject.tag = "LaserFloor";
				Floor floor = hits [i].transform.GetComponent<Floor> () as Floor;
				floor.DamageFloorByLaser ();
			}
			//hasLaser = false;
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
					Crate crate = hit.transform.GetComponent<Crate> () as Crate;
					if (crate.endedMove) {
						crate.Move (horizontal, vertical, out hit);
					}
				} else if (hit.transform.gameObject.tag == "LightSwitch") {
					Debug.Log ("Tried (and failed!) to move to the light switch!");
					Light light = GameObject.FindGameObjectWithTag("MainLight").GetComponent<Light>() as Light;
					light.intensity = 0f;
				}
			}
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
