using UnityEngine;

namespace AleVerDes.VoxelTerrain
{
    public static class VoxelTerrainUtils
    {
        public static Vector2Int[] GetNeighbours(Vector2Int blockPosition)
        {
            return new[]
            {
                blockPosition + Vector2Int.left,
                blockPosition + Vector2Int.right,
                blockPosition + Vector2Int.up,
                blockPosition + Vector2Int.down,
                blockPosition + Vector2Int.left + Vector2Int.up,
                blockPosition + Vector2Int.right + Vector2Int.up,
                blockPosition + Vector2Int.left + Vector2Int.down,
                blockPosition + Vector2Int.right + Vector2Int.down,
            };
        }
    }
}