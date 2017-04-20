using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Enemy : MovingObject {

	private Vector2 blockingObjectPosition = Vector2.zero;
	private int visionDistance;

	public int maxVisionDistance;

	public bool canMove;
	public bool hasPatrolPath; // Defines if an enemy has a specific patrol path
	public Vector2[] patrolPath;

	public Dictionary <Vector2, List <Vector2>> dangerousFloorPositions;
	public Vector2 lightSwitchPosition;
	public float fovAngle;

	// Use this for initialization
	protected override void Start () {
		animator = GetComponent<Animator> ();

		base.Start ();

		dangerousFloorPositions = new Dictionary<Vector2, List <Vector2>> ();
		GameController.singleton.AddEnemyToList (this);

		visionDistance = maxVisionDistance;

		if (!canMove) {
			ChangeSpriteDirection ((int)direction.x, (int)direction.y);
		} else {
			StartSpriteMoveAnimation ((int)direction.x, (int)direction.y);
		}
	}
		
	void Update()
	{
		Look();
	}

	void OnDestroy()
	{
		ResetLook (visionDistance);
	}

	// Returns the origin of the raycast to be performed
	// The input parameters represent the raycast default origin if there wasn't an offset
	// (There is an offset on the enemy's position for graphical reasons)
	private Vector2 GetRaycastOriginTransform(Vector2 defaultOrigin)
	{
		Vector2 transformOffset = new Vector2 (transform.position.x % 1, transform.position.y % 1);
		return new Vector2 (defaultOrigin.x - transformOffset.x, defaultOrigin.y - transformOffset.y);
	}

	protected override IEnumerator SmoothMovement(Vector3 end)
	{
		yield return StartCoroutine (base.SmoothMovement (end));
		Look();
	}
		
	public void ChangeVisionDistance(int newVisionDistance)
	{
		visionDistance = newVisionDistance;
	}


	private IEnumerator MoveToPosition(Vector2[] path)
	{
		RaycastHit2D hit;
		int i = 0;
		while (i < path.Length) {
			if (endedMove) {
				int pathX = (int)path [i].x;
				int pathY = (int)path [i].y;

				if (ChangeInDirection(pathX, pathY)) {
					ResetLook (visionDistance); 
					SaveDirection(pathX, pathY);
				}
				Move (pathX, pathY, out hit);
				StartSpriteMoveAnimation (pathX, pathY);
				i++;
			}
			yield return null;
		}
	}

	// The enemy will follow a speficied path to return to his duty position
	public IEnumerator MoveToDutyPosition(Vector2[] path)
	{
		yield return StartCoroutine (MoveToPosition (path));

		ChangeSpriteDirection ((int)direction.x, (int)direction.y);
	}

	// The enemy will follow a specified path to the LightSwitch
	public IEnumerator MoveToLightSwitch(Vector2[] path)
	{
		// We need to call ResetLook with the vision radius before the lights went off
		// Otherwise not all floor tiles will be updated
		ResetLook (maxVisionDistance);

		yield return StartCoroutine (MoveToPosition (path));

		yield return new WaitForSeconds(moveTime);

		StartCoroutine(GameController.singleton.SwitchLights());
	}


	private void MarkFloorAsDangerous(RaycastHit2D hit, Vector2 rayRotation)
	{
		// Only mark the floor as dangerous if its position isn't in the <dangerousFloorPositions> list
		if (!dangerousFloorPositions.ContainsKey (hit.transform.position)) {
			List <Vector2> rotation = new List<Vector2> ();
			rotation.Add (rayRotation);

			dangerousFloorPositions.Add (hit.transform.position, rotation);
			hit.transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
			hit.transform.gameObject.GetComponent<SpriteRenderer> ().color = Color.cyan;
		} 

		// In case the floor's position is in the list, we want to add every angle that 'sees' that floor
		// This is useful because if there is a blocking object in sight, it will no longer be seen (dangerous) ONLY IF
		// not visible by any angle
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
		
	// All objects previously marked as Dangerous in the direction <rayDirection>
	// will now be reset (their layer mask will become "Floor" again)
	private void UpdateFloorsAfterBlockingObject(Vector2 rayDirection)
	{
		RaycastHit2D[] hits;

		Vector2 origin = GetRaycastOriginTransform(transform.position);

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

	// Returns the widest floor in sight
	private int GetWidestFloor(float angle, float distance)
	{
		float angleRad = Mathf.Deg2Rad * angle;

		return (int)(Mathf.Ceil(Mathf.Tan (angleRad) * distance));
	}
		
	private Vector3 GetRayRotation(int tileOffset, float distance)
	{
		Vector2 endPoint;

		// Horizontal direction
		if (direction.x != 0) {
			endPoint = new Vector2 (direction.x + distance * direction.x, direction.y + tileOffset);
		}
		// Vertical direction
		else {
			endPoint = new Vector2 (direction.x + tileOffset, direction.y + distance * direction.y);
		}

		float angle = Vector2.Angle (direction, endPoint);

		return Quaternion.AngleAxis (angle*Mathf.Sign(tileOffset), transform.forward) * direction;
	}

	// After the enemy moves, some floor tiles will be out of sight
	private void UpdateFloorsAfterLastMove()
	{
		RaycastHit2D[] hits;
		// The origin of the raycasts will be in the previous position (before last moving)
		Vector2 origin = GetRaycastOriginTransform(new Vector2 (transform.position.x - direction.x, transform.position.y - direction.y));

		float distance = visionDistance +1f;

		// The vision will be calculated like a triangle. 
		// From the vision distance and angle we can deduct the rest of the triangle variables
		int widestFloor = GetWidestFloor(fovAngle, distance);

		// We will raycast several lines from the eye of the enemy to the different triangle points (hereby called <endPoint>)
		for (int i = -widestFloor; i <= widestFloor; i++) {

			Vector3 rayRotation = GetRayRotation (i, distance);

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
	// Will now be reset (their layer mask will become "Floor" again)
	// Within the distance given by <distance>
	public void ResetLook(int distance)
	{
		RaycastHit2D[] hits;
		Vector2 origin = GetRaycastOriginTransform(transform.position);

		// The vision will be calculated like a triangle. 
		// From the vision distance and angle we can deduct the rest of the triangle variables
		int widestFloor = GetWidestFloor(fovAngle, distance);

		// We will raycast several lines from the eye of the enemy to the different triangle points (hereby called <endPoint>)
		for (int i = -widestFloor; i <= widestFloor; i++) {

			Vector3 rayRotation = GetRayRotation (i, distance);

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

	// The enemy will look at his surroundings
	// In practice, this will change the layer masks of all non-blocking objects within its vision to 'DangerFloor'
	private void Look()
	{		
		RaycastHit2D[] hits;
		Vector2 origin = GetRaycastOriginTransform(transform.position);

		// Firstly, we will remove the floor tiles that the enemy no longer sees (because of his movement)
		UpdateFloorsAfterLastMove();

		// The vision will be calculated like a triangle. 
		// From the vision distance and angle we can deduct the rest of the triangle variables
		int widestFloor = GetWidestFloor(fovAngle, visionDistance);

		// We will raycast several lines from the eye of the enemy to the different triangle points (hereby called <endPoint>)
		for (int i = -widestFloor; i <= widestFloor; i++) {

			Vector3 rayRotation = GetRayRotation (i, visionDistance);

			boxCollider.enabled = false;
			hits = Physics2D.RaycastAll (origin, rayRotation, visionDistance);
			boxCollider.enabled = true;

			// Check if there is a blocking object in sight
			if (BlockingObjectInVision (hits)) {
				// All floor tiles in the direction <rayRotation> will be reset to regular floor tiles
				UpdateFloorsAfterBlockingObject (rayRotation);

				// Now we'll mark the floor tiles as dangerous until the blocking object
				float distanceToBlockingObject = Vector2.Distance (origin, blockingObjectPosition);

				if (distanceToBlockingObject < 1) {
					return;
				}

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
		ResetLook (visionDistance);

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

	private void GenericPatrol()
	{
		RaycastHit2D hit;
		RaycastHit2D[] hits;

		horizontal = (int)direction.x;
		vertical = (int)direction.y;

		// Before trying to move, the enemy will check if there is a hole in front of him
		// If there is, he won't move there, and will change direction instead
		Vector2 nextPosition = new Vector2 (transform.position.x + direction.x, transform.position.y + direction.y);
		if (FindHole (nextPosition, out hits)) {
			ChangePatrolDirection ();
		} else if (!Move (horizontal, vertical, out hit)) {
			ChangePatrolDirection ();
		} else {
			StartSpriteMoveAnimation (horizontal, vertical);
		}
	}

	public void Patrol()
	{
		// Checking if the enemy has a specific patrol path
		// If not, he will walk in circles (generic patrol)
		if (hasPatrolPath) {
			StartCoroutine (MoveToPosition (patrolPath));
		}
		else {			
			GenericPatrol ();
		}
	}

}
