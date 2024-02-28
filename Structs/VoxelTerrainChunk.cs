using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace TravkinGames.Voxels
{
    public struct VoxelTerrainChunk : IDisposable
    {
        private NativeArray<byte> _voxelsIndices;
        private readonly byte _xSize;
        private readonly byte _ySize;
        private readonly byte _zSize;
        
        /// <summary>
        /// Create a new voxel terrain chunk.
        /// </summary>
        /// <param name="chunkSize">Chunk size</param>
        public VoxelTerrainChunk(Vector3Int chunkSize)
        {
            _xSize = (byte)chunkSize.x;
            _ySize = (byte)chunkSize.y;
            _zSize = (byte)chunkSize.z;
            _voxelsIndices = new NativeArray<byte>(chunkSize.x * chunkSize.y * chunkSize.z, Allocator.Persistent);
        }
        
        /// <summary>
        /// Set voxel index at the specified position.
        /// </summary>
        /// <param name="position">Voxel position in chunk space.</param>
        /// <param name="voxelIndex">Voxel index in voxel database.</param>
        public void SetVoxel(Vector3Int position, byte voxelIndex)
        {
            _voxelsIndices[position.x + position.y * _xSize + position.z * _xSize * _ySize] = (byte) (voxelIndex + 1);
        }
        
        /// <summary>
        /// Set voxel index at the specified position.
        /// </summary>
        /// <param name="position">Voxel position in chunk space.</param>
        /// <param name="voxelIndex">Voxel index in voxel database.</param>
        public void SetVoxel(int3 position, byte voxelIndex)
        {
            _voxelsIndices[position.x + position.y * _xSize + position.z * _xSize * _ySize] = (byte) (voxelIndex + 1);
        }
        
        /// <summary>
        /// Check if voxel exists at the specified position.
        /// </summary>
        /// <param name="x">Voxel X position in chunk space.</param>
        /// <param name="y">Voxel Y position in chunk space.</param>
        /// <param name="z">Voxel Z position in chunk space.</param>
        /// <returns>Voxel existance</returns>
        public bool IsVoxelExists(int x, int y, int z)
        {
            if (x < 0 || x >= _xSize) return false;
            if (y < 0 || y >= _ySize) return false;
            if (z < 0 || z >= _zSize) return false;
            return _voxelsIndices[x + y * _xSize + z * _xSize * _ySize] > 0;
        }
        
        /// <summary>
        /// Check if voxel exists at the specified position.
        /// </summary>
        /// <param name="position">Voxel position in chunk space.</param>
        /// <returns>Voxel existance</returns>
        public bool IsVoxelExists(Vector3Int position)
        {
            if (position.x < 0 || position.x >= _xSize) return false;
            if (position.y < 0 || position.y >= _ySize) return false;
            if (position.z < 0 || position.z >= _zSize) return false;
            return _voxelsIndices[position.x + position.y * _xSize + position.z * _xSize * _ySize] > 0;
        }
        
        /// <summary>
        /// Check if voxel exists at the specified position.
        /// </summary>
        /// <param name="position">Voxel position in chunk space.</param>
        /// <returns>Voxel existance</returns>
        public bool IsVoxelExists(int3 position)
        {
            if (position.x < 0 || position.x >= _xSize) return false;
            if (position.y < 0 || position.y >= _ySize) return false;
            if (position.z < 0 || position.z >= _zSize) return false;
            return _voxelsIndices[position.x + position.y * _xSize + position.z * _xSize * _ySize] > 0;
        }
        
        /// <summary>
        /// Get voxel index at the specified position.
        /// </summary>
        /// <param name="position">Voxel position in chunk space.</param>
        /// <returns>Voxel index in voxel database</returns>
        public byte GetVoxelIndex(Vector3Int position)
        {
            return (byte)(_voxelsIndices[position.x + position.y * _xSize + position.z * _xSize * _ySize] - 1);
        }
        
        /// <summary>
        /// Get voxel index at the specified position.
        /// </summary>
        /// <param name="position">Voxel position in chunk space.</param>
        /// <returns>Voxel index in voxel database</returns>
        public byte GetVoxelIndex(int3 position)
        {
            return (byte)(_voxelsIndices[position.x + position.y * _xSize + position.z * _xSize * _ySize] - 1);
        }

        /// <summary>
        /// Try to get voxel index at the specified position.
        /// </summary>
        /// <param name="position">Voxel position in chunk space.</param>
        /// <param name="voxelIndex">Voxel index in voxel database</param>
        /// <returns>Voxel existance</returns>
        public bool TryGetVoxel(Vector3Int position, out byte voxelIndex)
        {
            if (IsVoxelExists(position))
            {
                voxelIndex = GetVoxelIndex(position);
                return true;
            }

            voxelIndex = 0;
            return false;
        }

        /// <summary>
        /// Try to get voxel index at the specified position.
        /// </summary>
        /// <param name="position">Voxel position in chunk space.</param>
        /// <param name="voxelIndex">Voxel index in voxel database</param>
        /// <returns>Voxel existance</returns>
        public bool TryGetVoxel(int3 position, out byte voxelIndex)
        {
            if (IsVoxelExists(position))
            {
                voxelIndex = GetVoxelIndex(position);
                return true;
            }

            voxelIndex = 0;
            return false;
        }
        
        /// <summary>
        /// Remove voxel at the specified position.
        /// </summary>
        /// <param name="position">Voxel position in chunk space.</param>
        public void RemoveVoxel(Vector3Int position)
        {
            _voxelsIndices[position.x + position.y * _xSize + position.z * _xSize * _ySize] = 0;
        }
        
        /// <summary>
        /// Remove voxel at the specified position.
        /// </summary>
        /// <param name="position">Voxel position in chunk space.</param>
        public void RemoveVoxel(int3 position)
        {
            _voxelsIndices[position.x + position.y * _xSize + position.z * _xSize * _ySize] = 0;
        }

        /// <summary>
        /// Dispose the chunk.
        /// </summary>
        public void Dispose()
        {
            _voxelsIndices.Dispose();
        }
    }
}