using UnityEngine;
using System.Collections;

public  class TextureGenerator
{

	public  Texture2D TextureFromColorMap(Color[] colorMap, int widthheight)
	{
		Texture2D texture = new Texture2D(widthheight, widthheight);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels(colorMap);
		texture.Apply();
		return texture;
	}


	public  Texture2D TextureFromHeightMap(float[,] heightMap)
	{
		int widthheight = heightMap.GetLength(0);
		int k = 0;
		Color[] colorMap = new Color[widthheight * widthheight];
		for (int x = 0; x < widthheight; x++)
		{
			for (int y = 0; y < widthheight; y++)
			{
				colorMap[k++] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
			}
		}

		return TextureFromColorMap(colorMap, widthheight);
	}

	// TERRAIN COLORS
	public  Color[] AssignTerrainColors(float[,] heightMap, MapTile_TerrainData mapTile)
	{
		int widthheight = heightMap.GetLength(0);

		Color[] colorMap = new Color[widthheight * widthheight];

		TerrainType[] mapTileRegions = mapTile.terrainRegions;

		for (int x = 0; x < widthheight; x++)
		{
			for (int y = 0; y < widthheight; y++)
			{
				float currentHeight = heightMap[x, y];
				for (int i = 0; i < mapTileRegions.Length; i++)
				{
					if (currentHeight <= mapTileRegions[i].terrain_height)
					{
						//if(currentHeight==0f) Debug.Log(mapTileRegions[i].terrain_name);
						colorMap[x * widthheight + y] = mapTileRegions[i].terrain_color;
						break;
					}
				}
			}
		}

		return colorMap;
	}

	public Texture2D GetTextureFromData(int tSize, float[,] data)
	{
		Texture2D result = new Texture2D(tSize, tSize, TextureFormat.RGBA32, false);
		result.filterMode = FilterMode.Trilinear;

		for (int x = 0; x < tSize; x++)
			for (int y = 0; y < tSize; y++)
			{
				Color col = new Color(data[x, y], data[x, y], data[x, y], 1.0f);
				result.SetPixel(y, x, col);
			}

		result.Apply();

		return result;
	}

}