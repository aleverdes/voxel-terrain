using UnityEngine;

namespace TravkinGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Descriptors/World", fileName = "World Descriptor")]
    public class WorldDescriptor : ScriptableObject
    {
        [Header("General")] 
        [SerializeField] private int _seed; 
        
        [Header("Size")]
        [SerializeField] private bool _isInfinite = true;
        [SerializeField] private Vector3Int _worldSize = new Vector3Int(256, 64, 256);
        
        [Header("Chunk")]
        [SerializeField] private Vector3Int _chunkSize = new Vector3Int(16, 16, 16);
        
        [Header("Biomes")]
        [SerializeField] private BiomeDatabase _biomeDatabase;
        [SerializeField] private BiomeMapGenerator _biomeMapGenerator;
        
        [Header("Voxels")]
        [SerializeField] private VoxelDatabase _voxelDatabase;
        [SerializeField] private VoxelAtlas _voxelAtlas;
        
        public int Seed => _seed;
        public bool IsInfinite => _isInfinite;
        public Vector3Int WorldSize => _worldSize;
        public Vector3Int ChunkSize => _chunkSize;
        public BiomeDatabase BiomeDatabase => _biomeDatabase;
        public BiomeMapGenerator BiomeMapGenerator => _biomeMapGenerator;
        public VoxelDatabase VoxelDatabase => _voxelDatabase;
        public VoxelAtlas VoxelAtlas => _voxelAtlas;
    }
}