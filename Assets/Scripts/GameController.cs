using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

	private Player player;
	private List <Enemy> enemies;
	private Light mainLight;
	private LevelSpecifications levelSpecs;
	private int level;
	private GameObject levelTutorialCanvas;
	private static bool firstTimeInLevel = true;
	private GameObject laserSlotImage;
	private GameObject potionSlotImage;

	[HideInInspector]
	public bool onTutorial = false;
	public static GameController singleton;
	public bool lightSwitchInputReady = true;
	public float lightSwitchInputDelay;

	void Awake () {
		if (singleton == null) {
			singleton = this;
		} else if (singleton != this) {
			Destroy (gameObject);
		}

		DontDestroyOnLoad (gameObject);

		FirstSetup ();
	}
		
	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
		
		if (!onTutorial) {
			MoveEnemies ();
		}
	}

	private void FirstSetup()
	{
		enemies = new List<Enemy> ();

		// Light switch references
		GameObject mainLightObject = GameObject.FindGameObjectWithTag ("MainLight");
		if (mainLightObject != null) {
			mainLight = mainLightObject.GetComponent<Light>() as Light;
		}

		// Item Slots references
		laserSlotImage = GameObject.Find("LaserSlotImage");
		if (laserSlotImage != null) {
			laserSlotImage.SetActive (false);
		}			
		potionSlotImage = GameObject.Find("PotionSlotImage");
		if (potionSlotImage != null) {
			potionSlotImage.SetActive (false);
		}
			
		StartCoroutine (SetupLevel ());
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
		
	// Show / Hide the laser slot image
	public void ShowLaserSlotImage(bool show)
	{
		if (laserSlotImage != null) {
			laserSlotImage.SetActive (show);
		}
	}

	// Show / Hide the potion slot image
	public void ShowPotionSlotImage(bool show)
	{
		if (potionSlotImage != null) {
			potionSlotImage.SetActive (show);
		}
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

			if (enemies [i].dangerousFloorPositions.ContainsKey (floorPosition)) {
				return true;
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
		
	private void ChangeEnemiesVision(int newVisionDistance)
	{
		for (int i = 0; i < enemies.Count; i++) {
			if (newVisionDistance == -1) {
				newVisionDistance = enemies [i].maxVisionDistance;
			}
			enemies [i].ChangeVisionDistance (newVisionDistance);
		}
	}

	// Activated if the player or an enemy pressed the light switch
	// If the lights are turned off, we will find the closest enemy to the light switch
	// And command it to go turn the lights on
	public IEnumerator SwitchLights()
	{
		GameObject lightSwitch = GameObject.FindGameObjectWithTag ("LightSwitch");
		Vector2 lightSwitchPosition = lightSwitch.transform.position;

		lightSwitchInputReady = false;

		if (mainLight.intensity == 1f) {
			mainLight.intensity = 0f;
			ChangeEnemiesVision (1);
		} else {
			mainLight.intensity = 1f;
			ChangeEnemiesVision (-1); // Passing the value -1 means restoring each enemy's max vision distance
		}

		if (mainLight.intensity == 0f) {
			int enemyIndex = FindClosestEnemy (lightSwitchPosition);

			yield return StartCoroutine (enemies [enemyIndex].MoveToLightSwitch (levelSpecs.pathToLightSwitch));
			yield return StartCoroutine (enemies [enemyIndex].MoveToDutyPosition (levelSpecs.pathToDutyPosition));
		}

		yield return new WaitForSeconds (lightSwitchInputDelay);

		lightSwitchInputReady = true;
	}

	// Display the level tutorial
	private IEnumerator SetupTutorial()
	{
		onTutorial = true;

		while (true) {
			if (Input.GetKeyUp (KeyCode.Space)) {
				break;
			}
			yield return null;
		}

		levelTutorialCanvas.SetActive (false);

		onTutorial = false;

		yield return null;
	}
		
	private IEnumerator SetupLevel()
	{
		yield return null;

		// Level Specifications
		GameObject levelSpecsObject = GameObject.FindGameObjectWithTag("LevelSpecifications");
		if (levelSpecsObject != null) {
			levelSpecs = levelSpecsObject.GetComponent<LevelSpecifications> () as LevelSpecifications;
		}

		levelTutorialCanvas = GameObject.Find ("LevelMessageCanvas");
		if (levelTutorialCanvas != null) {
			if (levelSpecs.showTutorial && firstTimeInLevel) {
				StartCoroutine (SetupTutorial ());
			} else {
				levelTutorialCanvas.SetActive (false);
			}
		}

		// Restore light, in case we left a dark room in the previous level
		mainLight.intensity = 1f;
	}

	// Advances the game to the next level
	public void FinishLevel()
	{	
		firstTimeInLevel = true;
		
		enemies.Clear ();

		level = SceneManager.GetActiveScene().buildIndex + 1;
		SceneManager.LoadScene (level);

		StartCoroutine (SetupLevel ());
	}
		
	public void GameOver()
	{
		firstTimeInLevel = false;

		enemies.Clear ();

		SceneManager.LoadScene(SceneManager.GetActiveScene().name);

		StartCoroutine (SetupLevel ());
	}
}
