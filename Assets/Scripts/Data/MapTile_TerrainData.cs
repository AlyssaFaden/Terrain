using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MapTile_TerrainData : UpdateMapData
{
    [Range(0, 6)]
    public int levelOfDetail;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public int mapChunkSize = 513;

    // Terrain Regions
    public TerrainType[] terrainRegions;

}
