using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSpecifications : MonoBehaviour {

	// UI references
	private GameObject itemSlots;
	private GameObject slotKeys;
	private GameObject laserSlotImage;
	private GameObject switchSlotImage;

	public Vector2[] pathToLightSwitch;
	public Vector2[] pathToDutyPosition;
	public bool showTutorial;


	void Awake()
	{
		// UI references

		// Mobile devices
		#if !UNITY_STANDALONE && !UNITY_WEBPLAYER && !UNITY_EDITOR
		slotKeys = GameObject.Find ("SlotKeys");
		if (slotKeys != null) {
		slotKeys.SetActive(false);
		}
		#endif

		laserSlotImage = GameObject.Find("LaserSlotImage");
		if (laserSlotImage != null) {
			laserSlotImage.SetActive (false);
		}			
		switchSlotImage = GameObject.Find("SwitchSlotImage");
		if (switchSlotImage != null) {
			switchSlotImage.SetActive (false);
		}

	}
		
	public bool isSwitchSlotImageVisible()
	{
		return switchSlotImage.activeSelf;
	}
		
	public void ShowSwitchSlotImage (bool show)
	{
		if (switchSlotImage != null) {
			switchSlotImage.SetActive (show);
		}
	}

	public void ShowLaserSlotImage (bool show)
	{
		if (laserSlotImage != null) {
			laserSlotImage.SetActive (show);
		}
	}
}
