using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace TaigaGames.Voxels
{
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct CalculateMeshGridJob : IJobParallelFor, IDisposable
    {
        [ReadOnly] public VoxelTerrainChunk CurrentVoxelTerrainChunk;
        [ReadOnly] public int3 ChunkSize;
        [ReadOnly] public float3 VoxelSize;
        
        [ReadOnly] public VoxelTerrainChunk TopNeighbourVoxelTerrainChunk;
        [ReadOnly] public VoxelTerrainChunk BottomNeighbourVoxelTerrainChunk;
        [ReadOnly] public VoxelTerrainChunk LeftNeighbourVoxelTerrainChunk;
        [ReadOnly] public VoxelTerrainChunk RightNeighbourVoxelTerrainChunk;
        [ReadOnly] public VoxelTerrainChunk FrontNeighbourVoxelTerrainChunk;
        [ReadOnly] public VoxelTerrainChunk BackNeighbourVoxelTerrainChunk;
        
        [ReadOnly] public NativeArray<int> VerticesCount;
        [ReadOnly] public NativeArray<int> TriangleIndicesCount;

        [ReadOnly] public NativeArray<float2> VoxelVariantIndicesToUvPositions;
        [ReadOnly] public float2 UvSize;
        [ReadOnly] public NativeArray<int2> VoxelIndexToVoxelVariantsStartIndexAndCount;
        [ReadOnly] public NativeArray<int> VoxelVariantsTopTextureIndex;
        [ReadOnly] public NativeArray<int> VoxelVariantsBottomTextureIndex;
        [ReadOnly] public NativeArray<int> VoxelVariantsSideTextureIndex;

        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<float3> Positions;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<float3> Normals;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<float4> Tangents;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<float2> TexCoords;
        [WriteOnly, NativeDisableContainerSafetyRestriction] public NativeArray<ushort> TriangleIndices;

        public void Execute(int index)
        {
            switch (index)
            {
                case 0: CalculateTopFaces(index); break;
                case 1: CalculateBottomFaces(index); break;
                case 2: CalculateLeftFaces(index); break;
                case 3: CalculateRightFaces(index); break;
                case 4: CalculateFrontFaces(index); break;
                case 5: CalculateBackFaces(index); break;
            }
        }

        private void CalculateTopFaces(int index)
        {
            var vertexIndex = 0;
            for (var i = 0; i < index; i++)
                vertexIndex += VerticesCount[i];
            var startVertexIndex = vertexIndex;

            var triangleIndex = 0;
            for (var i = 0; i < index; i++)
                triangleIndex += TriangleIndicesCount[i];
            var startTriangleIndex = triangleIndex;
            
            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (vertexIndex - startVertexIndex >= VerticesCount[index])
                    return;
                
                var exists = y == ChunkSize.y - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !TopNeighbourVoxelTerrainChunk.IsVoxelExists(x, 0, z)
                             || y < ChunkSize.y - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y + 1, z);
                if (!exists) continue;
                
                var v000 = new float3(x, y, z) * VoxelSize;
                var v001 = new float3(x, y, z + 1) * VoxelSize;
                var v011 = new float3(x, y + 1, z + 1) * VoxelSize;
                var v010 = new float3(x, y + 1, z) * VoxelSize;
                var v100 = new float3(x + 1, y, z) * VoxelSize;
                var v101 = new float3(x + 1, y, z + 1) * VoxelSize;
                var v111 = new float3(x + 1, y + 1, z + 1) * VoxelSize;
                var v110 = new float3(x + 1, y + 1, z) * VoxelSize;  
                
                var voxelIndex = CurrentVoxelTerrainChunk.GetVoxelIndex(x, y, z);
                var variantIndex = (x + y + z) % VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].y;
                var texturePosition = VoxelVariantsTopTextureIndex[VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].x + variantIndex];
                var uvPosition = VoxelVariantIndicesToUvPositions[texturePosition];
                var uvSize = UvSize;
                
                Positions[vertexIndex] = v010;
                Normals[vertexIndex] = new float3(0, 1, 0);
                Tangents[vertexIndex] = new float4(1, 0, 0, -1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1f - uvSize.y);
                vertexIndex++;

                Positions[vertexIndex] = v011;
                Normals[vertexIndex] = new float3(0, 1, 0);
                Tangents[vertexIndex] = new float4(1, 0, 0, -1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1);
                vertexIndex++;
                
                Positions[vertexIndex] = v111;
                Normals[vertexIndex] = new float3(0, 1, 0);
                Tangents[vertexIndex] = new float4(1, 0, 0, -1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f);
                vertexIndex++;
                
                Positions[vertexIndex] = v110;
                Normals[vertexIndex] = new float3(0, 1, 0);
                Tangents[vertexIndex] = new float4(1, 0, 0, -1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f - uvSize.y);
                vertexIndex++;
                
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 3);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 1);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
            }
        }
        
        private void CalculateBottomFaces(int index)
        {
            var vertexIndex = 0;
            for (var i = 0; i < index; i++)
                vertexIndex += VerticesCount[i];
            var startVertexIndex = vertexIndex;

            var triangleIndex = 0;
            for (var i = 0; i < index; i++)
                triangleIndex += TriangleIndicesCount[i];
            var startTriangleIndex = triangleIndex;
            
            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (vertexIndex - startVertexIndex >= VerticesCount[index])
                    return;
                
                var exists = y == 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !BottomNeighbourVoxelTerrainChunk.IsVoxelExists(x, ChunkSize.y - 1, z)
                             || y > 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y - 1, z);
                if (!exists) continue;
                
                var v000 = new float3(x, y, z) * VoxelSize;
                var v001 = new float3(x, y, z + 1) * VoxelSize;
                var v011 = new float3(x, y + 1, z + 1) * VoxelSize;
                var v010 = new float3(x, y + 1, z) * VoxelSize;
                var v100 = new float3(x + 1, y, z) * VoxelSize;
                var v101 = new float3(x + 1, y, z + 1) * VoxelSize;
                var v111 = new float3(x + 1, y + 1, z + 1) * VoxelSize;
                var v110 = new float3(x + 1, y + 1, z) * VoxelSize;  
                
                var voxelIndex = CurrentVoxelTerrainChunk.GetVoxelIndex(x, y, z);
                var variantIndex = (x + y + z) % VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].y;
                var texturePosition = VoxelVariantsBottomTextureIndex[VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].x + variantIndex];
                var uvPosition = VoxelVariantIndicesToUvPositions[texturePosition];
                var uvSize = UvSize;
                
                Positions[vertexIndex] = v001;
                Normals[vertexIndex] = new float3(0, -1, 0);
                Tangents[vertexIndex] = new float4(1, 0, 0, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1f - uvSize.y);
                vertexIndex++;
                
                Positions[vertexIndex] = v000;
                Normals[vertexIndex] = new float3(0, -1, 0);
                Tangents[vertexIndex] = new float4(1, 0, 0, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1);
                vertexIndex++;
                
                Positions[vertexIndex] = v100;
                Normals[vertexIndex] = new float3(0, -1, 0);
                Tangents[vertexIndex] = new float4(1, 0, 0, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f);
                vertexIndex++;
                
                Positions[vertexIndex] = v101;
                Normals[vertexIndex] = new float3(0, -1, 0);
                Tangents[vertexIndex] = new float4(1, 0, 0, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f - uvSize.y);
                vertexIndex++;
                
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 3);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 1);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
            }
        }
        
        private void CalculateLeftFaces(int index)
        {
            var vertexIndex = 0;
            for (var i = 0; i < index; i++)
                vertexIndex += VerticesCount[i];
            var startVertexIndex = vertexIndex;

            var triangleIndex = 0;
            for (var i = 0; i < index; i++)
                triangleIndex += TriangleIndicesCount[i];
            var startTriangleIndex = triangleIndex;
            
            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (vertexIndex - startVertexIndex >= VerticesCount[index])
                    return;
                
                var exists = x == 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !LeftNeighbourVoxelTerrainChunk.IsVoxelExists(ChunkSize.x - 1, y, z)
                             || x > 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x - 1, y, z);
                if (!exists) continue;
                
                var v000 = new float3(x, y, z) * VoxelSize;
                var v001 = new float3(x, y, z + 1) * VoxelSize;
                var v011 = new float3(x, y + 1, z + 1) * VoxelSize;
                var v010 = new float3(x, y + 1, z) * VoxelSize;
                var v100 = new float3(x + 1, y, z) * VoxelSize;
                var v101 = new float3(x + 1, y, z + 1) * VoxelSize;
                var v111 = new float3(x + 1, y + 1, z + 1) * VoxelSize;
                var v110 = new float3(x + 1, y + 1, z) * VoxelSize;  
                
                var voxelIndex = CurrentVoxelTerrainChunk.GetVoxelIndex(x, y, z);
                var variantIndex = (x + y + z) % VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].y;
                var texturePosition = VoxelVariantsSideTextureIndex[VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].x + variantIndex];
                var uvPosition = VoxelVariantIndicesToUvPositions[texturePosition];
                var uvSize = UvSize;
                
                Positions[vertexIndex] = v000;
                Normals[vertexIndex] = new float3(-1, 0, 0);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f - uvSize.y);
                vertexIndex++;
                
                Positions[vertexIndex] = v001;
                Normals[vertexIndex] = new float3(-1, 0, 0);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1 - uvSize.y);
                vertexIndex++;
                
                Positions[vertexIndex] = v011;
                Normals[vertexIndex] = new float3(-1, 0, 0);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1);
                vertexIndex++;
                
                Positions[vertexIndex] = v010;
                Normals[vertexIndex] = new float3(-1, 0, 0);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f);
                vertexIndex++;
                
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 3);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 1);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
            }
        }
        
        private void CalculateRightFaces(int index)
        {
            var vertexIndex = 0;
            for (var i = 0; i < index; i++)
                vertexIndex += VerticesCount[i];
            var startVertexIndex = vertexIndex;

            var triangleIndex = 0;
            for (var i = 0; i < index; i++)
                triangleIndex += TriangleIndicesCount[i];
            var startTriangleIndex = triangleIndex;
            
            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (vertexIndex - startVertexIndex >= VerticesCount[index])
                    return;
                
                var exists = x == ChunkSize.x - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !RightNeighbourVoxelTerrainChunk.IsVoxelExists(0, y, z)
                             || x < ChunkSize.x - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x + 1, y, z);
                if (!exists) continue;
                
                var v000 = new float3(x, y, z) * VoxelSize;
                var v001 = new float3(x, y, z + 1) * VoxelSize;
                var v011 = new float3(x, y + 1, z + 1) * VoxelSize;
                var v010 = new float3(x, y + 1, z) * VoxelSize;
                var v100 = new float3(x + 1, y, z) * VoxelSize;
                var v101 = new float3(x + 1, y, z + 1) * VoxelSize;
                var v111 = new float3(x + 1, y + 1, z + 1) * VoxelSize;
                var v110 = new float3(x + 1, y + 1, z) * VoxelSize;  
                
                var voxelIndex = CurrentVoxelTerrainChunk.GetVoxelIndex(x, y, z);
                var variantIndex = (x + y + z) % VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].y;
                var texturePosition = VoxelVariantsSideTextureIndex[VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].x + variantIndex];
                var uvPosition = VoxelVariantIndicesToUvPositions[texturePosition];
                var uvSize = UvSize;
                
                Positions[vertexIndex] = v101;
                Normals[vertexIndex] = new float3(1, 0, 0);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f - uvSize.y);
                vertexIndex++;
                
                Positions[vertexIndex] = v100;
                Normals[vertexIndex] = new float3(1, 0, 0);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1 - uvSize.y);
                vertexIndex++;
                
                Positions[vertexIndex] = v110;
                Normals[vertexIndex] = new float3(1, 0, 0);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1);
                vertexIndex++;
                
                Positions[vertexIndex] = v111;
                Normals[vertexIndex] = new float3(1, 0, 0);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f);
                vertexIndex++;
                
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 3);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 1);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
            }
        }
        
        private void CalculateFrontFaces(int index)
        {
            var vertexIndex = 0;
            for (var i = 0; i < index; i++)
                vertexIndex += VerticesCount[i];
            var startVertexIndex = vertexIndex;

            var triangleIndex = 0;
            for (var i = 0; i < index; i++)
                triangleIndex += TriangleIndicesCount[i];
            var startTriangleIndex = triangleIndex;
            
            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (vertexIndex - startVertexIndex >= VerticesCount[index])
                    return;

                var exists = z == ChunkSize.z - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !FrontNeighbourVoxelTerrainChunk.IsVoxelExists(x, y, 0)
                             || z < ChunkSize.z - 1 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z + 1);
                if (!exists) continue;
                
                var v000 = new float3(x, y, z) * VoxelSize;
                var v001 = new float3(x, y, z + 1) * VoxelSize;
                var v011 = new float3(x, y + 1, z + 1) * VoxelSize;
                var v010 = new float3(x, y + 1, z) * VoxelSize;
                var v100 = new float3(x + 1, y, z) * VoxelSize;
                var v101 = new float3(x + 1, y, z + 1) * VoxelSize;
                var v111 = new float3(x + 1, y + 1, z + 1) * VoxelSize;
                var v110 = new float3(x + 1, y + 1, z) * VoxelSize;  
                
                var voxelIndex = CurrentVoxelTerrainChunk.GetVoxelIndex(x, y, z);
                var variantIndex = (x + y + z) % VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].y;
                var texturePosition = VoxelVariantsSideTextureIndex[VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].x + variantIndex];
                var uvPosition = VoxelVariantIndicesToUvPositions[texturePosition];
                var uvSize = UvSize;
                
                Positions[vertexIndex] = v001;
                Normals[vertexIndex] = new float3(0, 0, 1);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f - uvSize.y);
                vertexIndex++;
                
                Positions[vertexIndex] = v101;
                Normals[vertexIndex] = new float3(0, 0, 1);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1 - uvSize.y);
                vertexIndex++;
                
                Positions[vertexIndex] = v111;
                Normals[vertexIndex] = new float3(0, 0, 1);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1);
                vertexIndex++;
                
                Positions[vertexIndex] = v011;
                Normals[vertexIndex] = new float3(0, 0, 1);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f);
                vertexIndex++;
                
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 3);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 1);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
            }
        }
        
        private void CalculateBackFaces(int index)
        {
            var vertexIndex = 0;
            for (var i = 0; i < index; i++)
                vertexIndex += VerticesCount[i];
            var startVertexIndex = vertexIndex;

            var triangleIndex = 0;
            for (var i = 0; i < index; i++)
                triangleIndex += TriangleIndicesCount[i];
            var startTriangleIndex = triangleIndex;
            
            for (var x = 0; x < ChunkSize.x; x++)
            for (var y = 0; y < ChunkSize.y; y++)
            for (var z = 0; z < ChunkSize.z; z++)
            {
                if (vertexIndex - startVertexIndex >= VerticesCount[index])
                    return;

                var exists = z == 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !BackNeighbourVoxelTerrainChunk.IsVoxelExists(x, y, ChunkSize.z - 1)
                             || z > 0 && CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z) && !CurrentVoxelTerrainChunk.IsVoxelExists(x, y, z - 1);
                if (!exists) continue;
                
                var v000 = new float3(x, y, z) * VoxelSize;
                var v001 = new float3(x, y, z + 1) * VoxelSize;
                var v011 = new float3(x, y + 1, z + 1) * VoxelSize;
                var v010 = new float3(x, y + 1, z) * VoxelSize;
                var v100 = new float3(x + 1, y, z) * VoxelSize;
                var v101 = new float3(x + 1, y, z + 1) * VoxelSize;
                var v111 = new float3(x + 1, y + 1, z + 1) * VoxelSize;
                var v110 = new float3(x + 1, y + 1, z) * VoxelSize;  
                
                var voxelIndex = CurrentVoxelTerrainChunk.GetVoxelIndex(x, y, z);
                var variantIndex = (x + y + z) % VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].y;
                var texturePosition = VoxelVariantsSideTextureIndex[VoxelIndexToVoxelVariantsStartIndexAndCount[voxelIndex].x + variantIndex];
                var uvPosition = VoxelVariantIndicesToUvPositions[texturePosition];
                var uvSize = UvSize;
                
                Positions[vertexIndex] = v100;
                Normals[vertexIndex] = new float3(0, 0, -1);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f - uvSize.y);
                vertexIndex++;
                
                Positions[vertexIndex] = v000;
                Normals[vertexIndex] = new float3(0, 0, -1);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1 - uvSize.y);
                vertexIndex++;
                
                Positions[vertexIndex] = v010;
                Normals[vertexIndex] = new float3(0, 0, -1);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(0, 1);
                vertexIndex++;
                
                Positions[vertexIndex] = v110;
                Normals[vertexIndex] = new float3(0, 0, -1);
                Tangents[vertexIndex] = new float4(0, 0, -1, 1);
                TexCoords[vertexIndex] = new float2(uvPosition) + new float2(uvSize.x, 1f);
                vertexIndex++;
                
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 3);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 2);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 1);
                triangleIndex++;
                TriangleIndices[triangleIndex] = (ushort) (vertexIndex - 4);
                triangleIndex++;
            }
        }
        
        public void Dispose()
        {
            Positions.Dispose();
            Normals.Dispose();
            Tangents.Dispose();
            TexCoords.Dispose();
            TriangleIndices.Dispose();
        }
    }
}