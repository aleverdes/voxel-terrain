using UnityEngine;

namespace AleVerDes.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Terrain Settings", fileName = "Voxel Terrain Settings")]
    public class VoxelTerrainSettings : ScriptableObject
    {
        [Header("Textures")]
        [SerializeField] private TextureAtlas _textureAtlas;
        [SerializeField] private Material _terrainMaterial;
        
        [Header("Sizes")]
        [SerializeField] private Vector3Int _chunkSize = new Vector3Int(16, 16, 16);
        [SerializeField] private Vector3 _blockSize = new Vector3(1f, 1f, 1f);
        
        [Header("Noise")]
        [SerializeField] private Vector3 _verticesNoiseScale = new Vector3(0.1f, 0.1f, 0.1f);
        
        public TextureAtlas TextureAtlas => _textureAtlas;
        public Material TerrainMaterial => _terrainMaterial;
        
        public Vector3Int ChunkSize => _chunkSize;
        public Vector3 BlockSize => _blockSize;
        
        public Vector3 VerticesNoiseScale => _verticesNoiseScale;
        
        public Vector3 GetChunkWorldSize()
        {
            return new Vector3(_chunkSize.x * _blockSize.x, _chunkSize.y * _blockSize.y, _chunkSize.z * _blockSize.z);
        }
    }
}