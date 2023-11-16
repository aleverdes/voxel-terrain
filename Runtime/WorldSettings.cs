using UnityEngine;

namespace AleVerDes.VoxelTerrain
{
    [CreateAssetMenu(menuName = "Voxel Terrain/Terrain Settings", fileName = "Voxel Terrain Settings")]
    public class WorldSettings : ScriptableObject
    {
        public Vector2Int WorldSize = new Vector2Int(64, 64);
        public Vector2Int ChunkSize = new Vector2Int(16, 16);
        public float BlockSize = 1f;
        public Material WorldMaterial;
        public Atlas WorldAtlas;
    }
}