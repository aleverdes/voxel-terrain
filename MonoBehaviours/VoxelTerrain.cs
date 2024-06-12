using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace TaigaGames.Voxels
{
    public class VoxelTerrain : MonoBehaviour
    {
        [SerializeField] private WorldDescriptor _worldDescriptor;
        
        [Header("Generation Settings")]
        [SerializeField] private Transform _generationOrigin;
        [SerializeField] private bool _generationByAngle;
        
        [Header("Render Settings")]
        [SerializeField] private int _renderDistance = 1;
        [SerializeField] private float _renderAngle = 60f;
        [SerializeField] private float _backwardRenderDistance = 1f;
        
        private Dictionary<Vector3Int, VoxelTerrainChunk> _chunksData;
        private HashSet<Vector3Int> _generatedChunks;
            
        private Dictionary<Vector3Int, ChunkView> _activeChunkViews;
        private Queue<ChunkView> _freeChunkViews;
        
        private readonly HashSet<Vector3Int> _toDeactivateChunks = new();
        private readonly HashSet<Vector3Int> _toActivateChunks = new();
        
        private NativeArray<float2> _voxelVariantIndicesToUvPositions;
        private NativeArray<int2> _voxelIndexToVoxelVariantsStartIndexAndCount;
        private NativeArray<int> _voxelVariantsTopTextureIndex;
        private NativeArray<int> _voxelVariantsBottomTextureIndex;
        private NativeArray<int> _voxelVariantsSideTextureIndex;
        private readonly Dictionary<VoxelDescriptor, byte> _voxelToIndex = new();
        
        private readonly List<Task<(Vector3Int, VoxelTerrainChunk)>> _chunkGenerationTasks = new();
        private readonly List<Vector3Int> _toGenerate = new();
        private float _lastGenerationTime;
        private float _chunkDiagonal;
        
        private void Awake()
        {
            PrepareAtlasData();
            PrepareVoxelsData();
            PrepareChunks();
        }

        private void OnDestroy()
        {
            DisposeAtlasData();
        }

        private void Update()
        {
            _toDeactivateChunks.Clear();
            foreach (var chunkPosition in _chunksData.Keys)
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

                if (!_worldDescriptor.IsInfinite)
                {
                    var ws = _worldDescriptor.WorldSizeInChunks;
                    if (chunkPosition.x < 0 || chunkPosition.x >= ws.x || chunkPosition.y < 0 || chunkPosition.y >= ws.y || chunkPosition.z < 0 || chunkPosition.z >= ws.z)
                    {
                        StartGenerationEmptyChunkTask(chunkPosition);
                        continue;
                    }
                }
                
                var chunkGlobalCenterPosition = new Vector3(
                    (chunkPosition.x + 0.5f) * _worldDescriptor.ChunkSize.x * _worldDescriptor.VoxelSize.x ,
                    (chunkPosition.y + 0.5f) * _worldDescriptor.ChunkSize.y * _worldDescriptor.VoxelSize.y,
                    (chunkPosition.z + 0.5f) * _worldDescriptor.ChunkSize.z * _worldDescriptor.VoxelSize.z
                );
                var backwardGenerationOrigin = _generationOrigin.position - _generationOrigin.forward * (_chunkDiagonal * _backwardRenderDistance);
                if (_generationByAngle && Vector3.Angle(_generationOrigin.forward, chunkGlobalCenterPosition - backwardGenerationOrigin) > _renderAngle)
                    continue;
                
                if (!_chunksData.ContainsKey(chunkPosition)) 
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
            
            if (_chunkGenerationTasks.Count > 0)
                for (var i = _chunkGenerationTasks.Count - 1; i >= 0; i--)
                    if (_chunkGenerationTasks[i].IsCompleted)
                    {
                        var (chunkPosition, chunk) = _chunkGenerationTasks[i].Result;
                        _chunksData.Add(chunkPosition, chunk);
                        if (_toActivateChunks.Contains(chunkPosition))
                            ActivateChunk(chunkPosition);
                        _chunkGenerationTasks.RemoveAt(i);
                    }
                

            if (Time.unscaledTime - _lastGenerationTime > 0.1f)
            {
                _lastGenerationTime = Time.unscaledTime;
                
                var chunkSize = new int3(_worldDescriptor.ChunkSize.x, _worldDescriptor.ChunkSize.y, _worldDescriptor.ChunkSize.z);
                var voxelSize = new float3(_worldDescriptor.VoxelSize.x, _worldDescriptor.VoxelSize.y, _worldDescriptor.VoxelSize.z);
                var chunkGlobalSize = chunkSize * voxelSize;
                    
                foreach (var chunkPosition in _toGenerate.OrderBy(chunkPosition => math.distance(_generationOrigin.position, new int3(chunkPosition.x, chunkPosition.y, chunkPosition.z) * chunkGlobalSize + 0.5f * chunkGlobalSize)))
                    StartGenerationChunkTask(chunkPosition);
                _toGenerate.Clear();
            }
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

        private void PrepareVoxelsData()
        {
            for (var i = 0; i < _worldDescriptor.VoxelDatabase.GetCount(); i++)
                _voxelToIndex.Add(_worldDescriptor.VoxelDatabase[i], (byte) i);
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
            
            
            _chunkDiagonal = new Vector3(
                _worldDescriptor.ChunkSize.x * _worldDescriptor.VoxelSize.x,
                _worldDescriptor.ChunkSize.y * _worldDescriptor.VoxelSize.y,
                _worldDescriptor.ChunkSize.z * _worldDescriptor.VoxelSize.z
            ).magnitude;
            
            _activeChunkViews = new Dictionary<Vector3Int, ChunkView>();
            _freeChunkViews = new Queue<ChunkView>();
            
            _chunksData ??= new Dictionary<Vector3Int, VoxelTerrainChunk>();
            _generatedChunks ??= new HashSet<Vector3Int>();

            if (_worldDescriptor.IsInfinite)
            {
                var rd = 2f * _renderDistance + 1;
                var t = rd * rd * rd * 2;
                for (var i = 0; i < t; i++)
                    PrepareChunk(i);
            }
            else
            {
                var ws = _worldDescriptor.WorldSizeInChunks.x * _worldDescriptor.WorldSizeInChunks.y * _worldDescriptor.WorldSizeInChunks.z;
                var rd = 2f * _renderDistance + 1;
                var t = Mathf.Max(rd * rd * rd * 2, ws);
                for (var i = 0; i < t; i++)
                    PrepareChunk(i);
            }

            return;
            
            void PrepareChunk(int i)
            {
                var chunkGameObject = new GameObject("Chunk #" + i);
                chunkGameObject.transform.SetParent(transform);
                chunkGameObject.hideFlags = HideFlags.HideInHierarchy;
                
                var mesh = new Mesh
                {
                    name = "Chunk #" + i
                };
                var meshFilter = chunkGameObject.AddComponent<MeshFilter>();
                var meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
                var meshCollider = chunkGameObject.AddComponent<MeshCollider>();
                
                meshRenderer.SetMaterials(_worldDescriptor.VoxelAtlas.AtlasMaterials);

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

        private void GenerateChunk(Vector3Int chunkPosition)
        {
            if (!_generatedChunks.Add(chunkPosition)) return;
            _toGenerate.Add(chunkPosition);
        }

        private void StartGenerationEmptyChunkTask(Vector3Int chunkPosition)
        {
            if (_generatedChunks.Add(chunkPosition))
                _chunksData.Add(chunkPosition, new VoxelTerrainChunk(_worldDescriptor.ChunkSize));
        }
        
        private void StartGenerationChunkTask(Vector3Int chunkPosition)
        {
            var task = Task.Factory.StartNew<(Vector3Int, VoxelTerrainChunk)>(() =>
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
                    var globalVoxelPosition = chunkOffset + new Vector3(
                            chunkVoxelPosition.x * _worldDescriptor.VoxelSize.x,
                            chunkVoxelPosition.y * _worldDescriptor.VoxelSize.y,
                            chunkVoxelPosition.z * _worldDescriptor.VoxelSize.z);
                
                    var voxelBiomeState = _worldDescriptor.BiomeMapGenerator.GetVoxelBiomeState(_worldDescriptor.Seed, globalVoxelPosition);
                    
                    var bestBiome = voxelBiomeState.BestBiome;
                    var bestVoxel = bestBiome.GetVoxel(_worldDescriptor.Seed, globalVoxelPosition);

                    var noiseSum = 0f;
                    for (var i = 0; i < voxelBiomeState.AllBiomes.Length; i++)
                    {
                        var biome = voxelBiomeState.AllBiomes[i];
                        noiseSum += biome.Weight
                                    * biome.BiomeDescriptor.LandscapeNoise.GetNoiseWithSeed(_worldDescriptor.Seed, globalVoxelPosition.x, globalVoxelPosition.y, globalVoxelPosition.z);
                    }
                    
                    if (bestBiome.IsVoxelExists(globalVoxelPosition, noiseSum / voxelBiomeState.AllBiomes.Length))
                        chunk.SetVoxel(chunkVoxelPosition, _voxelToIndex[bestVoxel]);
                }

                return (chunkPosition, chunk);
            });
            _chunkGenerationTasks.Add(task);
            _generatedChunks.Add(chunkPosition);
        }

        private void ActivateChunk(Vector3Int chunkPosition)
        {
            if (!_chunksData.ContainsKey(chunkPosition))
            {
                GenerateChunk(chunkPosition);
                return;
            }
            
            if (!_chunksData.ContainsKey(chunkPosition + Vector3Int.left))
            {
                GenerateChunk(chunkPosition + Vector3Int.left);
                return;
            }
            
            if (!_chunksData.ContainsKey(chunkPosition + Vector3Int.right))
            {
                GenerateChunk(chunkPosition + Vector3Int.right);
                return;
            }
            
            if (!_chunksData.ContainsKey(chunkPosition + Vector3Int.forward))
            {
                GenerateChunk(chunkPosition + Vector3Int.forward);
                return;
            }
            
            if (!_chunksData.ContainsKey(chunkPosition + Vector3Int.back))
            {
                GenerateChunk(chunkPosition + Vector3Int.back);
                return;
            }

            if (!_chunksData.ContainsKey(chunkPosition + Vector3Int.up))
            {
                GenerateChunk(chunkPosition + Vector3Int.up);
                return;
            }
            
            if (!_chunksData.ContainsKey(chunkPosition + Vector3Int.down))
            {
                GenerateChunk(chunkPosition + Vector3Int.down);
                return;
            }
            
            var chunkView = _freeChunkViews.Dequeue();
            _activeChunkViews.Add(chunkPosition, chunkView);
            chunkView.Mesh = GenerateChunkMesh(chunkView.Mesh, chunkPosition, out var meshVerticesCount);
            chunkView.MeshRenderer.enabled = true;
            chunkView.MeshCollider.sharedMesh = chunkView.Mesh;
            chunkView.MeshCollider.enabled = meshVerticesCount > 0;
            chunkView.MeshRenderer.gameObject.SetActive(true);
            chunkView.MeshRenderer.gameObject.name = "Chunk " + chunkPosition;
            var chunkOffset = new Vector3(
                chunkPosition.x * _worldDescriptor.VoxelSize.x * _worldDescriptor.ChunkSize.x,
                chunkPosition.y * _worldDescriptor.VoxelSize.y * _worldDescriptor.ChunkSize.y,
                chunkPosition.z * _worldDescriptor.VoxelSize.z * _worldDescriptor.ChunkSize.z
            );
            chunkView.MeshRenderer.transform.position = chunkOffset;
        }
        
        private void DeactivateChunk(Vector3Int chunkPosition)
        {
            var chunkView = _activeChunkViews[chunkPosition];
            chunkView.MeshRenderer.enabled = false;
            chunkView.MeshCollider.enabled = false;
            chunkView.MeshCollider.sharedMesh = null;
            chunkView.MeshRenderer.gameObject.SetActive(false);
            chunkView.MeshRenderer.gameObject.name = "Inactive Chunk " + chunkPosition;
            _activeChunkViews.Remove(chunkPosition);
            _freeChunkViews.Enqueue(chunkView);
        }

        private Mesh GenerateChunkMesh(Mesh mesh, Vector3Int chunkPosition, out int meshVerticesCount)
        {
            // Calculate mesh parameters
            var calculateMeshParametersJob = new CalculateMeshParametersJob()
            {
                CurrentVoxelTerrainChunk = _chunksData[chunkPosition],
                ChunkSize = new int3(_worldDescriptor.ChunkSize.x, _worldDescriptor.ChunkSize.y, _worldDescriptor.ChunkSize.z),
                TopNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(0, 1, 0)],
                BottomNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(0, -1, 0)],
                LeftNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(-1, 0, 0)],
                RightNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(1, 0, 0)],
                FrontNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(0, 0, 1)],
                BackNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(0, 0, -1)],
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
            var chunkSize = new int3(_worldDescriptor.ChunkSize.x, _worldDescriptor.ChunkSize.y, _worldDescriptor.ChunkSize.z);
            var voxelSize = new float3(_worldDescriptor.VoxelSize.x, _worldDescriptor.VoxelSize.y, _worldDescriptor.VoxelSize.z);
            var chunkGlobalSize = chunkSize * voxelSize;
            
            var calculateMeshGridJob = new CalculateMeshGridJob
            {
                CurrentVoxelTerrainChunk = _chunksData[chunkPosition],
                ChunkSize = chunkSize,
                VoxelSize = voxelSize,
                TopNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(0, 1, 0)],
                BottomNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(0, -1, 0)],
                LeftNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(-1, 0, 0)],
                RightNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(1, 0, 0)],
                FrontNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(0, 0, 1)],
                BackNeighbourVoxelTerrainChunk = _chunksData[chunkPosition + new Vector3Int(0, 0, -1)],
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