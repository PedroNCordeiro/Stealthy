using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour {

	private Rigidbody2D rb2D;
	private float inverseMoveTime;
	private IEnumerator SmoothMovementCoRoutine;
	private IEnumerator SmoothMovementBackCoRoutine;

	protected BoxCollider2D boxCollider;
	protected int horizontal = 0;
	protected int vertical = 0;
	public Vector2 direction; // Keeps the direction the moving object is facing

	public float moveTime;
	public LayerMask blockingLayer;
	[HideInInspector]
	public bool endedMove = true;


	protected virtual void Start () {
		boxCollider = GetComponent<BoxCollider2D> ();
		rb2D = GetComponent<Rigidbody2D> ();
		inverseMoveTime = 1f / moveTime;
	}

	protected virtual void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.layer == LayerMask.NameToLayer ("BlockingLayer")) {
			if (gameObject.tag != "Enemy") {
				StopCoroutine (SmoothMovementCoRoutine);
				StartCoroutine (SmoothMovementBackCoRoutine);
			}
		} 

		else if (other.gameObject.tag == "LaserFloor") {
			other.gameObject.tag = "DestructedFloor";
			Floor floor = other.gameObject.GetComponent<Floor> () as Floor;

			DestroyFloorByStepping (floor);
			Fall ();
		}

		else if (other.gameObject.tag == "DestructedFloor") {
			Fall ();
		}
	}

	private void DestroyFloorByStepping(Floor floor)
	{
		floor.DestructFloor ();
	}

	private void Fall()
	{
		Destroy (gameObject);
	}

	private IEnumerator SmoothMovementBack(Vector3 backPosition)
	{
		float sqrRemainingDistance = (transform.position - backPosition).sqrMagnitude;

		while (sqrRemainingDistance > float.Epsilon) {

			Vector3 newPosition = Vector3.MoveTowards (rb2D.position, backPosition, inverseMoveTime * Time.deltaTime);
			rb2D.MovePosition (newPosition);
			sqrRemainingDistance = (transform.position - backPosition).sqrMagnitude;
			yield return null;
		}
		endedMove = true;
	}

	protected virtual IEnumerator SmoothMovement(Vector3 end)
	{
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		while (sqrRemainingDistance > float.Epsilon) {

			Vector3 newPosition = Vector3.MoveTowards (rb2D.position, end, inverseMoveTime * Time.deltaTime);
			rb2D.MovePosition (newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;
			yield return null;
		}
		endedMove = true;
	}

	protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
	{
		endedMove = false;
		Vector2 start = transform.position;
		Vector2 end = start + new Vector2 (xDir, yDir);

		boxCollider.enabled = false;
		hit = Physics2D.Linecast (start, end, blockingLayer);
		boxCollider.enabled = true;

		if (hit.transform == null) {
			SmoothMovementCoRoutine =  SmoothMovement (end);
			SmoothMovementBackCoRoutine = SmoothMovementBack (start);
			StartCoroutine (SmoothMovementCoRoutine);
			return true;
		}
		endedMove = true;
		return false;
	}

	// Checks if the movement input given by <horizontal, vertical> differs from the previous input
	// i.e. Checks if the object changed direction
	protected bool ChangeInDirection(int horizontal, int vertical)
	{
		if (horizontal != direction.x || vertical != direction.y) {
			return true;
		}
		return false;
	}

	// Save the direction the object is facing
	protected void SaveDirection(int horizontal, int vertical)
	{
		direction = new Vector2 (horizontal, vertical);
	}
}
