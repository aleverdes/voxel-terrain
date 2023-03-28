using System;
using UnityEngine;

namespace AffenCode.VoxelTerrain
{
    [Serializable]
    public struct Block
    {
        public Vector2Int Position;
        
        public Face Top;
        public Face Bottom;
        public Face Left;
        public Face Right;
        public Face Forward;
        public Face Back;
    }

    [Serializable]
    public struct Face
    {
        public byte LayerIndex;
        public byte LayerTextureOverride;
    }
}