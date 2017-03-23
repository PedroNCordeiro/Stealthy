using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

	public static BoardManager singleton;
	public bool autoUpdate;
	public int boardWidth;
	public int boardHeight;

	public GameObject topLeftWall;
	public GameObject topRightWall;
	public GameObject bottomLeftWall;
	public GameObject bottomRightWall;
	public GameObject topWall;
	public GameObject bottomWall;
	public GameObject leftWall;
	public GameObject rightWall;
	public GameObject floor;
	public GameObject exit;
	public GameObject enemy;
	public GameObject crate;
	public GameObject potion;
	public GameObject laser;

	private Transform boardHolder;


	void Awake()
	{
		SetupBoard ();
	}

	public void SetupBoard()
	{
		boardHolder = new GameObject("Board").transform;

		for (int y = 0; y < boardHeight; y++) {
			for (int x = 0; x < boardWidth; x++) {
				GameObject toInstantiate = floor;
				if (x == 0) {
					toInstantiate = (y == 0) ? bottomLeftWall : (y == boardHeight -1) ? topLeftWall : leftWall;
				}
				else if (x == boardWidth -1) {
					toInstantiate = (y == 0) ? bottomRightWall : (y == boardHeight -1) ? topRightWall : rightWall;
				}
				else if (y == 0) {
					toInstantiate = bottomWall;
				}
				else if (y == boardHeight - 1) {
					toInstantiate = topWall;
				}

				GameObject instance = Instantiate (toInstantiate, new Vector3 (x, y), Quaternion.identity) as GameObject;
				instance.transform.SetParent (boardHolder);
			}
		}
		/*GameObject exitInstance = Instantiate (exit, new Vector3 (boardWidth - 2, boardHeight - 2), Quaternion.identity) as GameObject;
		exitInstance.transform.SetParent (boardHolder);*/

		GameObject crateInstance = Instantiate (crate, new Vector3 (2, 6), Quaternion.identity) as GameObject;
		crateInstance.transform.SetParent (boardHolder);

		GameObject potionInstance = Instantiate (potion, new Vector3 (2, 2), Quaternion.identity) as GameObject;
		potionInstance.transform.SetParent (boardHolder);

		/*GameObject laserInstance = Instantiate (laser, new Vector3 (1, 4), Quaternion.identity) as GameObject;
		laserInstance.transform.SetParent (boardHolder);*/

		GameObject enemyInstance = Instantiate (enemy, new Vector3 (3, 1), Quaternion.identity) as GameObject;
		enemyInstance.transform.SetParent (boardHolder);
	}
}
