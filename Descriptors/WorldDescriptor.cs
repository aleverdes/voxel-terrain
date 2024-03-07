using Sirenix.OdinInspector;
using UnityEngine;

namespace TravkinGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Descriptors/World", fileName = "World Descriptor")]
    public class WorldDescriptor : ScriptableObject
    {
        [Header("General")] 
        [SerializeField] private int _seed; 
        
        [Header("Size")]
        [SerializeField] private bool _isInfinite;
        [SerializeField, HideIf("_isInfinite")] private Vector3Int _worldSizeInChunks = new Vector3Int(8, 1, 8);
        
        [Header("Chunk")]
        [SerializeField] private Vector3Int _chunkSize = new Vector3Int(16, 32, 16);
        
        [Header("Biomes")]
        [SerializeField] private BiomeDatabase _biomeDatabase;
        [SerializeField] private BiomeMapGenerator _biomeMapGenerator;
        
        [Header("Voxels")]
        [SerializeField] private Vector3 _voxelSize = new Vector3(1, 1, 1);
        [SerializeField] private VoxelDatabase _voxelDatabase;
        [SerializeField] private VoxelAtlas _voxelAtlas;
        
        public int Seed => _seed;
        public bool IsInfinite => _isInfinite;
        public Vector3Int WorldSizeInChunks => _worldSizeInChunks;
        public Vector3Int ChunkSize => _chunkSize;
        public BiomeDatabase BiomeDatabase => _biomeDatabase;
        public BiomeMapGenerator BiomeMapGenerator => _biomeMapGenerator;
        public Vector3 VoxelSize => _voxelSize;
        public VoxelDatabase VoxelDatabase => _voxelDatabase;
        public VoxelAtlas VoxelAtlas => _voxelAtlas;
    }
}