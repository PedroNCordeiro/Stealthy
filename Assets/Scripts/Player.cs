using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MovingObject {

	private bool interactionKeyPressed = false;

	private bool movementInputReady = true;
	public float movementInputDelay;


	public struct Item {
		public bool keyPressed;
		public int charges;
		public bool inputReady;
		public float inputDelay;
	}

	private Item laser;
	public float laserInputDelay;


	public struct Consumable {
		public float timeLeft;
		public float duration;
		public bool isActive;
	}

	private Consumable invisibilityPotion;
	public float potionOfInvisilibityDuration;


	protected override void Start ()
	{
		animator = GetComponent<Animator> ();

		base.Start ();
		GameController.singleton.AddPlayer (this);

		laser.keyPressed = false;
		laser.charges = 0;
		laser.inputReady = true;
		laser.inputDelay = laserInputDelay;

		invisibilityPotion.isActive = false;
		invisibilityPotion.duration = potionOfInvisilibityDuration;
	}

	// Update is called once per frame
	void Update () {
		CheckInputs ();
	}

	protected override void OnTriggerEnter2D(Collider2D other)
	{
		base.OnTriggerEnter2D(other);

		// Check if GameOver
		if (other.gameObject.layer == LayerMask.NameToLayer ("DangerFloor") && !invisibilityPotion.isActive) {
			Die ();
		}

		else if (other.gameObject.tag == "Finish") {
			GameController.singleton.NextLevel ();
		}

		else if (other.gameObject.tag == "Potion") {
			BecomeInvisible (other);
		}

		else if (other.gameObject.tag == "Laser") {
			Destroy (other.gameObject);
			laser.charges = 1;
		}
	}

	// Reads all inputs related to player interactions
	private IEnumerator CheckInteractionInputs()
	{
		interactionKeyPressed = Input.GetKeyDown (KeyCode.X);

		if (interactionKeyPressed) {

			if (GameController.singleton.lightSwitchInputReady) {
				string objectTag;

				if (FindInteractiveObject (out objectTag)) {
					if (objectTag == "LightSwitch") {
						StartCoroutine(GameController.singleton.SwitchLights());
					}
				}

			}
		}

		yield return null;
	}

	// Reads all inputs related to player items
	private IEnumerator CheckItemInputs()
	{
		if (laser.inputReady) {

			laser.keyPressed = Input.GetKeyDown (KeyCode.Z);
			if (laser.keyPressed && laser.charges > 0) {

				laser.inputReady = false;

				UseLaser ();

				yield return new WaitForSeconds (laserInputDelay);

				laser.inputReady = true;
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
					ChangeSpriteDirection (horizontal, vertical);
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
		StartCoroutine (CheckInteractionInputs ());
		StartCoroutine (CheckItemInputs ());
		StartCoroutine (CheckMovementInputs ());
	}
		
	// Checks if there is a blocking object in the position given by <position>
	private bool FindBlockingObjectAtPosition(Vector2 position, out RaycastHit2D[] hits)
	{
		hits = Physics2D.RaycastAll (position, position, 0);
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Found null transform!");
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
				Debug.Log ("Found null transform!");
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
					Debug.Log ("Found null transform!");
					continue;
				}
				hits [i].transform.gameObject.tag = "LaserFloor";
				Floor floor = hits [i].transform.GetComponent<Floor> () as Floor;
				floor.DamageFloorByLaser ();
			}
			laser.charges--;
		}
	}

	// The player will try to move if there is not a blocking object in the way
	// If there is, and it is a crate, he will push it
	private void MovePlayer()
	{
		RaycastHit2D hit;

		if (endedMove) {

			if (!Move (horizontal, vertical, out hit)) {

				// Did we hit a blocking object?
				if (hit.transform.gameObject.tag == "BlockingObject") {
					Crate crate = hit.transform.GetComponent<Crate> () as Crate;
					if (crate.endedMove) {
						crate.Drag (horizontal, vertical, hit);
					}
				}
			}
		}
	}

	// Checks if there is an object the player can interact with
	private bool FindInteractiveObject(out string objectTag)
	{
		// Interactive objects need to be in front of the player
		Vector2 positionInFront = new Vector2 (transform.position.x + direction.x, transform.position.y + direction.y);

		RaycastHit2D[] hits;
		hits = Physics2D.RaycastAll (positionInFront, positionInFront, 0);

		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Found null transform!");
				continue;
			}
			if (hits [i].transform.gameObject.tag == "LightSwitch") {
				objectTag = "LightSwitch";
				return true;
			}
		}

		objectTag = "None";
		return false;
	}

	private void BecomeInvisible(Collider2D other)
	{
		Destroy (other.gameObject);
		invisibilityPotion.isActive = true;

		StartCoroutine (RemainInvisible (invisibilityPotion.duration));
	}

	// Make the player invisible for a given duration
	private IEnumerator RemainInvisible(float duration)
	{
		gameObject.GetComponent<SpriteRenderer> ().color = Color.black;

		invisibilityPotion.timeLeft = duration;
		while (invisibilityPotion.timeLeft > float.Epsilon) {
			invisibilityPotion.timeLeft -= Time.deltaTime;
			yield return null;
		}

		gameObject.GetComponent<SpriteRenderer> ().color = Color.white;

		invisibilityPotion.isActive = false;
	}

	// Checks if the player is invisible
	public bool isInvisible()
	{
		return invisibilityPotion.isActive;
	}

	public void Die()
	{
		Destroy (gameObject);
	}
}
