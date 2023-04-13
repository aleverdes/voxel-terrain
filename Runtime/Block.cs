using System;
using UnityEngine;

namespace AleVerDes.VoxelTerrain
{
    [Serializable]
    public struct Block
    {
        public bool Void;
        public Vector3Int Position;
        public Vector2Int Chunk;
        
        public BlockFace Top;
        public BlockFace Bottom;
        public BlockFace Left;
        public BlockFace Right;
        public BlockFace Forward;
        public BlockFace Back;

        public BlockVertex TopForwardRight;
        public BlockVertex TopForwardLeft;
        public BlockVertex TopBackRight;
        public BlockVertex TopBackLeft;
        public BlockVertex BottomForwardRight;
        public BlockVertex BottomForwardLeft;
        public BlockVertex BottomBackRight;
        public BlockVertex BottomBackLeft;
    }

    [Serializable]
    public struct BlockFace
    {
        public bool Draw;
        public byte LayerIndex;
        public byte LayerTextureIndex;
    }

    [Serializable]
    public struct BlockVertex
    {
        public float HeightOffset;
    }
}