using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotListener : MonoBehaviour {

	public void OnLaserSlotPointerUp ()
	{
		GameController.singleton.OnLaserSlotClick ();
	}

}
