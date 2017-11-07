using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
	public Text[] damageTexts;
	public Canvas canvas;
	public Camera cam;

	public void ShowDamageText(int amount, Vector2 worldPos)
	{
		//Text newDamageText = damageTexts[0];
		//Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
		//newDamageText.rectTransform.anchoredPosition = new Vector2(screenPoint.x, screenPoint.y);
	}
}
