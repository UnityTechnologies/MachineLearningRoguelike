using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Roguelike/Colour Palette", fileName = "ColourPalette",  order = 1)]
public class ColourPalette : ScriptableObject
{
	public Color healthGreen, healthOrange, healthRed;
}
