using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AleVerDes.VoxelTerrain
{
    [CreateAssetMenu(fileName = "Atlas", menuName = "Voxel Terrain/Atlas", order = 0)]
    public class Atlas : ScriptableObject
    {
        [Header("Atlas Settings")]
        [Min(64)] public int Size = 512;
        [Min(16)] public int TextureSize = 32;
        public AtlasLayer[] Layers;

        [Header("Texture Atlas")]
        public Vector2[] TexturesPositions;
        public Vector2 TextureSizeInAtlas;
        public Texture TextureAtlas;

#if UNITY_EDITOR
        [ContextMenu("Find Atlas Layers")]
        public void FindAtlasLayers()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }
            
            var folderPath = Path.GetDirectoryName(assetPath);
            var textureGuids = AssetDatabase.FindAssets("t:AtlasLayer", new[] { folderPath });
            
            Layers = new AtlasLayer[textureGuids.Length];

            for (var i = 0; i < textureGuids.Length; i++)
            {
                var atlasLayerGuidGuid = textureGuids[i];
                var atlasLayer = AssetDatabase.LoadAssetAtPath<AtlasLayer>(AssetDatabase.GUIDToAssetPath(atlasLayerGuidGuid));
                Layers[i] = atlasLayer;
            }
        }
        
        [ContextMenu("Generate Texture Atlas")]
        public void GenerateTextureAtlas()
        {
            var atlasTextureIndex = 0;
            var atlasTextureLength = Size / TextureSize;
            
            var uvSize = (float) TextureSize / Size;
            TextureSizeInAtlas = new Vector2(uvSize, uvSize);

            var atlas = new Texture2D(Size, Size, TextureFormat.RGBA32, 0, true);
            var textures = new List<Vector2>();
                
            for (var layerIndex = 0; layerIndex < Layers.Length; layerIndex++)
            {
                var layer = Layers[layerIndex];
                for (var layerTextureIndex = 0; layerTextureIndex < layer.Textures.Length; layerTextureIndex++)
                {
                    var texture = layer.Textures[layerTextureIndex];
                    var x = atlasTextureIndex % atlasTextureLength;
                    var y = Mathf.FloorToInt((float)atlasTextureIndex / atlasTextureLength);

                    var scaledTexture = TextureScaler.Scale(texture, TextureSize, TextureSize);
                    var pixels = texture.GetPixels(0, 0, scaledTexture.width, scaledTexture.height);
                    atlas.SetPixels(TextureSize * x, Size - TextureSize * (y + 1), TextureSize, TextureSize, pixels);
                    textures.Add(new Vector2(x, y - 1) * uvSize);
                    atlasTextureIndex++;
                }
            }
            TexturesPositions = textures.ToArray();
            
            var pathToAtlas = AssetDatabase.GetAssetPath(this);
            var pathToPng = Path.Combine(Path.GetDirectoryName(pathToAtlas), Path.GetFileNameWithoutExtension(pathToAtlas) + ".png");
            var pngBytes = atlas.EncodeToPNG();
            File.WriteAllBytes(pathToPng, pngBytes);
            AssetDatabase.ImportAsset(pathToPng);
            
            TextureAtlas = AssetDatabase.LoadAssetAtPath<Texture>(pathToPng);
            TextureAtlas.filterMode = FilterMode.Point;
        }
#endif
    }
}