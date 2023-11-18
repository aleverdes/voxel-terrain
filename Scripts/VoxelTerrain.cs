using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AleVerDes.Voxels
{
    public class VoxelTerrain : MonoBehaviour
    {
        [SerializeField] private VoxelTerrainSettings _settings;
        [SerializeField] private NoiseGenerator _verticesNoise;
        [HideInInspector] [SerializeField] private List<ChunkData> _chunks;

        public VoxelTerrainSettings Settings => _settings;
        public NoiseGenerator VerticesNoise => _verticesNoise;

        [Button("Create new terrain")]
        public void New()
        {
            if (_settings == null)
                throw new Exception("Settings not set");
            
            if (_verticesNoise == null)
                throw new Exception("Vertices noise not set");
            
            Delete();
            CreateChunk(Vector3Int.zero);
            UpdateChunks();
        }

        public void Delete()
        {
            foreach (var chunk in _chunks)
            {
                DestroyImmediate(chunk.Components.GameObject);
                DestroyImmediate(chunk.Data);
                DestroyImmediate(chunk.Mesh);
            }
            
            _chunks.Clear();
        }

        public void CreateChunk(Vector3Int chunkPosition)
        {
            for (var i = 0; i < _chunks.Count; i++)
                if (chunkPosition == _chunks[i].Position)
                    throw new Exception("Chunk already exists");

            var chunk = new ChunkData();
            chunk.Position = chunkPosition;
            chunk.Mesh = new Mesh { name = $"Chunk {chunkPosition}" };
            chunk.Components.GameObject = new GameObject($"Chunk {chunkPosition}");
            chunk.Components.GameObject.transform.SetParent(transform);
            chunk.Components.MeshFilter = chunk.Components.GameObject.AddComponent<MeshFilter>();
            chunk.Components.MeshRenderer = chunk.Components.GameObject.AddComponent<MeshRenderer>();
            chunk.Components.MeshCollider = chunk.Components.GameObject.AddComponent<MeshCollider>();
            chunk.Components.MeshRenderer.sharedMaterial = _settings.TerrainMaterial;
            chunk.Components.MeshFilter.sharedMesh = chunk.Mesh;
            chunk.Components.MeshCollider.sharedMesh = chunk.Mesh;

            var chunkAsset = ScriptableObject.CreateInstance<VoxelTerrainChunk>();
            chunkAsset.Initialize(_settings.ChunkSize);
            chunk.Data = chunkAsset;

            for (int x = 0; x < _settings.ChunkSize.x; x++)
            for (int y = 0; y < _settings.ChunkSize.y; y++)
            for (int z = 0; z < _settings.ChunkSize.z; z++)
                if (y <= _settings.ChunkSize.y / 2)
                    chunk.Data.GetBlockVoxelIndex(new Vector3Int(x, y, z), _settings.ChunkSize) = (byte)Random.Range(1, _settings.TextureAtlas.Count);

#if UNITY_EDITOR
            var assetName = $"Chunk {chunkPosition}";
            var path = $"{Path.GetDirectoryName(SceneManager.GetActiveScene().path)}/Chunks";
            
            if (!Directory.Exists(path)) 
                Directory.CreateDirectory(path);

            AssetDatabase.CreateAsset(chunkAsset, Path.Combine(path, assetName + ".asset"));
#endif
            
            _chunks.Add(chunk);
        }

        public void UpdateChunk(Vector3Int chunk, bool includeNeighbours = false)
        {
            if (includeNeighbours)
            {
                var list = new List<Vector3Int>();
                list.Add(chunk);
                list.AddRange(Utils.GetHorizontalNeighbours(chunk, true));
                UpdateChunks(list);
            }
            else
            {
                UpdateChunks(new[] { chunk });
            }
        }

        [Button("Update chunks")]
        public void UpdateChunks()
        {
            UpdateChunks(_chunks.ConvertAll(x => x.Position));
        }

        public void UpdateChunks(IEnumerable<Vector3Int> chunks)
        {
            foreach (var chunkPosition in chunks)
            {
                var chunk = GetChunkData(chunkPosition);
                chunk.Mesh = chunk.Data.UpdateMesh(chunk.Mesh, new VoxelTerrainChunk.GenerationData
                {
                    ChunkPosition = chunkPosition,
                    VoxelTerrain = this
                });
            }
        }

        public VoxelTerrainChunk GetChunk(Vector3Int chunkPosition)
        {
            return GetChunkData(chunkPosition).Data;
        }

        private ChunkData GetChunkData(Vector3Int chunkPosition)
        {
            for (var i = 0; i < _chunks.Count; i++)
                if (chunkPosition == _chunks[i].Position)
                    return _chunks[i];
            throw new Exception("Chunk not found");
        }

        public bool TryGetChunk(Vector3Int chunkPosition, out VoxelTerrainChunk chunk)
        {
            for (var i = 0; i < _chunks.Count; i++)
                if (chunkPosition == _chunks[i].Position)
                {
                    chunk = _chunks[i].Data;
                    return true;
                }

            chunk = default;
            return false;
        }

        public ref byte GetBlockVoxelIndex(Vector3Int blockPosition, Vector3Int chunkSize)
        {
            var chunkIndex = GetChunkIndex(blockPosition);
            var chunk = _chunks[chunkIndex];
            var chunkPosition = chunk.Position;
            var blockPositionInChunk = blockPosition - chunkPosition * chunkSize;
            return ref chunk.Data.GetBlockVoxelIndex(blockPositionInChunk, chunkSize);
        }
        
        public ref byte GetBlockNoiseWeightIndex(Vector3Int blockPosition, Vector3Int chunkSize)
        {
            var chunkIndex = GetChunkIndex(blockPosition);
            var chunk = _chunks[chunkIndex];
            var chunkPosition = chunk.Position;
            var blockPositionInChunk = blockPosition - chunkPosition * chunkSize;
            return ref chunk.Data.GetBlockNoiseWeightIndex(blockPositionInChunk, chunkSize);
        }
        
        public int GetChunkIndex(Vector3Int blockPosition)
        {
            var chunkSize = _settings.ChunkSize;
            var chunkPosition = new Vector3Int(Mathf.FloorToInt(blockPosition.x / (float) chunkSize.x), Mathf.FloorToInt(blockPosition.y / (float) chunkSize.y), Mathf.FloorToInt(blockPosition.z / (float) chunkSize.z));
            var chunkIndex = chunkPosition.x + chunkPosition.y * chunkSize.x + chunkPosition.z * chunkSize.x * chunkSize.y;
            return chunkIndex;
        }
        
        public Bounds GetChunkBounds(int chunkIndex)
        {
            var chunkPosition = _chunks[chunkIndex].Position;
            var chunkWorldSize = _settings.GetChunkWorldSize();
            var zeroWorldPosition = new Vector3(chunkPosition.x * chunkWorldSize.x, chunkPosition.y * chunkWorldSize.y, chunkPosition.z * chunkWorldSize.z);
            var centerWorldPosition = zeroWorldPosition + chunkWorldSize / 2f;
            return new Bounds(centerWorldPosition, chunkWorldSize);
        }
        
        [Serializable]
        private struct ChunkData
        {
            public Vector3Int Position;
            public ChunkComponents Components;
            public VoxelTerrainChunk Data;
            public Mesh Mesh;
        
            [Serializable]
            public struct ChunkComponents
            {
                public GameObject GameObject;
                public MeshFilter MeshFilter;
                public MeshRenderer MeshRenderer;
                public MeshCollider MeshCollider;
            }
        }
    }
}