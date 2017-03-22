﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour {

	private Rigidbody2D rb2D;
	private float inverseMoveTime;

	protected BoxCollider2D boxCollider;
	protected int horizontal = 0;
	protected int vertical = 0;
	protected IEnumerator SmoothMovementCoRoutine;
	protected IEnumerator SmoothMovementBackCoRoutine;

	public float moveTime = 0.1f;
	public LayerMask blockingLayer;
	[HideInInspector]
	public bool endedMove = true;

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

	public bool Move (int xDir, int yDir, out RaycastHit2D hit)
	{
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
		return false;
	}

	// Use this for initialization
	protected virtual void Start () {
		boxCollider = GetComponent<BoxCollider2D> ();
		rb2D = GetComponent<Rigidbody2D> ();
		inverseMoveTime = 1f / moveTime;
	}
}
