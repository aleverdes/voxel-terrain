using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AleVerDes.VoxelTerrain
{
    [SelectionBase]
    [ExecuteInEditMode]
    public class World : MonoBehaviour
    {
        [SerializeField] private WorldSettings _worldSettings;

        [HideInInspector] [SerializeField] private Chunk[] _chunks;
        [HideInInspector] [SerializeField] private byte[] _cellTextures;
        [HideInInspector] [SerializeField] private float[] _verticesHeights;
        [HideInInspector] [SerializeField] private List<int> _avoidedCells = new List<int>();

        [HideInInspector] public WorldTool LastWorldTool = WorldTool.None;
        
        [ContextMenu("Setup")]
        public void Setup()
        {
            Clear();
            SetupChunks();
            SetupCellTextures();
            SetupVerticesHeights();
            GenerateChunkMeshes();
        }

        private void Clear()
        {
            _cellTextures = null;
            _verticesHeights = null;
            _avoidedCells.Clear();
            
            foreach (var chunk in _chunks)
            {
                if (!chunk)
                    continue;

                if (Application.isPlaying)
                    Destroy(chunk.gameObject);
                else
                    DestroyImmediate(chunk.gameObject);
            }

            _chunks = null;
        }

        private void SetupChunks()
        {
            var xSize = _worldSettings.WorldSize.x / _worldSettings.ChunkSize.x;
            var zSize = _worldSettings.WorldSize.y / _worldSettings.ChunkSize.y;

            _chunks = new Chunk[xSize * zSize];

            for (int x = 0; x < xSize; x++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    var chunkObject = new GameObject($"Chunk [{x}, {z}]");
                    chunkObject.transform.SetParent(transform);
                    chunkObject.transform.localPosition = Vector3.zero;
                    chunkObject.transform.localRotation = Quaternion.identity;
                    chunkObject.transform.localScale = Vector3.one;

                    var chunk = chunkObject.AddComponent<Chunk>();
                    chunk.Setup(this, new Vector2Int(x * _worldSettings.ChunkSize.x, z * _worldSettings.ChunkSize.y));
                    _chunks[x + z * xSize] = chunk;
                }
            }
        }

        private void SetupCellTextures()
        {
            _cellTextures = new byte[_worldSettings.WorldSize.x * _worldSettings.WorldSize.y];
            for (var x = 0; x < _worldSettings.WorldSize.x; x++)
            {
                for (var z = 0; z < _worldSettings.WorldSize.y; z++)
                {
                    ref var cellTexture = ref GetCellTexture(x, z);
                    cellTexture = (byte)Random.Range(0, _worldSettings.WorldAtlas.Layers[0].Textures.Length);
                }
            }
        }

        private void SetupVerticesHeights()
        {
            _verticesHeights = new float[(_worldSettings.WorldSize.x + 1) * (_worldSettings.WorldSize.y + 1)];
            for (var x = 0; x < _worldSettings.WorldSize.x; x++)
            {
                for (var z = 0; z < _worldSettings.WorldSize.y; z++)
                {
                    ref var vertex = ref GetVertexHeight(x, z);
                    vertex = 0;
                }
            }
        }

        public void GenerateChunkMeshes()
        {
            foreach (var chunk in _chunks) 
                chunk.GenerateMesh();
        }

        public void GenerateChunkMeshes(IEnumerable<Vector2Int> affectedBlockPositions)
        {
            var blocks = new HashSet<Vector2Int>();
            foreach (var affectedBlockPosition in affectedBlockPositions)
            {
                blocks.Add(affectedBlockPosition);
                foreach (var neighbour in VoxelTerrainUtils.GetNeighbours(affectedBlockPosition)) 
                    blocks.Add(neighbour);
            }

            var affectedChunks = new HashSet<Chunk>();
            foreach (var affectedBlockPosition in blocks)
            {
                var chunk = GetChunk(affectedBlockPosition);
                if (chunk) 
                    affectedChunks.Add(chunk);
            }

            foreach (var affectedChunk in affectedChunks) 
                affectedChunk.GenerateMesh();
        }
        
        public Chunk GetChunk(Vector2Int position)
        {
            foreach (var chunk in _chunks)
                if (chunk.Rect.Contains(new Vector2Int(position.x, position.y)))
                    return chunk;

            return null;
        }

        public int GetCellIndex(Vector2Int position)
        {
            return GetCellTexture(position.x, position.y);
        }

        public int GetCellIndex(int x, int z)
        {
            try
            {
                return x + z * _worldSettings.WorldSize.x;
            }
            catch (Exception e)
            { 
                Debug.LogError($"Block index out of bounds: x[{x}], z[{z}]; world ({_worldSettings.WorldSize.x}, {_worldSettings.WorldSize.y}); formula is {x + z * _worldSettings.WorldSize.x} (max is {_worldSettings.WorldSize.x * _worldSettings.WorldSize.y})");
                throw;
            }
        }

        public ref byte GetCellTexture(Vector2Int position)
        {
            return ref GetCellTexture(position.x, position.y);
        }

        public ref byte GetCellTexture(int x, int z)
        {
            try
            {
                return ref _cellTextures[GetCellIndex(x, z)];
            }
            catch (Exception e)
            { 
                Debug.LogError($"Cell texture index out of bounds: x[{x}], z[{z}]; world ({_worldSettings.WorldSize.x}, {_worldSettings.WorldSize.y}); formula is {x + z * _worldSettings.WorldSize.x} (max is {_worldSettings.WorldSize.x * _worldSettings.WorldSize.y})");
                throw;
            }
        }

        public int GetVertexIndex(Vector2Int position)
        {
            return GetVertexIndex(position.x, position.y);
        }

        public int GetVertexIndex(int x, int z)
        {
            try
            {
                return x + z * (_worldSettings.WorldSize.x + 1);
            }
            catch (Exception e)
            { 
                Debug.LogError($"Vertex index out of bounds: x[{x}], z[{z}]; world ({_worldSettings.WorldSize.x}, {_worldSettings.WorldSize.y}); formula is {x + z * (_worldSettings.WorldSize.x + 1)} (max is {(_worldSettings.WorldSize.x + 1) * (_worldSettings.WorldSize.y + 1)})");
                throw;
            }
        }

        public ref float GetVertexHeight(Vector2Int position)
        {
            return ref GetVertexHeight(position.x, position.y);
        }

        public ref float GetVertexHeight(int x, int z)
        {
            try
            {
                return ref _verticesHeights[GetVertexIndex(x, z)];
            }
            catch (Exception e)
            { 
                Debug.LogError($"Vertex index out of bounds: x[{x}], z[{z}]; world ({_worldSettings.WorldSize.x}, {_worldSettings.WorldSize.y}); formula is {x + z * (_worldSettings.WorldSize.x + 1)} (max is {(_worldSettings.WorldSize.x + 1) * (_worldSettings.WorldSize.y + 1)})");
                throw;
            }
        }
        
        public bool IsCellAvoided(int index)
        {
            return _avoidedCells.Contains(index);
        }
        
        public void SetCellAvoided(int index, bool value)
        {
            if (value)
            {
                if (!_avoidedCells.Contains(index))
                {
                    _avoidedCells.Add(index);
                }
            }
            else
            {
                if (_avoidedCells.Contains(index))
                {
                    _avoidedCells.Remove(index);
                }
            }
        }
        
        public WorldSettings WorldSettings => _worldSettings;
    }
}