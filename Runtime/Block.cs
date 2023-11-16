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

        public BlockTopVerticesHeights TopVerticesHeights;
    }

    [Serializable]
    public struct BlockFace
    {
        public bool Draw;
        public byte LayerIndex;
        public byte LayerTextureIndex;
    }
    
    public struct BlockVertices
    {
        public Vector3 TopForwardRight;
        public Vector3 TopForwardLeft;
        public Vector3 TopBackRight;
        public Vector3 TopBackLeft;
        public Vector3 BottomForwardRight;
        public Vector3 BottomForwardLeft;
        public Vector3 BottomBackRight;
        public Vector3 BottomBackLeft;
    }

    public struct BlockTopVerticesHeights
    {
        public float ForwardRight;
        public float ForwardLeft;
        public float BackRight;
        public float BackLeft;
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