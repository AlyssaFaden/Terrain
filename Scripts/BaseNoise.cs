using UnityEngine;

public static class BaseNoise {


    public static float[,] GenerateNoiseMap(int heightwidth, int noiseSeed, float noiseScale, int octaves, float persistence, float lacunarity, Vector2 offset) {
        
        // SEED
        float[,] noiseMap = new float[heightwidth, heightwidth];

        System.Random prng = new System.Random(noiseSeed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }
        // SEED

        if (noiseScale <= 0)
            noiseScale = 0.0001f;

        // keeping track of our noise map heights, so if they go above 1 or below 0, we normalize them
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int x = 0; x < heightwidth; x++)
        {
            for (int y = 0; y < heightwidth; y++)
            {

                /*
                Looping through OCTAVES
                Each coherent-noise function that is part of a Perlin-noise function is called an octave. 
                These functions are called octaves because each function has, by default, double the frequency of the previous function;
                so here we are layering on noise effects one on top of the other
                */

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                float halfWidth = heightwidth / 2f;
                float halfHeight = heightwidth / 2f;

                for (int i = 0; i < octaves; i++)
                {
                    // adding FREQUENCY
                    float xCoord = (x - halfWidth) / noiseScale * frequency + octaveOffsets[i].x;
                    float yCoord = (y - halfHeight) / noiseScale * frequency + octaveOffsets[i].y;

                    // store the calculated perlin in a variable
                    // the *2-1 allows for the noise to be less than 1 sometimes
                    float perlinValue = Mathf.PerlinNoise(xCoord, yCoord) * 2 - 1;

                    // this was without octaves : noiseMap[x, y] = perlinValue;
                    noiseHeight += perlinValue * amplitude;

                    // Increasing the persistence produces "rougher" Perlin noise.
                    amplitude *= persistence; // range of 0 - 1
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int x = 0; x < heightwidth; x++) {
            for (int y = 0; y < heightwidth; y++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
    return noiseMap;
    }
}
