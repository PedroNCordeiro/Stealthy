using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	private Player player;
	private List <Enemy> enemies;
	private Light mainLight;

	public static GameController singleton;
	public GameObject boardManager;
	public bool lightSwitchInputReady = true;
	public float lightSwitchInputDelay;

	void Awake () {
		if (singleton == null) {
			singleton = this;
		} else if (singleton != this) {
			Destroy (gameObject);
		}

		DontDestroyOnLoad (gameObject);

		enemies = new List<Enemy> ();

		// Light switch references
		GameObject mainLightObject = GameObject.FindGameObjectWithTag ("MainLight");
		mainLight = mainLightObject.GetComponent<Light>() as Light;
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
		return player.isInvisible ();
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
					return true;
				}	
			}
		}

		return false;
	}

	// Finds the closest enemy to <endPosition> and returns its index in the enemies array
	private int FindClosestEnemy(Vector2 endPosition)
	{
		float min = float.MaxValue;
		int idx = 0;

		for (int i = 0; i < enemies.Count; i++) {
			float distance = Vector2.Distance (enemies [i].transform.position, endPosition);
			if (distance < min) {
				min = distance;
				idx = i;
			}
		}
	
		return idx;
	}

	// The path to the lightswitch
	// Each Vector2 represents the direction the enemy needs to move each time
	private Vector2[] PathToLightSwitch()
	{
		Vector2[] path = new Vector2[6];
		path [0] = new Vector2 (0, 1);
		path [1] = new Vector2 (-1, 0);
		path [2] = new Vector2 (-1, 0);
		path [3] = new Vector2 (-1, 0);
		path [4] = new Vector2 (-1, 0);
		path [5] = new Vector2 (-1, 0);  

		return path;
	}

	// The path to the lightswitch
	// Each Vector2 represents the direction the enemy needs to move each time
	private Vector2[] PathToDutyPosition()
	{
		Vector2[] path = new Vector2[6];

		path [0] = new Vector2 (1, 0);
		path [1] = new Vector2 (1, 0);
		path [2] = new Vector2 (1, 0);
		path [3] = new Vector2 (1, 0);
		path [4] = new Vector2 (1, 0);
		path [5] = new Vector2 (0, -1);

		return path;
	}

	// Activated if the player or an enemy pressed the light switch
	// If the lights are turned off, we will find the closest enemy to the light switch
	// And command it to go turn the lights on
	public IEnumerator SwitchLights()
	{
		GameObject lightSwitch = GameObject.FindGameObjectWithTag ("LightSwitch");
		Vector2 lightSwitchPosition = lightSwitch.transform.position;

		lightSwitchInputReady = false;

		mainLight.intensity = (mainLight.intensity == 1f) ? 0f : 1f;

		if (mainLight.intensity == 0f) {
			Vector2[] pathToLightSwitch = PathToLightSwitch ();
			Vector2[] pathToDutyPosition = PathToDutyPosition ();

			int enemyIndex = FindClosestEnemy (lightSwitchPosition);
			StartCoroutine (enemies [enemyIndex].MoveToLightSwitch (pathToLightSwitch));
			StartCoroutine (enemies [enemyIndex].MoveToDutyPosition (pathToDutyPosition));
		}

		yield return new WaitForSeconds (lightSwitchInputDelay);

		lightSwitchInputReady = true;
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
