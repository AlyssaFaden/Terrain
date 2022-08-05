using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class RawTerrainPng : MonoBehaviour
{
    [Tooltip("Flip y coordinates")]
    public bool flipY = false;
    [Tooltip("Windows byteorder[true], Mac[false]")]
    public bool windows = true;
    [Tooltip("Terrain data to export/import.")]
    public TerrainData terrainData;
    [Tooltip("PNG to use for conversion to RAW.")]
    public Texture2D InputPNG;
    [Tooltip("RAW file to conver to PNG (Assets folder)")]
    public string InputRAW;
    [Tooltip("Output PNG filename will be saved to (Assets folder)")]
    public string OutputPNG;
    [Tooltip("Output RAW filename will be saved to (Assets folder)")]
    public string OutputRAW;
    [Tooltip("Width/height to use for conversion [-1] to use terrain size.")]
    public int SizeConversion = -1;

    public void Init()
    {
        this.terrainData = this.GetComponent<TerrainCollider>().terrainData;
        this.OutputPNG = "TerrainPNG";
        this.OutputRAW = "TerrainRAW";
        this.SizeConversion = -1;
    }

    public void ImportPNG()
    {
        if (this.InputPNG == null) throw new Exception("No input PNG texture defined.");

        if (!this.InputPNG.isReadable)
            this.SetTextureImporterFormat(this.InputPNG, true);

        this.SetHeights(this.LoadTexture(this.InputPNG));
    }

    public void ExportPNG()
    {
        if (this.terrainData == null) throw new Exception("No terrain defined.");

        float[,] data = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        this.SaveTexture(this.OutputPNG, this.GetTextureFromData(terrainData.heightmapResolution, data));
        AssetDatabase.Refresh();
    }

    public void RawToPNG()
    {
        int size = this.SizeConversion == -1 ? terrainData.heightmapResolution : this.SizeConversion;
        float[,] data = ReadFile(Application.dataPath + "/" + this.InputRAW + (this.InputRAW.Substring(this.InputRAW.Length - 4, 4).ToLower() != ".raw" ? ".raw" : ""), size);
        this.SaveTexture(this.OutputPNG, this.GetTextureFromData(size, data));
        AssetDatabase.Refresh();
    }

    public void PNGtoRAW()
    {
        int size = this.SizeConversion == -1 ? terrainData.heightmapResolution : this.SizeConversion;
        float[,] data = this.LoadTexture(this.InputPNG);
        this.WriteFile(this.OutputRAW, data, size);
        AssetDatabase.Refresh();
    }

    public void SetTextureImporterFormat(Texture2D texture, bool isReadable)
    {
        if (null == texture) return;

        string assetPath = AssetDatabase.GetAssetPath(texture);
        var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (tImporter != null)
        {
            tImporter.textureType = TextureImporterType.Default;

            tImporter.isReadable = isReadable;

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();
        }
    }

    public float[,] LoadTexture(Texture2D tex)
    {
        float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        for (int x = 0; x < terrainData.heightmapResolution; x++)
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                heights[x, y] = tex.GetPixel(y, x).grayscale;//0.299*R+0.587*G+0.114*B
            }

        return heights;
    }

    public void SetHeights(float[,] data)
    {
        terrainData.SetHeights(0, 0, data);
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

    public void WriteFile(string filename, float[,] heights, int tSize)
    {
        byte[] data;

        data = new byte[tSize * tSize];

        for (int x = 0; x < tSize; ++x)
            for (int y = 0; y < tSize; ++y)
            {
                int index = x + y * tSize;
                int adj = flipY ? tSize - 1 - y : y;
                int height = Convert.ToInt32(heights[adj, x] * 256f);
                byte rawdHeight = Convert.ToByte(Clamp(height, 0, 255));
                data[index] = rawdHeight;
            }

        File.WriteAllBytes(Application.dataPath + "/" + filename + (filename.Substring(filename.Length - 4, 4).ToLower() != ".raw" ? ".raw" : ""), data);
    }

    public float[,] ReadFile(string filename, int tSize)
    {
        byte[] data;

        BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read));
        data = reader.ReadBytes(tSize * tSize);
        reader.Dispose();

        float[,] heights = new float[tSize, tSize];
        float one = 1.0f / 256f;

        for (int x = 0; x < tSize; ++x)
            for (int y = 0; y < tSize; ++y)
            {
                int ind = x + y * tSize;
                byte rawHeight = data[ind];
                float height = rawHeight * one;
                int adj = flipY ? tSize - 1 - y : y;
                heights[adj, x] = height;
            }
        return heights;
    }

    public void SaveTexture(string filename, Texture2D tex)
    {
        byte[] bytes = tex.EncodeToPNG();

        File.WriteAllBytes(Application.dataPath + "/" + filename + (filename.Substring(filename.Length - 4, 4).ToLower() != ".png" ? ".png" : ""), bytes);
    }

    public static object Clamp(int value, int min, int max)
    {
        return (value < min) ? min : (value > max) ? max : value;
    }
}

[CustomEditor(typeof(RawTerrainPng))]
public class RawFileEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RawTerrainPng myScript = (RawTerrainPng)target;
        if (GUILayout.Button("Initialize"))
            myScript.Init();

        if (GUILayout.Button("Export PNG"))
            myScript.ExportPNG();

        if (GUILayout.Button("Import PNG"))
            myScript.ImportPNG();

        if (GUILayout.Button("Convert PNG to RAW"))
            myScript.PNGtoRAW();

        if (GUILayout.Button("Convert RAW to PNG"))
            myScript.RawToPNG();
 
    }
}
