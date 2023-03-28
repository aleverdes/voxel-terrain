using System.IO;
using System.Linq;
using UnityEngine;

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
        public AtlasTexture[] AtlasTextures;

#if UNITY_EDITOR
        [ContextMenu("Generate Texture Atlas")]
        public void GenerateTextureAtlas()
        {
            var atlasTextureIndex = 0;
            var atlasTextureLength = Size / TextureSize;
            
            var uvSize = (float) TextureSize / Size;

            var atlas = new Texture2D(Size, Size, TextureFormat.RGBA32, 0, true);
            
            AtlasTextures = new AtlasTexture[CalculateAtlasTexturesLength()];

            var layerIndex = 0;
            foreach (var layer in Layers)
            {
                var layerTextureIndex = 0;
                foreach (var texture in layer.Textures)
                {
                    var x = atlasTextureIndex % atlasTextureLength;
                    var y = Mathf.FloorToInt((float) atlasTextureIndex / atlasTextureLength);

                    var pixels = texture.GetPixels(0, 0, texture.width, texture.height);
                    atlas.SetPixels(TextureSize * x, Size - TextureSize * (y + 1), TextureSize, TextureSize, pixels);

                    AtlasTextures[atlasTextureIndex] = new AtlasTexture
                    {
                        LayerIndex = (byte) layerIndex,
                        LayerTextureIndex = (byte) layerTextureIndex,
                        UvPosition = new Vector2(x, y) * uvSize,
                        UvSize = new Vector2(uvSize, uvSize)
                    };
                        
                    layerTextureIndex++;
                    atlasTextureIndex++;
                }

                layerIndex++;
            }

            var pathToAtlas = AssetDatabase.GetAssetPath(this);
            var pathToPng = Path.Combine(Path.GetDirectoryName(pathToAtlas), Path.GetFileNameWithoutExtension(pathToAtlas) + ".png");
            var pngBytes = atlas.EncodeToPNG();
            File.WriteAllBytes(pathToPng, pngBytes);
            AssetDatabase.ImportAsset(pathToPng);
            
            TextureAtlas = AssetDatabase.LoadAssetAtPath<Texture>(pathToPng);
        }

        private int CalculateAtlasTexturesLength()
        {
            return Layers.Sum(layer => layer.Textures.Length);
        }
#endif
    }
}