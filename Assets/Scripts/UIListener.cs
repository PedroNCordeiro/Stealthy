using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class adds functionalities to the UI events
public class UIListener : MonoBehaviour {

	public void OnLaserSlotPointerUp ()
	{
		GameController.singleton.OnLaserSlotClick ();
	}

	public void OnSwitchSlotPointerUp ()
	{
		GameController.singleton.OnSwitchSlotClick ();
	}
		
	// When the user triggers the PointerDown event
	// The input parameter specifies which arrow was triggered
	public void OnArrowPointerDown (int arrow)
	{
		GameController.singleton.OnArrowPointerDown (arrow);
	}

	public void OnArrowPointerUp (int arrow)
	{
		GameController.singleton.OnArrowPointerUp (arrow);
	}

	public void Quit()
	{
		Application.Quit ();
	}
}
