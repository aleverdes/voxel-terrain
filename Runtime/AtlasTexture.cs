using System;
using UnityEngine;

namespace AffenCode.VoxelTerrain
{
    [Serializable]
    public struct AtlasTexture
    {
        public byte LayerIndex;
        public byte LayerTextureIndex;
        public Vector2 UvPosition;
        public Vector2 UvSize;
    }
}