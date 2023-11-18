using System.Collections.Generic;
using UnityEngine;

namespace AleVerDes.Voxels
{
    public static class Utils
    {
        public static IEnumerable<Vector3Int> GetHorizontalNeighbours(Vector3Int voxelPosition, bool includeDiagonal = true)
        {
            if (!includeDiagonal)
                return new[]
                {
                    voxelPosition + Vector3Int.left,
                    voxelPosition + Vector3Int.right,
                    voxelPosition + Vector3Int.forward,
                    voxelPosition + Vector3Int.back,
                };
            
            return new[]
            {
                voxelPosition + Vector3Int.left,
                voxelPosition + Vector3Int.right,
                voxelPosition + Vector3Int.forward,
                voxelPosition + Vector3Int.back,
                voxelPosition + Vector3Int.left + Vector3Int.forward,
                voxelPosition + Vector3Int.right + Vector3Int.forward,
                voxelPosition + Vector3Int.left + Vector3Int.back,
                voxelPosition + Vector3Int.right + Vector3Int.back,
            };
        }
        
        public static IEnumerable<Vector3Int> GetAllNeighbours(Vector3Int voxelPosition)
        {
            var neighbours = new List<Vector3Int>();
            neighbours.AddRange(GetHorizontalNeighbours(voxelPosition));
            neighbours.AddRange(GetHorizontalNeighbours(voxelPosition + Vector3Int.up));
            neighbours.AddRange(GetHorizontalNeighbours(voxelPosition + Vector3Int.down));
            return neighbours;
        }
    }
}