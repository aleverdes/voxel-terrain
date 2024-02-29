using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace TravkinGames.Voxels
{
    public class VoxelTerrain : MonoBehaviour
    {
        [SerializeField] private WorldDescriptor _worldDescriptor;
        [SerializeField] private Transform _generationOrigin;
        [SerializeField] private int _renderDistance = 1;
        
        private Dictionary<Vector3Int, VoxelTerrainChunk> _chunks;
        private Dictionary<Vector3Int, ChunkView> _activeChunkViews;
        private Queue<ChunkView> _freeChunkViews;
        
        private readonly HashSet<Vector3Int> _toDeactivateChunks = new();
        private readonly HashSet<Vector3Int> _toActivateChunks = new();
        
        private NativeArray<float2> _voxelVariantIndicesToUvPositions;
        private NativeArray<int2> _voxelIndexToVoxelVariantsStartIndexAndCount;
        private NativeArray<int> _voxelVariantsTopTextureIndex;
        private NativeArray<int> _voxelVariantsBottomTextureIndex;
        private NativeArray<int> _voxelVariantsSideTextureIndex;
        
        private void Awake()
        {
            PrepareAtlasData();
            PrepareChunks();
            if (_worldDescriptor.IsPregenerationEnabled)
                PregenerateChunks();
        }

        private void OnDestroy()
        {
            DisposeAtlasData();
        }

        private void Update()
        {
            _toDeactivateChunks.Clear();
            foreach (var chunkPosition in _chunks.Keys)
                _toDeactivateChunks.Add(chunkPosition);
            
            _toActivateChunks.Clear();
            
            var originChunkPosition = new Vector3Int(
                Mathf.FloorToInt(_generationOrigin.position.x / _worldDescriptor.ChunkSize.x),
                Mathf.FloorToInt(_generationOrigin.position.y / _worldDescriptor.ChunkSize.y),
                Mathf.FloorToInt(_generationOrigin.position.z / _worldDescriptor.ChunkSize.z)
            );
            
            for (var x = -_renderDistance; x <= _renderDistance; x++)
            for (var y = -_renderDistance; y <= _renderDistance; y++)
            for (var z = -_renderDistance; z <= _renderDistance; z++)
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

        private void PrepareAtlasData()
        {
            _voxelVariantIndicesToUvPositions = new NativeArray<float2>(_worldDescriptor.VoxelAtlas.TexturesPositions.Length, Allocator.Persistent);
            for (int i = 0; i < _worldDescriptor.VoxelAtlas.TexturesPositions.Length; i++)
                _voxelVariantIndicesToUvPositions[i] = _worldDescriptor.VoxelAtlas.TexturesPositions[i];
            
            _voxelIndexToVoxelVariantsStartIndexAndCount = new NativeArray<int2>(_worldDescriptor.VoxelAtlas.VoxelIndexToVoxelVariantsStartIndexAndCount.Length, Allocator.Persistent);
            for (int i = 0; i < _worldDescriptor.VoxelAtlas.VoxelIndexToVoxelVariantsStartIndexAndCount.Length; i++)
                _voxelIndexToVoxelVariantsStartIndexAndCount[i] = new int2(_worldDescriptor.VoxelAtlas.VoxelIndexToVoxelVariantsStartIndexAndCount[i].x, _worldDescriptor.VoxelAtlas.VoxelIndexToVoxelVariantsStartIndexAndCount[i].y);
            
            _voxelVariantsTopTextureIndex = new NativeArray<int>(_worldDescriptor.VoxelAtlas.VoxelVariantsTopTextureIndex.Length, Allocator.Persistent);
            for (int i = 0; i < _worldDescriptor.VoxelAtlas.VoxelVariantsTopTextureIndex.Length; i++)
                _voxelVariantsTopTextureIndex[i] = _worldDescriptor.VoxelAtlas.VoxelVariantsTopTextureIndex[i];
            
            _voxelVariantsBottomTextureIndex = new NativeArray<int>(_worldDescriptor.VoxelAtlas.VoxelVariantsBottomTextureIndex.Length, Allocator.Persistent);
            for (int i = 0; i < _worldDescriptor.VoxelAtlas.VoxelVariantsBottomTextureIndex.Length; i++)
                _voxelVariantsBottomTextureIndex[i] = _worldDescriptor.VoxelAtlas.VoxelVariantsBottomTextureIndex[i];
            
            _voxelVariantsSideTextureIndex = new NativeArray<int>(_worldDescriptor.VoxelAtlas.VoxelVariantsSideTextureIndex.Length, Allocator.Persistent);
            for (int i = 0; i < _worldDescriptor.VoxelAtlas.VoxelVariantsSideTextureIndex.Length; i++)
                _voxelVariantsSideTextureIndex[i] = _worldDescriptor.VoxelAtlas.VoxelVariantsSideTextureIndex[i];
        }

        private void DisposeAtlasData()
        {
            _voxelVariantIndicesToUvPositions.Dispose();
            _voxelIndexToVoxelVariantsStartIndexAndCount.Dispose();
            _voxelVariantsTopTextureIndex.Dispose();
            _voxelVariantsBottomTextureIndex.Dispose();
            _voxelVariantsSideTextureIndex.Dispose();
        }
        
        private void PrepareChunks()
        {
            if (_activeChunkViews != null)
                foreach (var chunkView in _activeChunkViews.Values)
                {
                    Destroy(chunkView.Mesh);
                    Destroy(chunkView.MeshFilter.gameObject);
                }
            
            if (_freeChunkViews != null)
                foreach (var chunkView in _freeChunkViews)
                {
                    Destroy(chunkView.Mesh);
                    Destroy(chunkView.MeshFilter.gameObject);
                }
            
            _activeChunkViews = new Dictionary<Vector3Int, ChunkView>();
            _freeChunkViews = new Queue<ChunkView>();
            
            _chunks ??= new Dictionary<Vector3Int, VoxelTerrainChunk>();
            
            var atlasMaterials = _worldDescriptor.VoxelAtlas.AtlasMaterials;
            var t = 2f * _renderDistance + 1;
            for (var i = 0; i < t * t * t * 2; i++)
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
                
                meshRenderer.SetMaterials(atlasMaterials);

                meshFilter.sharedMesh = mesh;
                meshCollider.sharedMesh = null;
                _freeChunkViews.Enqueue(new ChunkView
                {
                    Mesh = mesh,
                    MeshFilter = meshFilter,
                    MeshRenderer = meshRenderer,
                    MeshCollider = meshCollider
                });
            }
        }

        private void PregenerateChunks()
        {
            var origin = _worldDescriptor.PregenerationOriginPosition;
            var size = _worldDescriptor.PregenerationSize;
            for (var x = origin.x - size.x; x <= origin.x + size.x; x++)
            for (var y = origin.y - size.y; y <= origin.y + size.y; y++)
            for (var z = origin.z - size.z; z <= origin.z + size.z; z++)
                GenerateChunk(new Vector3Int(x, y, z));
        }

        private VoxelTerrainChunk GenerateChunk(Vector3Int chunkPosition)
        {
            var chunk = new VoxelTerrainChunk(_worldDescriptor.ChunkSize);
            var chunkOffset = new Vector3(
                chunkPosition.x * _worldDescriptor.VoxelSize.x * _worldDescriptor.ChunkSize.x,
                chunkPosition.y * _worldDescriptor.VoxelSize.y * _worldDescriptor.ChunkSize.y,
                chunkPosition.z * _worldDescriptor.VoxelSize.z * _worldDescriptor.ChunkSize.z
            );

            for (var x = 0; x < _worldDescriptor.ChunkSize.x; x++)
            for (var y = 0; y < _worldDescriptor.ChunkSize.y; y++)
            for (var z = 0; z < _worldDescriptor.ChunkSize.z; z++)
            {
                var chunkVoxelPosition = new Vector3Int(x, y, z);
                var globalVoxelPosition = new Vector3Int(x, y, z) + chunkOffset;
                
                var voxelBiomeState = _worldDescriptor.BiomeMapGenerator.GetVoxelBiomeState(_worldDescriptor.Seed, globalVoxelPosition);
                var bestBiome = voxelBiomeState.BestBiome;
                var bestVoxel = bestBiome.GetVoxel(_worldDescriptor.Seed, globalVoxelPosition);
                var noiseSum = 0f;
                for (var i = 0; i < voxelBiomeState.AllBiomes.Length; i++)
                    noiseSum += voxelBiomeState.AllBiomes[i].Weight
                                * voxelBiomeState.AllBiomes[i].BiomeDescriptor.LandscapeNoise.GetNoiseWithSeed(_worldDescriptor.Seed, globalVoxelPosition.x, globalVoxelPosition.y, globalVoxelPosition.z);
                noiseSum /= voxelBiomeState.AllBiomes.Length;

                if (bestBiome.IsVoxelExists(globalVoxelPosition, noiseSum)) 
                    chunk.SetVoxel(chunkVoxelPosition, (byte)_worldDescriptor.VoxelDatabase.GetIndexOf(bestVoxel));
            }

            _chunks.Add(chunkPosition, chunk);
            return chunk;
        }

        private ChunkView ActivateChunk(Vector3Int chunkPosition)
        {
            var chunkView = _freeChunkViews.Dequeue();
            chunkView.MeshRenderer.enabled = true;
            _activeChunkViews.Add(chunkPosition, chunkView);
            chunkView.Mesh = GenerateChunkMesh(chunkView.Mesh, chunkPosition, out var meshVerticesCount);
            chunkView.MeshCollider.sharedMesh = chunkView.Mesh;
            chunkView.MeshCollider.enabled = meshVerticesCount > 0;
            var chunkOffset = new Vector3(
                chunkPosition.x * _worldDescriptor.VoxelSize.x * _worldDescriptor.ChunkSize.x,
                chunkPosition.y * _worldDescriptor.VoxelSize.y * _worldDescriptor.ChunkSize.y,
                chunkPosition.z * _worldDescriptor.VoxelSize.z * _worldDescriptor.ChunkSize.z
            );
            chunkView.MeshRenderer.transform.position = chunkOffset;
            return chunkView;
        }
        
        private void DeactivateChunk(Vector3Int chunkPosition)
        {
            var chunkView = _activeChunkViews[chunkPosition];
            chunkView.MeshRenderer.enabled = false;
            chunkView.MeshCollider.enabled = false;
            chunkView.MeshCollider.sharedMesh = null;
            _activeChunkViews.Remove(chunkPosition);
            _freeChunkViews.Enqueue(chunkView);
        }

        private VoxelTerrainChunk GetOrGenerateChunk(Vector3Int chunkPosition)
        {
            return _chunks.TryGetValue(chunkPosition, out var chunk) ? chunk : GenerateChunk(chunkPosition);
        }

        private Mesh GenerateChunkMesh(Mesh mesh, Vector3Int chunkPosition, out int meshVerticesCount)
        {
            // Calculate mesh parameters
            var calculateMeshParametersJob = new CalculateMeshParametersJob()
            {
                CurrentVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition),
                ChunkSize = new int3(_worldDescriptor.ChunkSize.x, _worldDescriptor.ChunkSize.y, _worldDescriptor.ChunkSize.z),
                TopNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(0, 1, 0)),
                BottomNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(0, -1, 0)),
                LeftNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(-1, 0, 0)),
                RightNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(1, 0, 0)),
                FrontNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(0, 0, 1)),
                BackNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(0, 0, -1)),
                ResultVerticesCount = new NativeArray<int>(6, Allocator.TempJob),
                ResultTriangleIndicesCount = new NativeArray<int>(6, Allocator.TempJob)
            };
            var calculateMeshParametersJobHandle = calculateMeshParametersJob.Schedule(6, 1);
            calculateMeshParametersJobHandle.Complete();

            // Combine results
            var verticesCount = 0;
            for (int i = 0; i < 6; i++) 
                verticesCount += calculateMeshParametersJob.ResultVerticesCount[i];
            
            var triangleIndicesCount = 0;
            for (int i = 0; i < 6; i++) 
                triangleIndicesCount += calculateMeshParametersJob.ResultTriangleIndicesCount[i];
            
            // Initialize mesh data for generation
            var meshDataArray = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArray[0];

            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
                4, Allocator.Temp, NativeArrayOptions.UninitializedMemory
            );

            vertexAttributes[0] = new VertexAttributeDescriptor(
                VertexAttribute.Position, dimension: 3, stream: 0
            );
            vertexAttributes[1] = new VertexAttributeDescriptor(
                VertexAttribute.Normal, dimension: 3, stream: 1
            );
            vertexAttributes[2] = new VertexAttributeDescriptor(
                VertexAttribute.Tangent, dimension: 4, stream: 2
            );
            vertexAttributes[3] = new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0, dimension: 2, stream: 3
            );
            
            meshData.SetVertexBufferParams(verticesCount, vertexAttributes);
            vertexAttributes.Dispose();
            
            meshData.SetIndexBufferParams(triangleIndicesCount, IndexFormat.UInt16);

            // Calculate mesh grid
            var chunkPositionInt3 = new int3(chunkPosition.x, chunkPosition.y, chunkPosition.z);
            var chunkSize = new int3(_worldDescriptor.ChunkSize.x, _worldDescriptor.ChunkSize.y, _worldDescriptor.ChunkSize.z);
            var voxelSize = new float3(_worldDescriptor.VoxelSize.x, _worldDescriptor.VoxelSize.y, _worldDescriptor.VoxelSize.z);
            var chunkGlobalSize = chunkSize * voxelSize;
            var chunkGlobalOffset = chunkPositionInt3 * chunkGlobalSize;
            var calculateMeshGridJob = new CalculateMeshGridJob
            {
                CurrentVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition),
                ChunkSize = chunkSize,
                VoxelSize = voxelSize,
                TopNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(0, 1, 0)),
                BottomNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(0, -1, 0)),
                LeftNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(-1, 0, 0)),
                RightNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(1, 0, 0)),
                FrontNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(0, 0, 1)),
                BackNeighbourVoxelTerrainChunk = GetOrGenerateChunk(chunkPosition + new Vector3Int(0, 0, -1)),
                VerticesCount = calculateMeshParametersJob.ResultVerticesCount,
                TriangleIndicesCount = calculateMeshParametersJob.ResultTriangleIndicesCount,
                Positions = meshData.GetVertexData<float3>(0),
                Normals = meshData.GetVertexData<float3>(1),
                Tangents = meshData.GetVertexData<float4>(2),
                TexCoords = meshData.GetVertexData<float2>(3),
                TriangleIndices = meshData.GetIndexData<ushort>(),
                VoxelVariantIndicesToUvPositions = _voxelVariantIndicesToUvPositions,
                UvSize = _worldDescriptor.VoxelAtlas.TextureSizeInAtlas,
                VoxelIndexToVoxelVariantsStartIndexAndCount = _voxelIndexToVoxelVariantsStartIndexAndCount,
                VoxelVariantsTopTextureIndex = _voxelVariantsTopTextureIndex,
                VoxelVariantsBottomTextureIndex = _voxelVariantsBottomTextureIndex,
                VoxelVariantsSideTextureIndex = _voxelVariantsSideTextureIndex
            };
            var calculateMeshGridJobHandle = calculateMeshGridJob.Schedule(6, 1);
            calculateMeshGridJobHandle.Complete();
            
            // Dispose job results
            calculateMeshParametersJob.Dispose();
            
            // Set submesh
            meshData.subMeshCount = 1;
            var bounds = new Bounds(chunkGlobalSize + chunkGlobalSize / 2f, chunkGlobalSize);
            
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleIndicesCount)
            {
                vertexCount = verticesCount,
                bounds = bounds
            });

            // Apply mesh data
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
            calculateMeshGridJob.Dispose();
            
            mesh.RecalculateBounds();

            meshVerticesCount = verticesCount;
                    
            return mesh;
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