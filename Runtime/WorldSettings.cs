using UnityEngine;

namespace AleVerDes.VoxelTerrain
{
    [CreateAssetMenu(menuName = "Voxel Terrain/Terrain Settings", fileName = "Voxel Terrain Settings")]
    public class WorldSettings : ScriptableObject
    {
        public Vector3Int WorldSize = new Vector3Int(64, 16, 64);
        public Vector2Int ChunkSize = new Vector2Int(16, 16);
        public float BlockSize = 1f;
        public Material WorldMaterial;
        public Atlas WorldAtlas;
    }
}