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

        public float TopForwardRightVertexHeight;
        public float TopForwardLeftVertexHeight;
        public float TopBackRightVertexHeight;
        public float TopBackLeftVertexHeight;
    }

    [Serializable]
    public struct BlockFace
    {
        public bool Draw;
        public byte LayerIndex;
        public byte LayerTextureIndex;
    }
    
    public static class BlockExtensions
    {
        public static bool IsSolid(this Block block)
        {
            return !block.Void;
        }
    }
    
    public static class BlockFaceExtensions
    {
        public static bool IsSolid(this BlockFace blockFace)
        {
            return blockFace.Draw;
        }
    }
    
    public enum BlockFaceDirection
    {
        Top,
        Bottom,
        Left,
        Right,
        Forward,
        Back
    }
    
    public enum BlockVertexPosition
    {
        TopForwardRight,
        TopForwardLeft,
        TopBackRight,
        TopBackLeft,
        BottomForwardRight,
        BottomForwardLeft,
        BottomBackRight,
        BottomBackLeft
    }
}