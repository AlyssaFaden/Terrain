using UnityEngine;
using System;
using UnityEngine.Profiling;
#if UNITY_EDITOR
using UnityEditor;
#endif

//*URP&HDRP Terrain Shader
//*Marco Amadei (Corvostudio di Amadei Marco) 
//*Software is protected by EU licensing

namespace Corvostudio.CorvoTerrainShader
{
    [ExecuteInEditMode]
    public sealed class CorvoTerrainManager : MonoBehaviour
    {
        [Header("Standard terrain settings (for auto-restore)")]
        public Material standardTerrainMaterial;

        [Header("Corvo terrain settings")]
        public Material corvoTerrainMaterial;
        [Range(0, 10)]
        public float terrainUVScale = 1;
        private float _prevTerrainUVScale = 20;
        public MaxTextureSize maxTextureSize = MaxTextureSize.res2048x2048;
        private MaxTextureSize _previousMaxTextureSize = MaxTextureSize.res2048x2048;
        public bool useHeightmap = false;


        [Range(0, 10)]
        private float editorAutoRefreshTime = 2;


        [NonSerialized]
        public Texture2DArray[] baked_diffuses = new Texture2DArray[0];//questi array sono tuttin della stessa lunghezza
        [NonSerialized]
        public Texture2DArray[] baked_normals = new Texture2DArray[0];
        [NonSerialized]
        public Texture2D[] baked_heightmaps = new Texture2D[0];

        private Terrain _terrain;


        void Awake()
        {
            if (IsCorvoTerrain())
                SetCorvoTerrain();
            else
                SetStandardTerrain();
        }

#if UNITY_EDITOR
        float nextUpdateTime = 0;
        void Update()
        {
            if (!Application.isPlaying && IsCorvoTerrain())
            {
                if (_prevTerrainUVScale != terrainUVScale)
                {
                    _prevTerrainUVScale = terrainUVScale;
                    Apply();
                }
                else if (editorAutoRefreshTime != 0 && Time.time > nextUpdateTime)
                {
                    nextUpdateTime = Time.time + editorAutoRefreshTime;
                    Apply();
                }
            }
        }

        [InitializeOnLoadMethod]
        private static void OnInitialized()
        {
            CorvoTerrainManager[] corvoTerrainManagers = GameObject.FindObjectsOfType<CorvoTerrainManager>();
            foreach (CorvoTerrainManager ctm in corvoTerrainManagers)
            {
                if (ctm.IsCorvoTerrain())
                {
                    if (ctm.TextureArraysChanged())
                        ctm.GenerateTextureArrays(true);
                    else
                        ctm.Apply();
                }
            }
        }
#endif

        /// <summary>
        /// Return true if currently the standard terrain material is in use
        /// </summary>
        public bool IsStandardTerrain()
        {
            return GetTerrain().materialTemplate != null && GetTerrain().materialTemplate == standardTerrainMaterial;
        }
        /// <summary>
        /// Set current terrain material as standard terrain material
        /// </summary>
        /// <param name="clearTextureBuffer">if true and on play mode, generated textures for corvoterrain material will be removed from the RAM</param>
        public void SetStandardTerrain(bool clearTextureBuffer = true)
        {
            if (standardTerrainMaterial)
                GetTerrain().materialTemplate = standardTerrainMaterial;

            if (Application.isPlaying && clearTextureBuffer)//ottimizzazioni: Onplaymode deletes unusued textures when clearTextureBuffer is true
            {
                baked_diffuses = new Texture2DArray[0];
                baked_normals = new Texture2DArray[0];
                baked_heightmaps = new Texture2D[0];
            }
        }

        /// <summary>
        /// Return true if currently the Corvo Terrain material is in use
        /// </summary>
        public bool IsCorvoTerrain()
        {
            return GetTerrain().materialTemplate != null && GetTerrain().materialTemplate == corvoTerrainMaterial;
        }
        /// <summary>
        /// Set Corvo terrain material on current terrain. Also generates and applies textures, if not already in RAM
        /// </summary>
        public void SetCorvoTerrain()
        {
            if (corvoTerrainMaterial)
            {
                GetTerrain().materialTemplate = corvoTerrainMaterial;

                if (TextureArraysChanged())
                    GenerateTextureArrays(true);
                else
                    Apply();
            }
        }

        /// <summary>
        /// Get current linked terrain
        /// </summary>
        public Terrain GetTerrain()
        {
            if (!_terrain)
                _terrain = GetComponent<Terrain>();
            return _terrain;
        }


        int GetArrayLenghtBasedonLayers(Terrain terrain)
        {
            return Mathf.Clamp((terrain.terrainData.terrainLayers.Length + 3) / 4, 2, 4);
        }

        /// <summary>
        /// Return true if textures in RAM doesn't match the required textures for Corvo terrain material. In this case you need to call GenerateTexturesArray to update the material.
        /// </summary>
        public bool TextureArraysChanged()
        {
            if (baked_diffuses.Length != GetArrayLenghtBasedonLayers(GetTerrain()) || _previousMaxTextureSize != maxTextureSize)
                return true;
            return false;
        }

        private Vector2 GetTextureSize()
        {
            Vector2 sizes = new Vector2(0, 0);
            _previousMaxTextureSize = maxTextureSize;
            foreach (TerrainLayer tex in GetTerrain().terrainData.terrainLayers)
            {
                if (tex.diffuseTexture.width > sizes.x)
                    sizes = new Vector2(tex.diffuseTexture.width, sizes.y);
                if (tex.diffuseTexture.width > sizes.y)
                    sizes = new Vector2(sizes.x, tex.diffuseTexture.height);
            }
            int maxSize = (int)Mathf.Max(sizes.x, sizes.y);
            if (maxSize > (int)maxTextureSize)
            {
                float reproportionValue = (float)maxTextureSize / maxSize;
                sizes = new Vector2((int)(sizes.x * reproportionValue), (int)(sizes.y * reproportionValue));
                //Debug.Log("Sizes exceeded. New sizes: " + sizes);
            }
            return sizes;
        }

        /// <summary>
        /// Generate textures in RAM for the Corvo terrain material.
        /// </summary>
        /// <param name="alsoApply">Also apply the generated textures. If false, you will need to call Apply() yourself.</param>
        /// <returns>Memory (RAM) used (only in editor)</returns>
        public long GenerateTextureArrays(bool alsoApply = false)
        {
            //get selected texture size
            Vector2 sizes = GetTextureSize();

            //aply main texture UVs
            int array_lenght = GetArrayLenghtBasedonLayers(GetTerrain());
            baked_diffuses = new Texture2DArray[array_lenght];
            baked_normals = new Texture2DArray[array_lenght];
            baked_heightmaps = new Texture2D[useHeightmap ? array_lenght : 0];

            int maxAvailableTextures = baked_diffuses.Length * 4;//4 layers per pass
            int current_id = 0;//id rispetto a applied_textures delle albedo
            for (int i = 0; i < /*terrain.terrainData.terrainLayers.Length*/maxAvailableTextures; i++)
            {
                if (i % 4 == 0)//nuovo giro di 4-set
                {
                    baked_diffuses[current_id] = new Texture2DArray((int)sizes.x, (int)sizes.y, 4, TextureFormat.RGBA32, true, false);
                    baked_diffuses[current_id].filterMode = FilterMode.Bilinear;
                    baked_diffuses[current_id].wrapMode = TextureWrapMode.Repeat;
                    baked_normals[current_id] = new Texture2DArray((int)sizes.x, (int)sizes.y, 4, TextureFormat.RGBA32, true, true);
                    baked_normals[current_id].filterMode = FilterMode.Bilinear;
                    baked_normals[current_id].wrapMode = TextureWrapMode.Repeat;
                    if (useHeightmap)
                    {
                        TerrainLayer[] ls = GetTerrain().terrainData.terrainLayers;
                        baked_heightmaps[current_id] = Get4ChannelHeightmap((int)sizes.x, (int)sizes.y, ls[current_id].maskMapTexture, ls[current_id + 1].maskMapTexture, ls[current_id + 2].maskMapTexture, ls[current_id + 3].maskMapTexture);
                        baked_heightmaps[current_id].filterMode = FilterMode.Bilinear;
                        baked_heightmaps[current_id].wrapMode = TextureWrapMode.Repeat;
                    }
                }

                if (i < GetTerrain().terrainData.terrainLayers.Length)
                {
                    //Set albedo textures array
                    CopyTextureInArray(GetTerrain().terrainData.terrainLayers[i].diffuseTexture, ref baked_diffuses[current_id], i % 4, sizes);
                    //set normalmaps array
                    CopyTextureInArray(GetTerrain().terrainData.terrainLayers[i].normalMapTexture, ref baked_normals[current_id], i % 4, sizes, true);
                    //set heightmap texture channels
                    //if (useHeightmap)
                    //CopyChannelInChannel(GetTerrain().terrainData.terrainLayers[0].maskMapTexture, ref baked_heightmaps[current_id], i % 4);
                }

                //at the end of every 4-set, send the array to the material
                if ((i + 1) % 4 == 0)//last of a 4-set
                {
                    baked_diffuses[current_id].Apply();
                    baked_normals[current_id].Apply();
                    if (useHeightmap)
                        baked_heightmaps[current_id].Apply();

                    current_id++;
                }
            }

            if (alsoApply)
                Apply();


            long totalSize = 0;
#if UNITY_EDITOR
            foreach (Texture2DArray t2da in baked_diffuses)
                if (t2da != null)
                    totalSize += Profiler.GetRuntimeMemorySizeLong(t2da);
            foreach (Texture2DArray t2da in baked_normals)
                if (t2da != null)
                    totalSize += Profiler.GetRuntimeMemorySizeLong(t2da);
            foreach (Texture2D t2d in baked_heightmaps)
                if (t2d != null)
                    totalSize += Profiler.GetRuntimeMemorySizeLong(t2d);
#endif
            return totalSize;
        }

        private void CopyTextureInArray(Texture2D sourcesTexture, ref Texture2DArray targetTextureArray, int arrayIndex, Vector2 targetResolution, bool isNormal = false)
        {
            targetTextureArray.SetPixels(
                    GetTextureResized(sourcesTexture, (int)targetResolution.x, (int)targetResolution.y, isNormal).GetPixels(0),
                    arrayIndex % 4,
                    0
                );
        }
        private Texture2D GetTextureResized(Texture2D texture, int width, int height, bool isNormal)
        {
            if (texture.isReadable && IsSameResolution(texture, width, height))//texture is readable and sizes are already ok
                return texture;
            else
                return Resize(texture, width, height, isNormal);
        }
        Texture2D Resize(Texture texture, int targetX, int targetY, bool isNormal)
        {
            RenderTexture previous = RenderTexture.active;

            RenderTexture tmp = RenderTexture.GetTemporary(targetX, targetY, 0, RenderTextureFormat.Default, isNormal ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.sRGB);
            Graphics.Blit(texture, tmp);
            RenderTexture.active = tmp;
            Texture2D result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            return result;
        }
        private bool IsSameResolution(Texture2D texture, int width, int height)
        {
            return width == texture.width && height == texture.height;
        }
        private Texture2D CreateTextureOfColor(int width, int height, Color color, bool isNormal = false)
        {
            Texture2D targetTexture = new Texture2D(width, height);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    targetTexture.SetPixel(x, y, color);

            targetTexture.Apply();

            return targetTexture;
        }
        private Texture2D Get4ChannelHeightmap(int width, int height, Texture2D layer0, Texture2D layer1, Texture2D layer2, Texture2D layer3)
        {
            Texture2D targetTexture = new Texture2D(width, height);

            if (layer0)
                layer0 = Resize(layer0, width, height, false);
            if (layer1)
                layer1 = Resize(layer1, width, height, false);
            if (layer2)
                layer2 = Resize(layer2, width, height, false);
            if (layer3)
                layer3 = Resize(layer3, width, height, false);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    targetTexture.SetPixel(x, y, new Color(
                        layer0 ? layer0.GetPixel(x, y).r : 0,
                        layer1 ? layer1.GetPixel(x, y).g : 0,
                        layer2 ? layer2.GetPixel(x, y).b : 0,
                        layer3 ? layer3.GetPixel(x, y).a : 0
                        ));
                }
            }

            return targetTexture;
        }

        /// <summary>
        /// Apply the generated textures to the corvo terrain material (if in use)
        /// </summary>
        /// <param name="applyTextures">If false, only metallicness, smoothness, UVscale will be updated.</param>
        /// <param name="printLog">Log eventual errors.</param>
        public void Apply(bool applyTextures = true, bool printLog = false)
        {
            if (TextureArraysChanged())
            {
                if (printLog)
                    Debug.LogError("Textures array seems to be changed. Please generate texture array before calling this.");
                return;
            }

            Material mat = GetTerrain().materialTemplate;
            GetTerrain().materialTemplate = null;

            //get selected texture size
            Vector2 sizes = GetTextureSize();

            //aply main texture UVs
            float[] appliedMetallness = new float[4];
            float[] appliedSmoothness = new float[4];
            float[] appliedNormalScale = new float[4];
            Matrix4x4 tilings1 = new Matrix4x4();
            Matrix4x4 tilings2 = new Matrix4x4();

            int current_id = 0;//id rispetto a applied_textures delle albedo - id aument every 4-set texture
            for (int i = 0; i < GetTerrain().terrainData.terrainLayers.Length; i++)
            {
                TerrainLayer curLayer = GetTerrain().terrainData.terrainLayers[i];

                //set metallic and smoothness
                appliedMetallness[i % 4] = curLayer.metallic;
                appliedSmoothness[i % 4] = curLayer.smoothness;
                appliedNormalScale[i % 4] = curLayer.normalScale;


                //at the end of every 4-set, send the array to the material
                if ((i + 1) % 4 == 0 || i == (GetTerrain().terrainData.terrainLayers.Length - 1))//last of a 4-set or last in total
                {
                    string splatIndex = "Splats" + (current_id * 4).ToString() + ((1 + current_id) * 4 - 1).ToString();
                    string parallaxIndex = "Parallax" + (current_id * 4).ToString() + ((1 + current_id) * 4 - 1).ToString();

                    if (applyTextures && baked_diffuses[current_id] != null)
                    {
                        mat.SetTexture(splatIndex, baked_diffuses[current_id]);
                        mat.SetTexture(splatIndex + "_nm", baked_normals[current_id]);
                        if (baked_heightmaps.Length == baked_diffuses.Length)
                            mat.SetTexture(parallaxIndex, baked_heightmaps[current_id]);
                    }

                    mat.SetVector(splatIndex + "_met", new Vector4(appliedMetallness[0], appliedMetallness[1], appliedMetallness[2], appliedMetallness[3]));
                    mat.SetVector(splatIndex + "_smt", new Vector4(appliedSmoothness[0], appliedSmoothness[1], appliedSmoothness[2], appliedSmoothness[3]));
                    mat.SetVector(splatIndex + "_nm_intensity", new Vector4(appliedNormalScale[0], appliedNormalScale[1], appliedNormalScale[2], appliedNormalScale[3]));

                    current_id++;
                }

                //tilings
                if (i<8)
                {
                int id = i * 2;
                tilings1[(int)(id / 4), id % 4] = (1 / curLayer.tileSize.y) * terrainUVScale * GetTerrain().terrainData.size.x;
                id++;
                tilings1[(int)(id / 4), id % 4] = (1 / curLayer.tileSize.x) * terrainUVScale * GetTerrain().terrainData.size.x;
                }
                else
                {
                int id = (i-8) * 2;
                tilings2[(int)(id / 4), id % 4] = (1 / curLayer.tileSize.y) * terrainUVScale * GetTerrain().terrainData.size.x;
                id++;
                tilings2[(int)(id / 4), id % 4] = (1 / curLayer.tileSize.x) * terrainUVScale * GetTerrain().terrainData.size.x;
                }
            }

            //set uv tilings
            mat.SetMatrix("UVs", tilings1);
            mat.SetMatrix("UVs2", tilings2);

            //apply control textures
            if (applyTextures)
            {
                //2 alpha map textures = 8 layers, 4 alpha map textures = 16 layers
                int alphaTexturesCount =GetTerrain().terrainData.alphamapTextures.Length<=2?2:4;/* Mathf.Clamp(GetTerrain().terrainData.alphamapTextures.Length, 2, 4);*/
                for (int i = 0; i < alphaTexturesCount; i++)
                {
                    if (i < GetTerrain().terrainData.alphamapTextures.Length)
                        mat.SetTexture("_Control" + i, GetTerrain().terrainData.alphamapTextures[i]);
                    else
                        mat.SetTexture("_Control" + i, Texture2D.blackTexture);
                }
            }

            GetTerrain().materialTemplate = mat;
        }

        /// <summary>
        /// Set snow intensity if material allows it
        /// </summary>
        /// <param name="snowIntensity">[0,1] value of intensity</param>
        public void SetSnowIntensity(float snowIntensity)
        {
            //set uv tilings
            Material mat = GetTerrain().materialTemplate;
            GetTerrain().materialTemplate = null;
            mat.SetFloat("_SnowIntensity", snowIntensity);
            GetTerrain().materialTemplate = mat;
        }
    }


    public enum MaxTextureSize
    {
        res32x32 = 32,
        res64x64 = 64,
        res128x128 = 128,
        res256x256 = 256,
        res512x512 = 512,
        res1024x1024 = 1024,
        res2048x2048 = 2048,
        res4096x4096 = 4096
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CorvoTerrainManager))]
    public sealed class CorvoTerrainManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CorvoTerrainManager corvoTerrainManager = (CorvoTerrainManager)target;
            Color defaultButtonColor = UnityEngine.GUI.backgroundColor;

            if (!corvoTerrainManager.IsStandardTerrain())
            {
                if (corvoTerrainManager.standardTerrainMaterial != null)
                {
                    UnityEngine.GUI.backgroundColor = new Color(1, .5f, 0, 1);
                    if (GUILayout.Button("Switch to Standard unity terrain material"))
                        corvoTerrainManager.SetStandardTerrain();
                }

                if (corvoTerrainManager.TextureArraysChanged())
                {
                    UnityEngine.GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("Calculate textures"))
                        ForceCalculateTextures(corvoTerrainManager);
                }
                else
                {
                    UnityEngine.GUI.backgroundColor = defaultButtonColor;
                    if (GUILayout.Button("Force recalculate textures"))
                        ForceCalculateTextures(corvoTerrainManager);
                    /*if (!corvoTerrainManager.TextureArraysChanged())
                    {
                        if (GUILayout.Button("Just apply changes"))
                            corvoTerrainManager.Apply(true, true);
                    }*/
                }
            }

            if (!corvoTerrainManager.IsCorvoTerrain())
            {
                if (corvoTerrainManager.corvoTerrainMaterial != null)
                {
                    UnityEngine.GUI.backgroundColor = new Color(1, .5f, 0, 1);
                    if (GUILayout.Button("Switch to Corvo terrain material"))
                        corvoTerrainManager.SetCorvoTerrain();
                }
                else
                {
                    UnityEngine.GUI.color = new Color(1, .5f, 0, 1);
                    GUILayout.Label("Please, assign a corvo terrain material to corvoTerrainMaterial parameter.");
                }
            }
        }

        private void ForceCalculateTextures(CorvoTerrainManager corvoTerrainManager)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Generating textures", "This action should take just a few seconds..", .3f);
                long memoryUsed = corvoTerrainManager.GenerateTextureArrays();
                EditorUtility.DisplayProgressBar("Applying textures", "This action should take just a few seconds..", .6f);
                Debug.Log("Used memory: " + (memoryUsed / (float)1048576) + "Mb");
                corvoTerrainManager.Apply(true, true);
                EditorUtility.ClearProgressBar();
            }
            catch (Exception e)
            {
                Debug.LogError("Error occurred: " + e);
                EditorUtility.ClearProgressBar();
            }
        }
    }
#endif
}