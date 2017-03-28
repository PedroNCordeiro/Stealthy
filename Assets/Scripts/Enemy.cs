using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

	private Vector2 blockingObjectPosition = Vector2.zero;

	public int visionDistance;
	public bool canMove;
	public List <Vector2> dangerousFloorPositions;
	public Vector2 lightSwitchPosition;

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		GameController.singleton.AddEnemyToList (this);
		dangerousFloorPositions = new List<Vector2> ();

		StartCoroutine(Look());
	}
		
	void Update()
	{
		StartCoroutine (Look ());
	}

	void OnDestroy()
	{
		ResetLook ();
	}

	protected override IEnumerator SmoothMovement(Vector3 end)
	{
		yield return StartCoroutine (base.SmoothMovement (end));
		StartCoroutine(Look());
	}

	// The enemy will follow a speficied path to return to his duty position
	public IEnumerator MoveToDutyPosition(Vector2[] path)
	{
		RaycastHit2D hit;
		int i = 0;
		while (i < path.Length) {
			if (endedMove) {
				int pathX = (int)path [i].x;
				int pathY = (int)path [i].y;

				if (ChangeInDirection (pathX, pathY)) {
					ResetLook ();
					SaveDirection (pathX, pathY);
				}
				Move ((int)path [i].x, (int)path [i].y, out hit);
				i++;
			}
			yield return null;
		}

		yield return new WaitForSeconds(moveTime);
	}

	// The enemy will follow a specified path to the LightSwitch
	public IEnumerator MoveToLightSwitch(Vector2[] path)
	{
		RaycastHit2D hit;
		int i = 0;
		while (i < path.Length) {
			if (endedMove) {
				int pathX = (int)path [i].x;
				int pathY = (int)path [i].y;

				if (ChangeInDirection(pathX, pathY)) {
					ResetLook (); 
					SaveDirection(pathX, pathY);
				}
				Move (pathX, pathY, out hit);
				i++;
			}
			yield return null;
		}

		yield return new WaitForSeconds(moveTime);

		bool lightsOFF = GameController.singleton.CheckLightsOff ();
		StartCoroutine(GameController.singleton.SwitchLights(!lightsOFF));

		yield return null;
	}


	private void MarkFloorAsDangerous(RaycastHit2D hit)
	{
		// Only mark the floor as dangerous if it not in the list already
		if (!dangerousFloorPositions.Contains (hit.transform.position)) {
			dangerousFloorPositions.Add (hit.transform.position);
			hit.transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
			hit.transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;
		}
	}

	private void MarkFloorAsRegular(RaycastHit2D hit)
	{
		dangerousFloorPositions.Remove (hit.transform.position);

		if (!GameController.singleton.IsFloorSeenByAnotherEnemy(hit.transform.position, this)) {
			hit.transform.gameObject.layer = LayerMask.NameToLayer ("Floor");
			hit.transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.white;
		}
	}

	// Checks if there is a blocking object on enemy's sight
	// Also specifically checks if that blocking object is the player
	// If so, it'll be Game Over, unless the player has taken a potion of invisibility
	private bool BlockingObjectInVision(RaycastHit2D[] hits)
	{
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Found null transform!");
				continue;
			} else if (hits [i].transform.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
				blockingObjectPosition = new Vector2 (hits [i].transform.position.x, hits [i].transform.position.y);
				if (hits [i].transform.gameObject.tag == "Player") {
					if (GameController.singleton.IsPlayerInvisible ()) {
						continue;
					}
					GameController.singleton.GameOver ();
				}
				ResetLook ();
				return true;
			}
		}
		return false;
	}
		
	// Floor below the enemy will change its layer from 'DangerousFloor' to 'Floor'
	// Since it is no longer seen by him
	private void UpdateFloorBelow(Vector2 floorPosition, out RaycastHit2D[] hits)
	{
		hits = Physics2D.RaycastAll (floorPosition, floorPosition, 0);

		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Found null transform!");
				continue;
			}
			MarkFloorAsRegular (hits [i]);
		}
	}

	// All objects hit by the input RaycastHit array will change their layer to 'DangerousLayer'
	// With the exception of the player when invisible
	// If the player is invisible, he will keep its layer (blocking layer)
	private void MarkObjectsAsDangerous(RaycastHit2D[] hits)
	{
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Found null transform!");
				continue;
			} else if (hits [i].transform.gameObject.tag == "Player") {
				continue;
			}
			MarkFloorAsDangerous (hits [i]);
		}
	}

	// All objects previously marked as Dangerous
	// will now be reset (their layer mask will become "Floor" again)
	private void ResetLook()
	{
		RaycastHit2D[] hits;

		Vector2 origin = new Vector2 (transform.position.x + direction.x, transform.position.y + direction.y);
		float distance = visionDistance - 1;

		hits = Physics2D.RaycastAll (origin, direction, distance);

		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Found null transform!");
				continue;
			}
			if (hits[i].transform.gameObject.layer == LayerMask.NameToLayer("DangerFloor")) {
				MarkFloorAsRegular (hits [i]);
			}
		}
	}

	// The enemy will look at his surroundings
	// In practice, this will change the layer masks of all non-blocking objects within its vision to 'DangerFloor'
	// All objects within this vision range are also modified graphically
	public IEnumerator Look()
	{		
		RaycastHit2D[] hits;

		Vector2 origin = new Vector2 ((transform.position.x), (transform.position.y));

		// Firstly, we will remove the floor below the enemy as DangerFloor
		// Since the enemy can no longer see it
		boxCollider.enabled = false;
		UpdateFloorBelow(origin, out hits);
		boxCollider.enabled = true;


		// Then we'll want to check all objects within his vision range
		float distance = visionDistance - 1;
		origin = new Vector2 ((transform.position.x + direction.x), (transform.position.y + direction.y));
		hits = Physics2D.RaycastAll (origin, direction, distance);


		// All objects are gonna be checked if they are blocking further vision (objects from the layer 'BlockingLayer')
		// If so, we will store the first blocking object's position
		// And mark all objects until that point as Dangerous
		// (Dangerous objects are objects seen by the enemy)

		if (BlockingObjectInVision (hits)) {
			float distanceToBlockingObject = Vector2.Distance (origin, blockingObjectPosition);
			if (distanceToBlockingObject < 1) {
				yield break;
			}
			hits = Physics2D.RaycastAll (origin, direction, distanceToBlockingObject - 1);
		}

		MarkObjectsAsDangerous (hits);

		yield return null;
	}
		
	// Returns true if there is a hole (destructed floor) in the position given by <position>
	// And returns false otherwise
	private bool FindHole(Vector2 position, out RaycastHit2D[] hits)
	{
		hits = Physics2D.RaycastAll (position, position, 0);
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Found null transform!");
				continue;
			}
			else if (hits[i].transform.gameObject.tag == "DestructedFloor") {
				return true;
			}
		}
		return false;
	}

	private void ChangePatrolDirection()
	{
		ResetLook ();

		if (direction.x == 1) {
			horizontal = 0;
			vertical = 1;
		} else if (direction.y == 1) {
			vertical = 0;
			horizontal = -1;
		} else if (direction.x == -1) {
			horizontal = 0;
			vertical = -1;
		} else if (direction.y == -1) {
			vertical = 0;
			horizontal = 1;
		}

		SaveDirection (horizontal, vertical);

		StartCoroutine(Look());
	}

	public void Patrol()
	{
		RaycastHit2D hit;
		RaycastHit2D[] hits;

		horizontal = (int)direction.x;
		vertical = (int)direction.y;

		// Before trying to move, the enemy will check if there is a hole in front of him
		// (Hole = destructed floor)
		// If there is, he won't move there, and will change direction instead
		Vector2 nextPosition = new Vector2(transform.position.x + direction.x, transform.position.y + direction.y);
		if (FindHole(nextPosition, out hits)) {
			ChangePatrolDirection ();
		}

		else if (!Move (horizontal, vertical, out hit)) {
			ChangePatrolDirection ();
		}
	}

}
