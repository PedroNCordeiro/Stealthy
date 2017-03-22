using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	private Enemy enemy;
	private Player player;


	public static GameController singleton;
	public GameObject boardManager;
	public bool moveEnemy = true;

	void Awake () {
		if (singleton == null) {
			singleton = this;
		} else if (singleton != this) {
			Destroy (gameObject);
		}

		DontDestroyOnLoad (gameObject);
		InitGame ();
	}

	void Update()
	{
		if (enemy.endedMove && moveEnemy) {
			enemy.Patrol ();
		} else {
			StartCoroutine (enemy.Look (-1, 0));
		}
	}

	public void InitGame()
	{
		if (BoardManager.singleton == null) {
			Instantiate(boardManager);
		}
	}

	public void AddEnemy(Enemy script)
	{
		this.enemy = script;
	}

	public void AddPlayer(Player script)
	{
		this.player = script;
	}

	public bool isPlayerInvisible()
	{
		return player.isInvisible;
	}

	public void GameOver()
	{
		player.Die ();
	}
}
