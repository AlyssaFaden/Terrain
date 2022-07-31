using UnityEngine;
using System.Collections;

public static class TextureGenerator
{

	public static Texture2D TextureFromColorMap(Color[] colorMap, int heightwidth)
	{
		Texture2D texture = new Texture2D(heightwidth, heightwidth);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colorMap);
		texture.Apply();
		return texture;
	}


	public static Texture2D TextureFromHeightMap(float[,] heightMap)
	{
		int widthheight = heightMap.GetLength(0);
		Debug.Log(widthheight);

		Color[] colorMap = new Color[widthheight * widthheight];
		for (int y = 0; y < widthheight; y++)
		{
			for (int x = 0; x < widthheight; x++)
			{
				colorMap[y * widthheight + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
			}
		}

		return TextureFromColorMap(colorMap, widthheight);
	}

}