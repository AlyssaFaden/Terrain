using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MapTile_ReadHeightMapPNG : MonoBehaviour
{

    public static float[,] LoadTextureFromFile(bool flipArray, string path)
    {

        Texture2D loadedTexture = LoadImageFromDrive(path);

        float[,] heights = ConvertImageToArray(loadedTexture);

        if (flipArray) { heights = Utilities.RotateMatrix180(heights); }

        return heights;
    }

    public static Texture2D LoadImageFromDrive(string path)
    {
        byte[] byteArray = File.ReadAllBytes(path);
        //create a texture and load byte array to it
        // Texture size does not matter 
        Texture2D sampleTexture = new Texture2D(2, 2);

        // the size of the texture will be replaced by image size
        bool isLoaded = sampleTexture.LoadImage(byteArray);

        // apply this texure as per requirement on image or material
        //GameObject image = GameObject.Find("RawImage");
        //if (isLoaded)
        //{
        //    image.GetComponent<RawImage>().texture = sampleTexture;
        //}

        return sampleTexture;
    }


    public static float[,] ConvertImageToArray(Texture2D tex)
    {
        //bool flipY = true;
        float[,] heights = new float[513, 513];
        for (int x = 0; x < 513; x++)
            for (int y = 0; y < 513; y++)
            {
                //int adj = flipY ? 513 - 1 - y : y;
                heights[x, y] = tex.GetPixel(y, x).grayscale;//0.299*R+0.587*G+0.114*B
            }

        return heights;
    }
}
