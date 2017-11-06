using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour {
	private SpriteRenderer backgroundBar, healthBar;
	private Transform healthBarTransform;
	private ColourPalette palette;
	
	private void Awake()
	{
		healthBarTransform = transform.GetChild(0);

		backgroundBar = GetComponent<SpriteRenderer>();
		healthBar = healthBarTransform.GetComponent<SpriteRenderer>();

		palette = Resources.Load<ColourPalette>("Palette");

		SetHealth(1,1);
	}

	public void SetHealth(float health, float totalHealth)
	{
		//enable/disable the health bar
		if(health == totalHealth)
		{
			backgroundBar.enabled = false;
			healthBar.enabled = false;
			return;
		}
		else
		{
			backgroundBar.enabled = true;
			healthBar.enabled = true;
		}

		//size the health bar
		float amount = health/totalHealth;
		amount = Mathf.Clamp01(amount);
		healthBarTransform.localScale = new Vector3(amount, 1f, 1f);

		//colour the health bar
		if(amount >= .75f)
		{
			healthBar.color = palette.healthGreen;
		}
		else if(amount >= .5f)
		{
			healthBar.color = palette.healthOrange;
		}
		else
		{
			healthBar.color = palette.healthRed;
		}
	}
}
