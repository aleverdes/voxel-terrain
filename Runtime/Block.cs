using System;
using UnityEngine;

namespace AffenCode.VoxelTerrain
{
    [Serializable]
    public struct Block
    {
        public bool Void;
        public Vector3Int Position;
        
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
        public bool Draw;
        public byte LayerIndex;
        public byte LayerTextureIndex;
    }
}