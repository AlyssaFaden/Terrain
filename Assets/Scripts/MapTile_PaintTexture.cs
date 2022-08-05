using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile_PaintTexture : MonoBehaviour
{

    private void SetTerrainSplatMap(Terrain terrain, Texture2D[] textures)
    {
        //var terrainData = terrain.terrainData;

        //// The Splat map (Textures)
        //SplatPrototype[] splatPrototype = new SplatPrototype[terrainData.splatPrototypes.Length];
        //for (int i = 0; i < terrainData.splatPrototypes.Length; i++)
        //{
        //    splatPrototype[i] = new SplatPrototype();
        //    splatPrototype[i].texture = textures[i];    //Sets the texture
        //    splatPrototype[i].tileSize = new Vector2(terrainData.splatPrototypes[i].tileSize.x, terrainData.splatPrototypes[i].tileSize.y);    //Sets the size of the texture
        //    splatPrototype[i].tileOffset = new Vector2(terrainData.splatPrototypes[i].tileOffset.x, terrainData.splatPrototypes[i].tileOffset.y);    //Sets the size of the texture
        //}
        //terrainData.splatPrototypes = splatPrototype;

        float[,,] splatMapData = terrain.terrainData.GetAlphamaps(0, 0, 100, 100);
        for (int i = 26; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                splatMapData[i, j, 0] = 0;
                splatMapData[i, j, 1] = 0;
                splatMapData[i, j, 2] = 1;
            }
        }
        TerrainLayer[] layers = terrain.terrainData.terrainLayers;
        layers[2].tileSize = new Vector2(100, 100);
        terrain.terrainData.SetAlphamaps(0, 0, splatMapData);
        terrain.Flush();
    }
}
