using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute()]
public class MapTile_NoiseData : UpdateMapData
{

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

    protected override void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
        base.OnValidate();
    }
}

