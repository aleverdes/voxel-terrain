using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace TravkinGames.Voxels
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct CalculateMeshParametersJob : IJobParallelFor, IDisposable
    {
        [ReadOnly] public VoxelTerrainChunk CurrentVoxelTerrainChunk;
        [ReadOnly] public int3 ChunkSize;
        
        [ReadOnly] public VoxelTerrainChunk TopNeighbourVoxelTerrainChunk;
        [ReadOnly] public VoxelTerrainChunk BottomNeighbourVoxelTerrainChunk;
        [ReadOnly] public VoxelTerrainChunk LeftNeighbourVoxelTerrainChunk;
        [ReadOnly] public VoxelTerrainChunk RightNeighbourVoxelTerrainChunk;
        [ReadOnly] public VoxelTerrainChunk FrontNeighbourVoxelTerrainChunk;
        [ReadOnly] public VoxelTerrainChunk BackNeighbourVoxelTerrainChunk;
        
        [WriteOnly] public NativeArray<int> ResultVerticesCount;
        [WriteOnly] public NativeArray<int> ResultTriangleIndicesCount;
        
        public void Execute(int index)
        {
            switch (index)
            {
                case 0: CalculateMeshParametersByTopFace(); break; 
                case 1: CalculateMeshParametersByBottomFace(); break;
                case 2: CalculateMeshParametersByLeftFace(); break;
                case 3: CalculateMeshParametersByRightFace(); break;
                case 4: CalculateMeshParametersByFrontFace(); break;
                case 5: CalculateMeshParametersByBackFace(); break;
            }
        }

        private void CalculateMeshParametersByTopFace()
        {
            var verticesCount = 0;
            var triangleIndicesCount = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (y == ChunkSize.y - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !TopNeighbourVoxelTerrainChunk.IsVoxelExists(x, 0, z)
                    || y < ChunkSize.y - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y + 1, z))
                {
                    verticesCount += 4;
                    triangleIndicesCount += 6;
                }
            }

            ResultVerticesCount[0] = verticesCount;
            ResultTriangleIndicesCount[0] = triangleIndicesCount;
        }
        
        private void CalculateMeshParametersByBottomFace()
        {
            var verticesCount = 0;
            var triangleIndicesCount = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (y == 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !BottomNeighbourVoxelTerrainChunk.IsVoxelExists(x, ChunkSize.y - 1, z)
                    || y > 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y - 1, z))
                {
                    verticesCount += 4;
                    triangleIndicesCount += 6;
                }
            }

            ResultVerticesCount[1] = verticesCount;
            ResultTriangleIndicesCount[1] = triangleIndicesCount;
        }
        
        private void CalculateMeshParametersByLeftFace()
        {
            var verticesCount = 0;
            var triangleIndicesCount = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (x == 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !LeftNeighbourVoxelTerrainChunk.IsVoxelExists(ChunkSize.x - 1, y, z)
                    || x > 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x - 1, y, z))
                {
                    verticesCount += 4;
                    triangleIndicesCount += 6;
                }
            }

            ResultVerticesCount[2] = verticesCount;
            ResultTriangleIndicesCount[2] = triangleIndicesCount;
        }
        
        private void CalculateMeshParametersByRightFace()
        {
            var verticesCount = 0;
            var triangleIndicesCount = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (x == ChunkSize.x - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !RightNeighbourVoxelTerrainChunk.IsVoxelExists(0, y, z)
                    || x < ChunkSize.x - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x + 1, y, z))
                {
                    verticesCount += 4;
                    triangleIndicesCount += 6;
                }
            }

            ResultVerticesCount[3] = verticesCount;
            ResultTriangleIndicesCount[3] = triangleIndicesCount;
        }
        
        private void CalculateMeshParametersByFrontFace()
        {
            var verticesCount = 0;
            var triangleIndicesCount = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (z == ChunkSize.z - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !FrontNeighbourVoxelTerrainChunk.IsVoxelExists(x, y, 0)
                    || z < ChunkSize.z - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z + 1))
                {
                    verticesCount += 4;
                    triangleIndicesCount += 6;
                }
            }

            ResultVerticesCount[4] = verticesCount;
            ResultTriangleIndicesCount[4] = triangleIndicesCount;
        }
        
        private void CalculateMeshParametersByBackFace()
        {
            var verticesCount = 0;
            var triangleIndicesCount = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (z == 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !BackNeighbourVoxelTerrainChunk.IsVoxelExists(x, y, ChunkSize.z - 1)
                    || z > 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z - 1))
                {
                    verticesCount += 4;
                    triangleIndicesCount += 6;
                }
            }

            ResultVerticesCount[5] = verticesCount;
            ResultTriangleIndicesCount[5] = triangleIndicesCount;
        }

        public void Dispose()
        {
            ResultVerticesCount.Dispose();
            ResultTriangleIndicesCount.Dispose();
        }
    }
}