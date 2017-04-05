using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject {

	private Vector2 blockingObjectPosition = Vector2.zero;

	public int visionDistance;
	public bool canMove;
	public Dictionary <Vector2, List <Vector2>> dangerousFloorPositions;
	public Vector2 lightSwitchPosition;
	public float fovAngle;

	// Use this for initialization
	protected override void Start () {
		base.Start ();

		dangerousFloorPositions = new Dictionary<Vector2, List <Vector2>> ();
		GameController.singleton.AddEnemyToList (this);

		direction = new Vector2 (1, 0); // facing upwards
	}
		
	void Update()
	{
		Look();
	}

	void OnDestroy()
	{
		ResetLook ();
	}

	protected override IEnumerator SmoothMovement(Vector3 end)
	{
		yield return StartCoroutine (base.SmoothMovement (end));
		Look();
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

		StartCoroutine(GameController.singleton.SwitchLights());

		yield return null;
	}


	private void MarkFloorAsDangerous(RaycastHit2D hit, Vector2 rayRotation)
	{
		// Only mark the floor as dangerous if it not in the list already
		if (!dangerousFloorPositions.ContainsKey (hit.transform.position)) {
			List <Vector2> rotation = new List<Vector2> ();
			rotation.Add (rayRotation);

			dangerousFloorPositions.Add (hit.transform.position, rotation);
			hit.transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
			hit.transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;
		} 

		else if (!dangerousFloorPositions [hit.transform.position].Contains (rayRotation)) {
				dangerousFloorPositions [hit.transform.position].Add (rayRotation);
		}
	}

	private void MarkFloorAsRegular(RaycastHit2D hit, Vector2 rayDirection)
	{
		dangerousFloorPositions.Remove (hit.transform.position);
		if (!GameController.singleton.IsFloorSeenByAnotherEnemy (hit.transform.position, this)) {
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
				blockingObjectPosition = hits[i].transform.position;
				if (hits [i].transform.gameObject.tag == "Player") {
					if (GameController.singleton.IsPlayerInvisible ()) {
						continue;
					}
					GameController.singleton.GameOver ();
				}
				return true;
			}
		}
		return false;
	}
		
	// All objects hit by the input RaycastHit array will change their layer to 'DangerousLayer'
	// With the exception of the player when invisible
	// If the player is invisible, he will keep its layer (blocking layer)
	private void MarkObjectsAsDangerous(RaycastHit2D[] hits, Vector2 rayRotation)
	{
		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Found null transform!");
				continue;
			} 
			if (hits [i].transform.gameObject.tag == "Player") {
				continue;
			}
			if (hits [i].transform.gameObject.layer != LayerMask.NameToLayer ("BlockingLayer")) {
				MarkFloorAsDangerous (hits [i], rayRotation);
			}
		}
	}
		
	// All objects previously marked as Dangerous
	// will now be reset (their layer mask will become "Floor" again)
	private void ResetLookRay(Vector2 rayDirection)
	{
		RaycastHit2D[] hits;

		Vector2 origin = transform.position;

		hits = Physics2D.RaycastAll (origin, rayDirection, visionDistance);

		for (int i = 0; i < hits.Length; i++) {
			if (hits [i].transform == null) {
				Debug.Log ("Found null transform!");
				continue;
			}
			if (hits[i].transform.gameObject.layer == LayerMask.NameToLayer("DangerFloor")) { 
				if (dangerousFloorPositions.ContainsKey (hits [i].transform.position)) {
					dangerousFloorPositions [hits [i].transform.position].Remove (rayDirection);
				
					if (dangerousFloorPositions [hits [i].transform.position].Count == 0) {
						MarkFloorAsRegular (hits [i], rayDirection);
					}
				}
			}
		}
			
	}

	// After the enemy moves, some floor tiles will be out of his vision
	private void ResetLookAfterLastMove()
	{
		RaycastHit2D[] hits;
		Vector2 origin = transform.position;

		float distance = visionDistance +1f;
		origin = new Vector2 (transform.position.x - direction.x, transform.position.y - direction.y);

		float fovAngleRadians = Mathf.Deg2Rad * fovAngle;
		int maxVisionWidth = (int)(Mathf.Ceil(Mathf.Tan (fovAngleRadians) * distance));

		for (int i = -maxVisionWidth; i <= maxVisionWidth; i++) {
			Vector2 endPoint;

			// Horizontal direction
			if (direction.x != 0) {
				endPoint = new Vector2 (direction.x + distance * direction.x, direction.y + i);
			}

			// Vertical direction
			else {
				endPoint = new Vector2 (direction.x + i, direction.y + distance * direction.y);
			}

			float angle = Vector2.Angle (direction, endPoint);	

			Vector3 rayRotation = Quaternion.AngleAxis (angle*Mathf.Sign(i), transform.forward) * direction;

			hits = Physics2D.RaycastAll (origin, rayRotation, distance);	

			for (int j = 0; j < hits.Length; j++) {
				if (hits [j].transform == null) {
					Debug.Log ("Found null transform!");
					continue;
				}
				if (hits[j].transform.gameObject.layer == LayerMask.NameToLayer("DangerFloor")) {
					MarkFloorAsRegular (hits [j], rayRotation);
				}
			}
		}
	}

	// All objects previously marked as Dangerous
	// will now be reset (their layer mask will become "Floor" again)
	private void ResetLook()
	{
		RaycastHit2D[] hits;
		Vector2 origin = transform.position;

		origin = transform.position;

		float fovAngleRadians = Mathf.Deg2Rad * fovAngle;
		int maxVisionWidth = (int)(Mathf.Ceil(Mathf.Tan (fovAngleRadians) * visionDistance));

		for (int i = -maxVisionWidth; i <= maxVisionWidth; i++) {
			Vector2 endPoint;

			// Horizontal direction
			if (direction.x != 0) {
				endPoint = new Vector2 (direction.x + visionDistance * direction.x, direction.y + i);
			}

			// Vertical direction
			else {
				endPoint = new Vector2 (direction.x + i, direction.y + visionDistance * direction.y);
			}

			float angle = Vector2.Angle (direction, endPoint);	

			Vector3 rayRotation = Quaternion.AngleAxis (angle*Mathf.Sign(i), transform.forward) * direction;

			hits = Physics2D.RaycastAll (origin, rayRotation, visionDistance);	

			for (int j = 0; j < hits.Length; j++) {
				if (hits [j].transform == null) {
					Debug.Log ("Found null transform!");
					continue;
				}
				if (hits[j].transform.gameObject.layer == LayerMask.NameToLayer("DangerFloor")) {
					MarkFloorAsRegular (hits [j], rayRotation);
				}
			}
		}
	}

	// The enemy will look at his surroundings
	// In practice, this will change the layer masks of all non-blocking objects within its vision to 'DangerFloor'
	private void Look()
	{		
		RaycastHit2D[] hits;
		Vector2 origin = transform.position;

		// Firstly, we will remove the floor tiles that the enemy no longer sees (because of his movement)
		ResetLookAfterLastMove();

		// Origin will be in the center of the tile that is in front of the enemy
		origin = transform.position;

		float fovAngleRadians = Mathf.Deg2Rad * fovAngle;
		int maxVisionWidth = (int)(Mathf.Ceil(Mathf.Tan (fovAngleRadians) * visionDistance));

		for (int i = -maxVisionWidth; i <= maxVisionWidth; i++) {
			Vector2 endPoint;

			// Horizontal direction
			if (direction.x != 0) {
				endPoint = new Vector2 (visionDistance * direction.x, direction.y + i);
			}

			// Vertical direction
			else {
				endPoint = new Vector2 (direction.x + i, visionDistance * direction.y);
			}
				
			float angle = Vector2.Angle (direction, endPoint);	

			Vector3 rayRotation = Quaternion.AngleAxis (angle * Mathf.Sign (i), transform.forward) * direction;

			boxCollider.enabled = false;
			hits = Physics2D.RaycastAll (origin, rayRotation, visionDistance);
			boxCollider.enabled = true;
			Debug.DrawRay (origin, rayRotation * visionDistance, Color.white);


			if (BlockingObjectInVision (hits)) {
				ResetLookRay (rayRotation);

				float distanceToBlockingObject = Vector2.Distance (origin, blockingObjectPosition);

				if (distanceToBlockingObject < 1) {
					return;
				}

				// Mark every floor before blocking object as dangerous
				hits = Physics2D.RaycastAll (origin, rayRotation, distanceToBlockingObject - 1);
			}

			MarkObjectsAsDangerous (hits, rayRotation);
		}
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

		Look();
	}

	public void Patrol()
	{
		RaycastHit2D hit;
		RaycastHit2D[] hits;

		horizontal = (int)direction.x;
		vertical = (int)direction.y;

		// Before trying to move, the enemy will check if there is a hole in front of him
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
