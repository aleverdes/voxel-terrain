using UnityEngine;

namespace AffenCode.VoxelTerrain
{
    [CreateAssetMenu(fileName = "Atlas Layer", menuName = "Voxel Terrain/Atlas Layer", order = 1)]
    public class AtlasLayer : ScriptableObject
    {
        public Texture2D[] Textures;
    }
}