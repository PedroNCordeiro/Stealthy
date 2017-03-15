using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(BoardManager))]
public class BoardGenerator : Editor {

	public override void OnInspectorGUI() {
		BoardManager boardManager = (BoardManager)target;

		if (DrawDefaultInspector ()) {
			if (boardManager.autoUpdate) {
				boardManager.SetupBoard ();
			}
		}

		if (GUILayout.Button ("Generate")) {
			boardManager.SetupBoard ();
		}
	}
}
