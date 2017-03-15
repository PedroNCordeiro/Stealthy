using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	public static GameController singleton;
	public BoardManager boardManager;

	void Awake () {
		if (singleton == null) {
			singleton = this;
		} else if (singleton != this) {
			Destroy (gameObject);
		}

		DontDestroyOnLoad (gameObject);
		InitGame ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void InitGame()
	{
		boardManager.SetupBoard ();
	}
}
