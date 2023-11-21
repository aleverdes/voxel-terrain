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
    [SelectionBase]
    [DisallowMultipleComponent]
    public class VoxelTerrain : MonoBehaviour
    {
        [SerializeField] private VoxelTerrainSettings _settings;
        [SerializeField] private NoiseProvider _verticesNoise;
        [HideInInspector] [SerializeField] private List<ChunkData> _chunks;
        
        [HideInInspector] public VoxelTerrainEditorTool SelectedEditorTool;
        [HideInInspector] public float PaintingBrushRadius = 1f;
        [HideInInspector] public float NoiseWeightBrushRadius = 1f;
        [Range(0, 255f)] [HideInInspector] public float NoiseWeightBrushStrength = 1f;
        [HideInInspector] public Voxel SelectedPaintingVoxel;
        
        public VoxelTerrainSettings Settings => _settings;
        public NoiseProvider VerticesNoise => _verticesNoise;

        [Button("Create new terrain")]
        [ContextMenu("Create new terrain")]
        public void New()
        {
            if (_settings == null)
                throw new Exception("Settings not set");
            
            if (_verticesNoise == null)
                throw new Exception("Vertices noise not set");
            
            Delete();
            CreateChunk(Vector3Int.zero);
            
            CreateChunk(Vector3Int.right);
            CreateChunk(Vector3Int.right + Vector3Int.forward);
            CreateChunk(Vector3Int.right + Vector3Int.back);
            
            CreateChunk(Vector3Int.left);
            CreateChunk(Vector3Int.left + Vector3Int.forward);
            CreateChunk(Vector3Int.left + Vector3Int.back);
            
            CreateChunk(Vector3Int.forward);
            CreateChunk(Vector3Int.back);
            
            UpdateChunks();
        }

        public void Delete()
        {
            foreach (var chunk in _chunks)
            {
                if (chunk.Components.GameObject != null)
                    DestroyImmediate(chunk.Components.GameObject);
#if UNITY_EDITOR
                if (chunk.Data != null)
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(chunk.Data));
                if (chunk.Mesh != null)
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(chunk.Mesh));
#endif
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
        [ContextMenu("Update chunks")]
        public void UpdateChunks()
        {
            UpdateChunks(_chunks.ConvertAll(x => x.Position));
        }

        public void UpdateChunks(IEnumerable<Vector3Int> chunks)
        {
            foreach (var chunkPosition in chunks)
            {
                var chunk = GetChunkData(chunkPosition);
                chunk.Data.UpdateMesh(ref chunk.Mesh, new VoxelTerrainChunk.GenerationData
                {
                    ChunkPosition = chunkPosition,
                    VoxelTerrain = this
                });
                chunk.Components.MeshCollider.sharedMesh = chunk.Mesh;
#if UNITY_EDITOR
                EditorUtility.SetDirty(chunk.Data);
                var assetName = $"Chunk {chunk.Position} Mesh";
                var path = $"{Path.GetDirectoryName(SceneManager.GetActiveScene().path)}/Chunks/" + assetName + ".asset";
                if (!File.Exists(path))
                {
                    AssetDatabase.CreateAsset(chunk.Mesh, path);
                    AssetDatabase.SaveAssets();
                }
#endif
            }
        }

        public void UpdateChunks(IEnumerable<int> chunks)
        {
            foreach (var chunkIndex in chunks)
            {
                var chunk = _chunks[chunkIndex];
                chunk.Data.UpdateMesh(ref chunk.Mesh, new VoxelTerrainChunk.GenerationData
                {
                    ChunkPosition = chunk.Position,
                    VoxelTerrain = this
                });
                chunk.Components.MeshCollider.sharedMesh = chunk.Mesh;
                chunk.Components.MeshFilter.sharedMesh = chunk.Mesh;
#if UNITY_EDITOR
                EditorUtility.SetDirty(chunk.Data);
                var assetName = $"Chunk {chunk.Position} Mesh";
                var path = $"{Path.GetDirectoryName(SceneManager.GetActiveScene().path)}/Chunks/" + assetName + ".asset";
                if (!File.Exists(path))
                {
                    AssetDatabase.CreateAsset(chunk.Mesh, path);
                    AssetDatabase.SaveAssets();
                }
#endif
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

        public bool IsSolidBlock(Vector3Int blockPosition)
        {
            var chunkIndex = GetChunkIndex(blockPosition);
            if (chunkIndex < 0 || chunkIndex >= _chunks.Count)
                return false;
            return GetBlockVoxelIndex(blockPosition) > 0;
        }

        public bool IsBlockExistsInChunks(Vector3Int blockPosition)
        {
            var chunkIndex = GetChunkIndex(blockPosition);
            if (chunkIndex < 0 || chunkIndex >= _chunks.Count)
                return false;
            return true;
        }
        
        public ref byte GetBlockVoxelIndex(Vector3Int blockPosition)
        {
            var chunkSize = _settings.ChunkSize;
            var chunkIndex = GetChunkIndex(blockPosition);
            var chunk = _chunks[chunkIndex];
            var chunkPosition = chunk.Position;
            var blockPositionInChunk = blockPosition - chunkPosition * chunkSize;
            return ref chunk.Data.GetBlockVoxelIndex(blockPositionInChunk, chunkSize);
        }
        
        public ref byte GetBlockNoiseWeight(Vector3Int blockPosition)
        {
            var chunkSize = _settings.ChunkSize;
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
            
            for (var i = 0; i < _chunks.Count; i++)
                if (chunkPosition == _chunks[i].Position)
                    return i;

            return -1;
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