using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(BoardManager))]
public class BoardGenerator : Editor {

	public override void OnInspectorGUI() {
		BoardManager boardManager = (BoardManager)target;
		 
		DrawDefaultInspector ();

		if (GUILayout.Button ("Generate")) {
			boardManager.SetupBoard ();
		}
	}
}
