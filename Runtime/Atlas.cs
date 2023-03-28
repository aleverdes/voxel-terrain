using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AffenCode.VoxelTerrain
{
    [CreateAssetMenu(fileName = "Atlas", menuName = "Voxel Terrain/Atlas", order = 0)]
    public class Atlas : ScriptableObject
    {
        [Header("Atlas Settings")]
        [Min(64)] public int Size = 512;
        [Min(16)] public int TextureSize = 32;
        public AtlasLayer[] Layers;

        [Header("Texture Atlas")]
        public Texture TextureAtlas;
        public AtlasLayerData[] AtlasLayers;

#if UNITY_EDITOR
        [ContextMenu("Generate Texture Atlas")]
        public void GenerateTextureAtlas()
        {
            var atlasTextureIndex = 0;
            var atlasTextureLength = Size / TextureSize;
            
            var uvSize = (float) TextureSize / Size;

            var atlas = new Texture2D(Size, Size, TextureFormat.RGBA32, 0, true);
            
            AtlasLayers = new AtlasLayerData[Layers.Length];

            for (var layerIndex = 0; layerIndex < Layers.Length; layerIndex++)
            {
                var layer = Layers[layerIndex];
                AtlasLayers[layerIndex] = new AtlasLayerData()
                {
                    Textures = new AtlasTextureData[layer.Textures.Length]
                };
                
                for (var layerTextureIndex = 0; layerTextureIndex < layer.Textures.Length; layerTextureIndex++)
                {
                    var texture = layer.Textures[layerTextureIndex];
                    var x = atlasTextureIndex % atlasTextureLength;
                    var y = Mathf.FloorToInt((float)atlasTextureIndex / atlasTextureLength);

                    var pixels = texture.GetPixels(0, 0, texture.width, texture.height);
                    atlas.SetPixels(TextureSize * x, Size - TextureSize * (y + 1), TextureSize, TextureSize, pixels);

                    AtlasLayers[layerIndex].Textures[layerTextureIndex] = new AtlasTextureData()
                    {
                        UvPosition = new Vector2(x, y) * uvSize,
                        UvSize = new Vector2(uvSize, uvSize)
                    };

                    atlasTextureIndex++;
                }
            }

            var pathToAtlas = AssetDatabase.GetAssetPath(this);
            var pathToPng = Path.Combine(Path.GetDirectoryName(pathToAtlas), Path.GetFileNameWithoutExtension(pathToAtlas) + ".png");
            var pngBytes = atlas.EncodeToPNG();
            File.WriteAllBytes(pathToPng, pngBytes);
            AssetDatabase.ImportAsset(pathToPng);
            
            TextureAtlas = AssetDatabase.LoadAssetAtPath<Texture>(pathToPng);
        }
#endif
    }
}