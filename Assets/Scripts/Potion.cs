using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour {

	private GameObject consumer;
	private SpriteRenderer spriteRenderer;

	public float duration;

	void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer> ();
	}

	public void StartEffect (GameObject drinker)
	{
		consumer = drinker;
		consumer.GetComponent<Player> ().SetInvisibility (true);

		spriteRenderer.enabled = false; // Hide the potion when consumed

		consumer.GetComponent<SpriteRenderer> ().color = Color.black;

		StartCoroutine (RemainEffect (duration));
	}

	// Make the player invisible for a given duration
	private IEnumerator RemainEffect(float duration)
	{
		float timeLeft = duration;
		while (timeLeft > float.Epsilon) {
			timeLeft -= Time.deltaTime;
			yield return null;
		}

		EndEffect ();
	}

	private void EndEffect()
	{
		consumer.GetComponent<SpriteRenderer> ().color = Color.white;
		consumer.GetComponent<Player> ().SetInvisibility (false);

		Destroy (gameObject);
	}

}
