using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class adds functionalities to the UI events
public class SlotListener : MonoBehaviour {

	public void OnLaserSlotPointerUp ()
	{
		GameController.singleton.OnLaserSlotClick ();
	}

	public void OnSwitchSlotPointerUp ()
	{
		GameController.singleton.OnSwitchSlotClick ();
	}
}
