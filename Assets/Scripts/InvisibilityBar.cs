using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InvisibilityBar : MonoBehaviour {

	private Image foregroundImage;

	public float Value
	{
		get 
		{
			if(foregroundImage != null)
				return (int)(foregroundImage.fillAmount*100);	
			else
				return 0;	
		}
		set 
		{
			if(foregroundImage != null)
				foregroundImage.fillAmount = value/100f;	
		} 
	}

	void Start () {
		foregroundImage = gameObject.GetComponent<Image>();		
		Value = 100;
	}	

	void Update()
	{
		//Reduce fill amount over 30 seconds
		Value -= 2 * Time.deltaTime;

		if (Value == 50) {
			gameObject.SetActive (false);
		}
	}
}
