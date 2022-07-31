using System.Collections;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateMesh(float[,] noise, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
    {

        // for loops will jump this amount instead of '1', thus drawing fewer triangles
        int lodIncrement;
        if (levelOfDetail == 0)
            lodIncrement = 1;
        else
            lodIncrement = levelOfDetail * 2;

        int xSize = noise.GetLength(0);
        int ySize = noise.GetLength(1);

        float halfWidth = (xSize) / 2f;
        float halfHeight = (ySize) / 2f;

        int verticesPerLine = xSize / lodIncrement;

        MeshData meshMap = new MeshData(verticesPerLine, verticesPerLine);

        float nodeHeight = 0;
        int prevX = 0;
        int prevY = 0;

        for (int i = 0, y = 0; y <= ySize; y+=lodIncrement)
        {
            for (int x = 0; x <= xSize; x+=lodIncrement)
            {
                if (x >= xSize && y >= ySize)
                {
                    prevY = (ySize - 1) - (y - 1);
                    prevX = (xSize - 1) - (x - 1);
                }
                else if (x >= xSize)
                {
                    prevX = (xSize - 1) - (x - 1);
                    prevY = (ySize - 1) - y;
                }
                else if (y >= ySize)
                {
                    prevY = (ySize - 1) - (y - 1);
                    prevX = (xSize - 1) - x;
                }
                else
                {
                    prevX = (xSize - 1) - x;
                    prevY = (ySize - 1) - y;
                }
                nodeHeight = heightCurve.Evaluate(noise[prevX, prevY]) * heightMultiplier;
                meshMap.vertices[i] = new Vector3(x - halfWidth, nodeHeight, y - halfHeight);
                meshMap.uvs[i] = new Vector2(1f - (float)x / xSize, 1f - (float)y / ySize);

                i++;
            }
        }
        
        for (int ti = 0, vi = 0, y = 0; y <ySize; y+=lodIncrement, vi++)
        {
            for (int x = 0; x <xSize; x+=lodIncrement, ti += 6, vi++)
            {
                meshMap.triangles[ti] = vi;
                meshMap.triangles[ti + 3] = meshMap.triangles[ti + 2] = vi + 1;
                meshMap.triangles[ti + 4] = meshMap.triangles[ti + 1] = vi + verticesPerLine + 1;
                meshMap.triangles[ti + 5] = vi + verticesPerLine + 2;
            }
        }
        
        return meshMap;
    }
}

 public class MeshData
 {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
         vertices = new Vector3[ (meshWidth+1) * (meshHeight+1) ];
         triangles = new int[ meshWidth * meshHeight * 6 ];
         uvs = new Vector2[(meshWidth + 1) * (meshHeight + 1)];
    }


    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        //mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

}