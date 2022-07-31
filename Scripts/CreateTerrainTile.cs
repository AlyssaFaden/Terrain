using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTerrainTile : MonoBehaviour
{

	/// <summary>
	/// Creates a terrain from heights.
	/// </summary>
	/// <param name="heightPercents">Terrain height percentages ranging from 0 to 1.</param>
	/// <param name="maxHeight">The maximum height of the terrain, corresponding to a height percentage of 1.</param>
	/// <param name="heightSampleDistance">The horizontal/vertical distance between height samples.</param>
	/// <param name="splatPrototypes">The textures used by the terrain.</param>
	/// <param name="alphaMap">Texture blending information.</param>
	/// <param name="position">The position of the terrain.</param>
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
	/// <param name="heightSampleDistance">The horizontal/vertical distance between height samples.</param>
	/// <param name="splatPrototypes">The textures used by the terrain.</param>
	/// <param name="alphaMap">Texture blending information.</param>
	/// <returns>A TerrainData instance.</returns>
	public static TerrainData CreateTerrainData(float[,] heightMap, float maxHeight)
	{

		// Create the TerrainData.
		var terrainData = new TerrainData();
		terrainData.heightmapResolution = heightMap.GetLength(0);

		var terrainWidth = (terrainData.heightmapResolution - 1);

		int heightwidth = heightMap.GetLength(0);
		terrainData.size = new Vector3(heightwidth, maxHeight, heightwidth);
		terrainData.SetHeights(0, 0, heightMap);

		return terrainData;
	}


	public static GameObject CreateTerrainFromTerrainData(TerrainData terrainData)
	{
		// Create the terrain game object.
		GameObject terrainObject = new GameObject("terrain");

		var terrain = terrainObject.AddComponent<Terrain>();
		terrain.terrainData = terrainData;

		terrainObject.AddComponent<TerrainCollider>().terrainData = terrainData;

		//terrainObject.transform.position = position;

		return terrainObject;
	}


}
