using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

	public int visionDistance = 2;
	public LayerMask floorLayerMask;

	private BoxCollider2D boxCollider;

	private void ChangeFloorTags(Vector2 currentPosition)
	{
		RaycastHit2D hit;
		boxCollider.enabled = false;
		for (int i = 1; i <= visionDistance; i++) {
			Vector2 endPosition = new Vector2 (currentPosition.x, currentPosition.y - i);
			hit = Physics2D.Linecast (currentPosition, endPosition, floorLayerMask);
			hit.transform.gameObject.layer = LayerMask.NameToLayer ("DangerFloor");
		}
		boxCollider.enabled = true;
	}

	// Use this for initialization
	void Start () {
		boxCollider = GetComponent<BoxCollider2D> ();

		ChangeFloorTags (transform.position);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
