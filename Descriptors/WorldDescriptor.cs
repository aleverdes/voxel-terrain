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
        [SerializeField] private Vector3 _voxelSize = new Vector3(1, 1, 1);
        [SerializeField] private VoxelDatabase _voxelDatabase;
        [SerializeField] private VoxelAtlas _voxelAtlas;
        
        [Header("Pregeneration")]
        [SerializeField] private bool _isPregenerationEnabled = true;
        [SerializeField] private Vector3Int _pregenerationOriginPosition = new Vector3Int(0, 0, 0);
        [SerializeField] private Vector3Int _pregenerationSize = new Vector3Int(9, 3, 9);
        
        public int Seed => _seed;
        public bool IsInfinite => _isInfinite;
        public Vector3Int WorldSize => _worldSize;
        public Vector3Int ChunkSize => _chunkSize;
        public BiomeDatabase BiomeDatabase => _biomeDatabase;
        public BiomeMapGenerator BiomeMapGenerator => _biomeMapGenerator;
        public Vector3 VoxelSize => _voxelSize;
        public VoxelDatabase VoxelDatabase => _voxelDatabase;
        public VoxelAtlas VoxelAtlas => _voxelAtlas;
        
        public bool IsPregenerationEnabled => _isPregenerationEnabled;
        public Vector3Int PregenerationOriginPosition => _pregenerationOriginPosition;
        public Vector3Int PregenerationSize => _pregenerationSize;
    }
}