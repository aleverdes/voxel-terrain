using System.Collections.Generic;
using UnityEngine;

namespace TravkinGames.Voxels
{
    public class VoxelTerrain : MonoBehaviour
    {
        [SerializeField] private WorldDescriptor _worldDescriptor;
        [SerializeField] private Transform _generationOrigin;
        [SerializeField] private int _renderDistance = 1;
        
        [Header("Technical")]
        [SerializeField] private int _maxChunksCount = 3 * 3 * 3;
        
        private Dictionary<Vector3Int, VoxelTerrainChunk> _chunks;
        private Dictionary<Vector3Int, ChunkView> _activeChunkViews;
        private Queue<ChunkView> _freeChunkViews;
        
        private readonly HashSet<Vector3Int> _toDeactivateChunks = new();
        private readonly HashSet<Vector3Int> _toActivateChunks = new();

        private void Awake()
        {
            PrepareChunks();
        }

        private void Update()
        {
            _toDeactivateChunks.Clear();
            foreach (var chunkPosition in _chunks.Keys)
                _toDeactivateChunks.Add(chunkPosition);
            
            var originChunkPosition = new Vector3Int(
                Mathf.FloorToInt(_generationOrigin.position.x / _worldDescriptor.ChunkSize.x),
                Mathf.FloorToInt(_generationOrigin.position.y / _worldDescriptor.ChunkSize.y),
                Mathf.FloorToInt(_generationOrigin.position.z / _worldDescriptor.ChunkSize.z)
            );
            
            for (var x = _renderDistance; x <= _renderDistance; x++)
            for (var y = _renderDistance; y <= _renderDistance; y++)
            for (var z = _renderDistance; z <= _renderDistance; z++)
            {
                var chunkPosition = originChunkPosition + new Vector3Int(x, y, z);
                
                if (!_chunks.ContainsKey(chunkPosition)) 
                    GenerateChunk(chunkPosition);
                
                _toDeactivateChunks.Remove(chunkPosition);
                _toActivateChunks.Add(chunkPosition);
            }
            
            foreach (var chunkPosition in _toDeactivateChunks)
                if (_activeChunkViews.ContainsKey(chunkPosition))
                    DeactivateChunk(chunkPosition);

            foreach (var chunkPosition in _toActivateChunks)
                if (!_activeChunkViews.ContainsKey(chunkPosition))
                    ActivateChunk(chunkPosition);
        }

        private void PrepareChunks()
        {
            _activeChunkViews = new Dictionary<Vector3Int, ChunkView>();
            _freeChunkViews = new Queue<ChunkView>();
            _chunks = new Dictionary<Vector3Int, VoxelTerrainChunk>();
            for (var i = 0; i < _maxChunksCount; i++)
            {
                var chunk = new GameObject("Chunk #" + i);
                chunk.transform.SetParent(transform);
                var mesh = new Mesh
                {
                    name = "Chunk #" + i
                };
                var meshFilter = chunk.AddComponent<MeshFilter>();
                var meshRenderer = chunk.AddComponent<MeshRenderer>();
                var meshCollider = chunk.AddComponent<MeshCollider>();

                var atlasMaterials = _worldDescriptor.VoxelAtlas.AtlasMaterials;
                meshRenderer.sharedMaterials = new Material[atlasMaterials.Length];
                for (var j = 0; j < atlasMaterials.Length; j++) 
                    meshRenderer.sharedMaterials[j] = atlasMaterials[j];

                meshFilter.sharedMesh = mesh;
                meshCollider.sharedMesh = mesh;
                _freeChunkViews.Enqueue(new ChunkView
                {
                    Mesh = mesh,
                    MeshFilter = meshFilter,
                    MeshRenderer = meshRenderer,
                    MeshCollider = meshCollider
                });
            }
        }

        private void GenerateChunk(Vector3Int chunkPosition)
        {
            var chunk = new VoxelTerrainChunk(_worldDescriptor.ChunkSize);

            for (var x = 0; x < _worldDescriptor.ChunkSize.x; x++)
            for (var y = 0; y < _worldDescriptor.ChunkSize.y; y++)
            for (var z = 0; z < _worldDescriptor.ChunkSize.z; z++)
            {
                var voxelPosition = new Vector3Int(x, y, z);
                var voxelBiomeState = _worldDescriptor.BiomeMapGenerator.GetVoxelBiomeState(_worldDescriptor.Seed, voxelPosition);
                var bestBiome = voxelBiomeState.BestBiome;
                var bestVoxel = bestBiome.GetVoxel(_worldDescriptor.Seed, voxelPosition);
                var noiseSum = 0f;
                for (var i = 0; i < voxelBiomeState.AllBiomes.Length; i++)
                    noiseSum += voxelBiomeState.AllBiomes[i].Weight
                                * voxelBiomeState.AllBiomes[i].BiomeDescriptor.LandscapeNoise.GetNoiseWithSeed(_worldDescriptor.Seed, x, y, z);
                noiseSum /= voxelBiomeState.AllBiomes.Length;
                if (bestBiome.IsVoxelExists(voxelPosition, noiseSum))
                    chunk.SetVoxel(voxelPosition, (byte) _worldDescriptor.VoxelDatabase.GetIndexOf(bestVoxel));
            }
            
            _chunks.Add(chunkPosition, chunk);
        }

        private ChunkView ActivateChunk(Vector3Int chunkPosition)
        {
            var chunkView = _freeChunkViews.Dequeue();
            chunkView.MeshRenderer.enabled = true;
            chunkView.MeshCollider.enabled = true;
            _activeChunkViews.Add(chunkPosition, chunkView);
            return chunkView;
        }
        
        private void DeactivateChunk(Vector3Int chunkPosition)
        {
            var chunkView = _activeChunkViews[chunkPosition];
            chunkView.MeshRenderer.enabled = false;
            chunkView.MeshCollider.enabled = false;
            _activeChunkViews.Remove(chunkPosition);
            _freeChunkViews.Enqueue(chunkView);
        }

        private class ChunkView
        {
            public Mesh Mesh;
            public MeshFilter MeshFilter;
            public MeshRenderer MeshRenderer;
            public MeshCollider MeshCollider;
        }
    }
}