using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	private Enemy enemy;
	private Player player;


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

	public void AddEnemy(Enemy script)
	{
		this.enemy = script;
	}

	public void AddPlayer(Player script)
	{
		this.player = script;
	}

	public void GameOver()
	{
		player.Die ();
	}

	void Update()
	{
		if (enemy.endedMove) {
			enemy.endedMove = false;
			enemy.Patrol ();
		}
	}
}
