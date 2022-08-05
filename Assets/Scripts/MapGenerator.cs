using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, UnityTerrain};
    public DrawMode drawMode;

    public bool autoUpdateMap;
    public bool isTest;
    public bool flipMapFromDefault;

    public MapTile_TerrainData mapTile_TerrainData;
    public MapTile_NoiseData mapTile_NoiseData;

    TextureGenerator textureGenerator = new TextureGenerator();

    private static string path;

    private static float[,] noiseMap;
    private static float[,] noiseMapForMesh;
    private static float[,] noiseToDisplay;

    void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            GenerateMap();
        }
    }

    // START MAP GEN //
    public void GenerateMap()
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        GameObject mapTile;

        // Application.dataPath + "/Save/" + filename
        path = "C:/Unity/Projects/World Gen - 2/Assets/HeightMaps/hm1.png";

        noiseMap = new float[mapTile_TerrainData.mapChunkSize, mapTile_TerrainData.mapChunkSize];
        noiseMapForMesh = new float[mapTile_TerrainData.mapChunkSize, mapTile_TerrainData.mapChunkSize];
        noiseToDisplay = new float[mapTile_TerrainData.mapChunkSize, mapTile_TerrainData.mapChunkSize];

        if (isTest)
        {
            noiseMap = MapTile_ReadHeightMapPNG.LoadTextureFromFile(true, path);
        }
        else
        {
            noiseMap = MapTile_NoiseMap.GenerateNoiseMap(mapTile_TerrainData.mapChunkSize,
                                                                    mapTile_NoiseData.noiseSeed,
                                                                    mapTile_NoiseData.noiseMapScale,
                                                                    mapTile_NoiseData.octaves,
                                                                    mapTile_NoiseData.persistence,
                                                                    mapTile_NoiseData.lacunarity,
                                                                    mapTile_NoiseData.offset);
        }

        noiseToDisplay = (flipMapFromDefault)? Utilities.RotateMatrix180(noiseMap) : noiseMap;
        noiseMapForMesh = Utilities.RotateMatrix180(noiseToDisplay);

        Color[] colorMap = textureGenerator.AssignTerrainColors(noiseToDisplay, mapTile_TerrainData);

        
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(textureGenerator.TextureFromHeightMap(noiseToDisplay));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(textureGenerator.TextureFromColorMap(colorMap, mapTile_TerrainData.mapChunkSize));
        }
        else if (drawMode == DrawMode.UnityTerrain)
        {
            mapTile = MapTile_CreateTerrain.CreateTerrain(noiseMapForMesh, mapTile_TerrainData.meshHeightMultiplier);
        }
    }


    void OnValidate()
    {
        if (mapTile_NoiseData != null)
        {
            mapTile_NoiseData.OnValuesUpdated -= OnValuesUpdated;
            mapTile_NoiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (mapTile_TerrainData != null)
        {
            mapTile_TerrainData.OnValuesUpdated -= OnValuesUpdated;
            mapTile_TerrainData.OnValuesUpdated += OnValuesUpdated;
        }
    }
}


[System.Serializable]
public struct TerrainType
{
    public string terrain_name;
    public float terrain_height;
    public Color terrain_color;
}