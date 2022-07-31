using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh, UnityTerrain};
    public DrawMode drawMode;

    public bool autoUpdateMap;

    const int mapChunkSize = 500;

    [Range(0,6)]
    public int levelOfDetail;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public int noiseSeed = 1;

    /*
    The word "lacunarity" literally refers to a gap or pool as derived from the word for "lake", 
    but in morphological analysis it has been variously defined as gappiness
    */
    public float lacunarity;

    /*
    Each coherent-noise function that is part of a Perlin-noise function is called an octave. 
    These functions are called octaves because each function has, by default, double the frequency of the previous function;
    so here we are layering on noise effects one on top of the other
    */
    public int octaves;
    [Range(0, 1)]
    /*
    Persistence. A multiplier that determines how quickly the amplitudes diminish for each successive octave in a Perlin-noise function. 
    The amplitude of each successive octave is equal to the product of the previous octave's amplitude and the persistence value. 
    Increasing the persistence produces "rougher" Perlin noise.
    */
    public float persistence;

    /* 
    we're doing this because the viewer defaults to zoomed in
    when generating our noise map in the perlin section, we will
    multiply by scale 
    */
    public float noiseMapScale = 20f;

    /*
    offsets allow us to alter the perlin noise map and give it variety
    */
    public Vector2 offset;

    // Terrain Regions
    public TerrainType[] terrainRegions;

    GameObject mapTile;

    // START MAP GEN //
    public void GenerateMap()
    {
        float[,] noiseMap = BaseNoise.GenerateNoiseMap(mapChunkSize, noiseSeed, noiseMapScale, octaves, persistence, lacunarity, offset);

        Color[] colorMap = AssignTerrainColors(noiseMap);

        //Mesh meshMap = MeshGenerator.GenerateMesh(noiseMap);




        // going to instantiate the map, but first get a reference to the mapDisplay
        MapDisplay display = FindObjectOfType<MapDisplay>();
        

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize));
        }
        else if (drawMode == DrawMode.UnityTerrain)
        {
            if(mapTile == null)
            {
                mapTile = CreateTerrainTile.CreateTerrain(noiseMap, meshHeightMultiplier);
            }
            else
            {

            }
            
        }
    }

    // TERRAIN COLORS
    public Color[] AssignTerrainColors(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Color[] colorMap = new Color[width * height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < terrainRegions.Length; i++)
                {
                    if (currentHeight <= terrainRegions[i].terrain_height)
                    {
                        colorMap [y * width + x] = terrainRegions[i].terrain_color;
                        break;
                    }
                }
            }
        }

        return colorMap;
    }

    void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
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