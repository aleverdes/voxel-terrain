using System;
using UnityEngine;

namespace AffenCode.VoxelTerrain
{
    [Serializable]
    public struct AtlasLayerData
    {
        public AtlasTextureData[] Textures;
    }

    [Serializable]
    public struct AtlasTextureData
    {
        public Vector2 UvPosition;
        public Vector2 UvSize;
    }
}