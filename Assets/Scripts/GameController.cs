using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController singleton;
	public GameObject boardManager;

	void Awake () {
		if (singleton == null) {
			singleton = this;
		} else if (singleton != this) {
			Destroy (gameObject);
		}

		DontDestroyOnLoad (gameObject);
		InitGame ();
	}

	public void InitGame()
	{
		if (BoardManager.singleton == null) {
			Instantiate(boardManager);
		}
	}
}
