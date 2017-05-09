using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InvisibilityBar : MonoBehaviour {

	private Image foregroundImage;

	private float fillAmount
	{
		get {
			if (foregroundImage != null) {
				return foregroundImage.fillAmount;
			} else {
				return 0f;
			}
		}

		set {
			if (foregroundImage != null) {
				foregroundImage.fillAmount = value;
			}
		}
	}

	void Start () {
		foregroundImage = gameObject.GetComponent<Image>();	
		fillAmount = 1f;
	}	
		
	public void ShowInvisibilityBar()
	{
		foregroundImage.enabled = true;
	}

	public void HideInvisibilityBar()
	{
		foregroundImage.enabled = false;
	}

	public IEnumerator StartProgress (float potionDuration)
	{
		foregroundImage.enabled = true;
		float inversePotionDuration = 1.0f / potionDuration;

		while (fillAmount > float.Epsilon) {
			
			// Reduce fill amount over <potionDuration> seconds
			fillAmount -= inversePotionDuration * Time.deltaTime;
			yield return null;
		}

		foregroundImage.enabled = false;
	}

	public void ReduceBar (float reduceAmount)
	{
		fillAmount -= reduceAmount;
	}

}
