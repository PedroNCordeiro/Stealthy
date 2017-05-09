using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour {

	private GameObject player;
	private Player playerScript;
	private SpriteRenderer spriteRenderer;
	private SpriteRenderer playerSpriteRenderer;
	private float inverseDuration;

	public float duration;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer> ();
		player = GameObject.FindGameObjectWithTag ("Player");
		playerScript = player.GetComponent<Player> () as Player;
		playerSpriteRenderer = player.GetComponent<SpriteRenderer> ();

		inverseDuration = 1.0f / duration;
	}

	public void StartEffect ()
	{
		playerScript.SetInvisibility (true);
		playerScript.ShowInvisibilityBar ();

		spriteRenderer.enabled = false; // Hide the potion when consumed
		playerSpriteRenderer.color = Color.black;

		StartCoroutine (RemainEffect (duration));
	}

	// Make the player invisible for a given duration
	private IEnumerator RemainEffect(float duration)
	{
		float timeLeft = duration;
		float delta;
		while (timeLeft > float.Epsilon) {
			delta = Time.deltaTime;
			timeLeft -= delta;

			playerScript.reduceInvisibilityBar (inverseDuration * delta);
			yield return null;
		}

		EndEffect ();
	}

	private void EndEffect()
	{
		playerSpriteRenderer.color = Color.white;
		playerScript.SetInvisibility (false);

		playerScript.HideInvisibilityBar ();

		Destroy (gameObject);
	}

}
