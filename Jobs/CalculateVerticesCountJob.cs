using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace TravkinGames.Voxels
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct CalculateVerticesCountJob : IJobFor
    {
        public VoxelTerrainChunk CurrentVoxelTerrainChunk;
        public int3 ChunkSize;
        
        public VoxelTerrainChunk TopNeighbourVoxelTerrainChunk;
        public VoxelTerrainChunk BottomNeighbourVoxelTerrainChunk;
        public VoxelTerrainChunk LeftNeighbourVoxelTerrainChunk;
        public VoxelTerrainChunk RightNeighbourVoxelTerrainChunk;
        public VoxelTerrainChunk FrontNeighbourVoxelTerrainChunk;
        public VoxelTerrainChunk BackNeighbourVoxelTerrainChunk;
        
        public int ResultVerticesCount;
        
        public void Execute(int index)
        {
            switch (index)
            {
                case 0: ResultVerticesCount += CalculateTopVerticesCount(); break; 
                case 1: ResultVerticesCount += CalculateBottomVerticesCount(); break;
                case 2: ResultVerticesCount += CalculateLeftVerticesCount(); break;
                case 3: ResultVerticesCount += CalculateRightVerticesCount(); break;
                case 4: ResultVerticesCount += CalculateFrontVerticesCount(); break;
                case 5: ResultVerticesCount += CalculateBackVerticesCount(); break;
            }
        }

        private int CalculateTopVerticesCount()
        {
            var count = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (y == ChunkSize.y - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !TopNeighbourVoxelTerrainChunk.IsVoxelExists(x, y, z))
                    count += 4;
                else if (y < ChunkSize.y - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y + 1, z))
                    count += 4;
            }
            
            return count;
        }
        
        private int CalculateBottomVerticesCount()
        {
            var count = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (y == 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !BottomNeighbourVoxelTerrainChunk.IsVoxelExists(x, y, z))
                    count += 4;
                else if (y > 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y - 1, z))
                    count += 4;
            }
            
            return count;
        }
        
        private int CalculateLeftVerticesCount()
        {
            var count = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (x == 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !LeftNeighbourVoxelTerrainChunk.IsVoxelExists(x, y, z))
                    count += 4;
                else if (x > 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x - 1, y, z))
                    count += 4;
            }
            
            return count;
        }
        
        private int CalculateRightVerticesCount()
        {
            var count = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (x == ChunkSize.x - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !RightNeighbourVoxelTerrainChunk.IsVoxelExists(x, y, z))
                    count += 4;
                else if (x < ChunkSize.x - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x + 1, y, z))
                    count += 4;
            }
            
            return count;
        }
        
        private int CalculateFrontVerticesCount()
        {
            var count = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (z == 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !FrontNeighbourVoxelTerrainChunk.IsVoxelExists(x, y, z))
                    count += 4;
                else if (z > 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z - 1))
                    count += 4;
            }
            
            return count;
        }

        private int CalculateBackVerticesCount()
        {
            var count = 0;

            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (z == ChunkSize.z - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !BackNeighbourVoxelTerrainChunk.IsVoxelExists(x, y, z))
                    count += 4;
                else if (z < ChunkSize.z - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z + 1))
                    count += 4;
            }

            return count;
        }
    }
}