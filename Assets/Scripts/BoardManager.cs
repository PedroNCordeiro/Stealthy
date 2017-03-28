using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {

	public static BoardManager singleton;
	public bool autoUpdate;
	public int boardWidth;
	public int boardHeight;

	public int enemyCount;

	public GameObject topLeftWall;
	public GameObject topRightWall;
	public GameObject bottomLeftWall;
	public GameObject bottomRightWall;
	public GameObject topWall;
	public GameObject bottomWall;
	public GameObject leftWall;
	public GameObject rightWall;
	public GameObject floor;

	public boardObjectInfo[] objectsInfo;

	private Transform boardHolder;


	void Awake()
	{
		SetupBoard ();
	}

	public void SetupBoard()
	{
		boardHolder = new GameObject("Board").transform;

		// Setup floors and walls
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

		// Setup other objects
		for (int i = 0; i < objectsInfo.Length; i++) {
			GameObject instance = Instantiate (objectsInfo[i].boardObject, objectsInfo[i].objectPosition, Quaternion.identity) as GameObject;
			instance.transform.SetParent (boardHolder);
		}
			
	}


	[System.Serializable]
	public struct boardObjectInfo {
		public GameObject boardObject;
		public Vector2 objectPosition;
	}

}
