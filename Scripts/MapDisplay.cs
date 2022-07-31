using UnityEngine;


public class MapDisplay : MonoBehaviour {

    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawTexture(Texture2D texture)
    {
        textureRender.sharedMaterial.mainTexture = texture;
    }

    public void DrawMesh(MeshData meshMap, Texture2D texture)
    {
        meshFilter.sharedMesh = meshMap.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
