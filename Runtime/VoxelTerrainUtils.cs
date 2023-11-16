using UnityEngine;

namespace AleVerDes.VoxelTerrain
{
    public static class VoxelTerrainUtils
    {
        public static Vector3Int[] GetNeighbours(Vector3Int blockToGettingNeighbours)
        {
            return new[]
            {
                blockToGettingNeighbours + Vector3Int.left,
                blockToGettingNeighbours + Vector3Int.right,
                blockToGettingNeighbours + Vector3Int.forward,
                blockToGettingNeighbours + Vector3Int.back,
                blockToGettingNeighbours + Vector3Int.left + Vector3Int.forward,
                blockToGettingNeighbours + Vector3Int.right + Vector3Int.forward,
                blockToGettingNeighbours + Vector3Int.left + Vector3Int.back,
                blockToGettingNeighbours + Vector3Int.right + Vector3Int.back,
            };
        }
    }
}