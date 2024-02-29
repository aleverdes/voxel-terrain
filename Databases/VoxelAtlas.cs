using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using TravkinGames.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TravkinGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Atlas", fileName = "Voxel Atlas")]
    public class VoxelAtlas : ScriptableObject
    {
        private static readonly int[] AtlasSizes = { 64, 128, 256, 512, 1024, 2048, 4096 };
        private static readonly int[] TextureSizes = { 8, 16, 32, 64, 128, 256 };
        
        [SerializeField] private VoxelDatabase _voxelDatabase;
        
        [Header("Atlas Settings")] 
        [ValueDropdown("AtlasSizes")] [SerializeField] private int _atlasSize = 1024;
        [ValueDropdown("TextureSizes")] [SerializeField] private int _textureSize = 64;

        [Header("Technical Data")] 
        [SerializeField] private Texture _atlasTexture;
        [SerializeField] private List<Material> _atlasMaterials;
        [SerializeField] private float _textureRectScale = 0.995f;
        [SerializeField] private bool _showAdvancedTechnicalData;
        [SerializeField, ShowIf("_showAdvancedTechnicalData")] private VoxelData[] _voxelData;
        [SerializeField, ShowIf("_showAdvancedTechnicalData")] private Vector2[] _texturesPositions;
        [SerializeField, ShowIf("_showAdvancedTechnicalData")] private Vector2 _textureSizeInAtlas;
        [SerializeField, ShowIf("_showAdvancedTechnicalData")] private Vector2Int[] _voxelIndexToVoxelVariantsStartIndexAndCount;
        [SerializeField, ShowIf("_showAdvancedTechnicalData")] private int[] _voxelVariantsTopTextureIndex;
        [SerializeField, ShowIf("_showAdvancedTechnicalData")] private int[] _voxelVariantsBottomTextureIndex;
        [SerializeField, ShowIf("_showAdvancedTechnicalData")] private int[] _voxelVariantsSideTextureIndex;
        
        public Texture AtlasTexture => _atlasTexture;
        public List<Material> AtlasMaterials => _atlasMaterials;
        
        public Vector2[] TexturesPositions => _texturesPositions;
        public Vector2 TextureSizeInAtlas => _textureSizeInAtlas;
        public float TextureRectScale => _textureRectScale;
        public Vector2Int[] VoxelIndexToVoxelVariantsStartIndexAndCount => _voxelIndexToVoxelVariantsStartIndexAndCount;
        public int[] VoxelVariantsTopTextureIndex => _voxelVariantsTopTextureIndex;
        public int[] VoxelVariantsBottomTextureIndex => _voxelVariantsBottomTextureIndex;
        public int[] VoxelVariantsSideTextureIndex => _voxelVariantsSideTextureIndex;
        
#if UNITY_EDITOR
        [Button("Generate")]
        private void Generate()
        {
            CalculateMaterials();
            GenerateAtlasData();
        }

        /// <summary>
        /// Calculate the materials from the voxel database
        /// </summary>
        private void CalculateMaterials()
        {
            // Get all materials from the voxel database
            var materials = new List<Material>();
            foreach (var voxelDescriptor in _voxelDatabase.GetElements())
            foreach (var voxelDescriptorVariant in voxelDescriptor.Variants)
                if (!materials.Contains(voxelDescriptorVariant.Material))
                    materials.Add(voxelDescriptorVariant.Material);

            // Save the materials to the atlas
            _atlasMaterials = materials;
        }

        /// <summary>
        /// Generate the atlas texture and the voxel data
        /// </summary>
        private void GenerateAtlasData()
        {
            // Calculate the number of textures in the atlas
            var atlasTextureLength = _atlasSize / _textureSize;
            
            // Calculate the size of the texture in the atlas
            var uvSize = (float) _textureSize / _atlasSize;
            _textureSizeInAtlas = new Vector2(uvSize, uvSize);

            // Create the atlas texture and the voxel data
            var atlas = new Texture2D(_atlasSize, _atlasSize, TextureFormat.RGBA32, 0, true);
            var voxelDataList = new List<VoxelData>();
            var texturesIndices = new Dictionary<Texture2D, int>();
            var texturesUvPositions = new Dictionary<int, Vector2>();
            
            // Initialize the variants count
            var voxelIndexToVoxelVariantsStartIndexAndCount = new Vector2Int[_voxelDatabase.GetCount()];
            var voxelVariantsTopTextureIndex = new List<int>();
            var voxelVariantsBottomTextureIndex = new List<int>();
            var voxelVariantsSideTextureIndex = new List<int>();

            // Fill the atlas with textures
            var atlasTextureIndex = 0;
            var prevVoxelVariantsLength = 0;
            var globalVoxelVariantIndex = 0;
            for (var voxelIndex = 0; voxelIndex < _voxelDatabase.GetCount(); voxelIndex++)
            {
                // Get the voxel and its variants
                var voxel = _voxelDatabase[voxelIndex];
                var voxelData = new VoxelData
                {
                    VoxelIndex = voxelIndex,
                    VariantsTextures = new VoxelVariantTextures[voxel.Variants.Length]
                };
                
                voxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex] = new Vector2Int(prevVoxelVariantsLength, voxel.Variants.Length);
                prevVoxelVariantsLength += voxel.Variants.Length;

                // Fill the voxel data with textures
                for (var voxelVariantIndex = 0; voxelVariantIndex < voxel.Variants.Length; voxelVariantIndex++)
                {
                    // Get the variant and its textures
                    var voxelVariant = voxel.Variants[voxelVariantIndex];

                    voxelData.VariantsTextures[voxelVariantIndex].TopTextureIndex = GetVoxelVariantTextureAtlasIndex(voxelVariant.Top);
                    voxelData.VariantsTextures[voxelVariantIndex].BottomTextureIndex = GetVoxelVariantTextureAtlasIndex(voxelVariant.Bottom);
                    voxelData.VariantsTextures[voxelVariantIndex].SideTextureIndex = GetVoxelVariantTextureAtlasIndex(voxelVariant.Side);

                    voxelVariantsTopTextureIndex.Add(voxelData.VariantsTextures[voxelVariantIndex].TopTextureIndex);
                    voxelVariantsBottomTextureIndex.Add(voxelData.VariantsTextures[voxelVariantIndex].BottomTextureIndex);
                    voxelVariantsSideTextureIndex.Add(voxelData.VariantsTextures[voxelVariantIndex].SideTextureIndex);
                    globalVoxelVariantIndex++;
                    
                    continue;

                    // Get the index of the texture in the atlas
                    int GetVoxelVariantTextureAtlasIndex(Texture2D texture)
                    {
                        if (texturesIndices.TryGetValue(texture, out var inAtlasIndex))
                            return inAtlasIndex;

                        var topTextureColumnAndRow = GetTextureColumnAndRow(atlasTextureIndex, atlasTextureLength);
                        var uvPosition = WriteTextureToAtlas(atlas, topTextureColumnAndRow, texture);
                        texturesUvPositions[atlasTextureIndex] = uvPosition;

                        texturesIndices[texture] = atlasTextureIndex;
                        inAtlasIndex = atlasTextureIndex;
                        atlasTextureIndex++;

                        return inAtlasIndex;
                    }
                }

                // Add the voxel data to the list
                voxelDataList.Add(voxelData);
            }

            // Save voxel data and textures positions
            _voxelData = voxelDataList.ToArray();
            _texturesPositions = texturesUvPositions.Values.ToArray();
            _voxelIndexToVoxelVariantsStartIndexAndCount = voxelIndexToVoxelVariantsStartIndexAndCount;
            _voxelVariantsTopTextureIndex = voxelVariantsTopTextureIndex.ToArray();
            _voxelVariantsBottomTextureIndex = voxelVariantsBottomTextureIndex.ToArray();
            _voxelVariantsSideTextureIndex = voxelVariantsSideTextureIndex.ToArray();
            
            // Saving the atlas to the project
            var pathToAtlas = AssetDatabase.GetAssetPath(this);
            var pathToPng = Path.Combine(
                Path.GetDirectoryName(pathToAtlas) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(pathToAtlas)}.png"
            );
            var pngBytes = atlas.EncodeToPNG();
            File.WriteAllBytes(pathToPng, pngBytes);
            
            // Importing the texture to the project
            AssetDatabase.ImportAsset(pathToPng);
            _atlasTexture = AssetDatabase.LoadAssetAtPath<Texture>(pathToPng);
            _atlasTexture.filterMode = FilterMode.Point;
        }

        /// <summary>
        /// Get the column and row of the texture in the atlas
        /// </summary>
        /// <param name="index">Index of the texture</param>
        /// <param name="length">Length of the atlas</param>
        /// <returns>Column and row of the texture in the atlas</returns>
        private Vector2Int GetTextureColumnAndRow(int index, int length)
        {
            var x = index % length;
            var y = Mathf.FloorToInt((float)index / length);
            return new Vector2Int(x, y);
        }
        
        /// <summary>
        /// Write the texture to the atlas
        /// </summary>
        /// <param name="atlas">Atlas texture</param>
        /// <param name="position">Position of the texture in the atlas</param>
        /// <param name="texture">Texture to write</param>
        /// <returns>Possition of the texture in the atlas</returns>
        private Vector2 WriteTextureToAtlas(Texture2D atlas, Vector2Int position, Texture2D texture)
        {
            var scaledTexture = TextureScaler.Scale(texture, _textureSize, _textureSize);
            var pixels = scaledTexture.GetPixels(0, 0, scaledTexture.width, scaledTexture.height);
            atlas.SetPixels(_textureSize * position.x, _atlasSize - _textureSize * (position.y + 1), _textureSize, _textureSize, pixels);
            return position * _textureSizeInAtlas;
        }
#endif
        
        /// <summary>
        /// Container for the voxel data: index, material index and variant textures
        /// </summary>
        [Serializable]
        private struct VoxelData
        {
            public int VoxelIndex;
            public int MaterialIndex;
            public VoxelVariantTextures[] VariantsTextures;
        }

        /// <summary>
        /// Container for the variant textures: top, bottom and side
        /// </summary>
        [Serializable]
        private struct VoxelVariantTextures
        {
            public int TopTextureIndex;
            public int BottomTextureIndex;
            public int SideTextureIndex;
        }
    }
}