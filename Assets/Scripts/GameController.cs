using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	private Player player;
	private List <Enemy> enemies;

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

		enemies = new List<Enemy> ();
	}

	void Update()
	{
		MoveEnemies ();
	}

	public void AddEnemyToList(Enemy script)
	{
		enemies.Add (script);
	}

	private void RemoveEnemyFromList(Enemy script)
	{
		enemies.Remove (script);
	}

	public void AddPlayer(Player script)
	{
		this.player = script;
	}

	public bool IsPlayerInvisible()
	{
		return player.isInvisible;
	}

	private void MoveEnemies()
	{
		for (int i = 0; i < enemies.Count; i++) {
			if (enemies[i].canMove && enemies [i].endedMove) {
				enemies [i].Patrol ();
			}
		}
	}

	// Checks if the floor in position <floorPosition> is
	// seen by an enemy other than <askingEnemy>
	public bool IsFloorSeenByAnotherEnemy(Vector2 floorPosition, Enemy askingEnemy)
	{
		for (int i = 0; i < enemies.Count; i++) {
			if (enemies [i] == askingEnemy) {
				continue;
			}
			for (int j = 0; j < enemies[i].dangerousFloorPositions.Count; j++) {
				if (enemies[i].dangerousFloorPositions[j].x == floorPosition.x && enemies[i].dangerousFloorPositions[j].y == floorPosition.y) {
					Debug.Log ("FloorSeenByAnotherEnemy is TRUE");
					return true;
				}	
			}
		}

		return false;
	}

	// Advances the game for the next level
	public void NextLevel()
	{
		SceneManager.LoadScene (1);
	}

	public void GameOver()
	{
		player.Die ();
	}
}
