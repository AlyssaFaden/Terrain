using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile_CreateTerrain : MonoBehaviour
{

	/// <summary>
	/// Creates a terrain from heights.
	/// </summary>
	/// <param name="heightPercents">Terrain height percentages ranging from 0 to 1.</param>
	/// <param name="maxHeight">The maximum height of the terrain, corresponding to a height percentage of 1.</param>
	/// <returns>A terrain GameObject.</returns>
	//public static GameObject CreateTerrain(float[,] heightPercents, float maxHeight, float heightSampleDistance, SplatPrototype[] splatPrototypes, float[,,] alphaMap, Vector3 position)
	public static GameObject CreateTerrain(float[,] heightMap, float maxHeight)
	{
		//var terrainData = CreateTerrainData(heightPercents, maxHeight, heightSampleDistance, splatPrototypes, alphaMap);
		var terrainData = CreateTerrainData(heightMap, maxHeight);

		return CreateTerrainFromTerrainData(terrainData);
	}


	/// <summary>
	/// Creates terrain data from heights.
	/// </summary>
	/// <param name="heightPercents">Terrain height percentages ranging from 0 to 1.</param>
	/// <param name="maxHeight">The maximum height of the terrain, corresponding to a height percentage of 1.</param>
	/// <returns>A TerrainData instance.</returns>
	public static TerrainData CreateTerrainData(float[,] heightMap, float maxHeight)
	{

		// Create the TerrainData.
		var terrainData = new TerrainData();

		terrainData.heightmapResolution = heightMap.GetLength(0);
		
		var terrainWidth = (terrainData.heightmapResolution - 1);
			
		terrainData.size = new Vector3(terrainWidth, maxHeight, terrainWidth);
		terrainData.SetHeights(0, 0, heightMap);

		return terrainData;
	}


	public static GameObject CreateTerrainFromTerrainData(TerrainData terrainData)
	{
		// Create the terrain game object.
		GameObject terrainObject = new GameObject("terrain"+ Random.Range(-10.0f, 10.0f));
		var terrain = terrainObject.AddComponent<Terrain>();

		terrain.terrainData = terrainData;
		terrainObject.AddComponent<TerrainCollider>().terrainData = terrainData;
		terrain.groupingID = 1 + (int)Random.Range(-10.0f, 10.0f);
		terrain.allowAutoConnect = true;
		terrain.drawHeightmap = true;
		terrain.heightmapPixelError = 5;
		terrain.basemapDistance = 1000;
		//terrain.shadowCastingMode = "TwoSided";
		//terrain.materialTemplate = "abc";
		terrain.drawTreesAndFoliage = true;
		
		Vector3 position = new Vector3(991, 0, 17);
		terrainObject.transform.position = position;

		return terrainObject;
	}

	public static float[,] FlipNoise(float[,] heightMap)
    {
		int dimensions = heightMap.GetLength(0);
		float[,] newHeightMap = new float[dimensions, dimensions];

		int rows = dimensions - 1;

		for (int y = 0; y < dimensions; y++)
		{
			for (int x = 0; x < dimensions; x++)
			{
				newHeightMap[x, y] = heightMap[x, rows - y];
			}
		}
				return newHeightMap;
    }


}
