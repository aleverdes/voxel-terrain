using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AleVerDes.VoxelTerrain
{
    [SelectionBase]
    [ExecuteInEditMode]
    public class World : MonoBehaviour
    {
        [SerializeField] private WorldSettings _worldSettings;

        [HideInInspector] [SerializeField] private Block[] _blocks;
        [HideInInspector] [SerializeField] private Chunk[] _chunks;

        [HideInInspector] public WorldTool LastWorldTool = WorldTool.None;
        
        [ContextMenu("Setup")]
        public void Setup()
        {
            Clear();
            SetupChunks();
            SetupBlocks();
            GenerateChunkMeshes();
        }

        private void Clear()
        {
            _blocks = null;
            foreach (var chunk in _chunks)
            {
                if (!chunk)
                {
                    continue;
                }
                
                if (Application.isPlaying)
                {
                    Destroy(chunk.gameObject);
                }
                else
                {
                    DestroyImmediate(chunk.gameObject);
                }
            }

            _chunks = null;
        }

        private void SetupChunks()
        {
            var xSize = _worldSettings.WorldSize.x / _worldSettings.ChunkSize.x;
            var zSize = _worldSettings.WorldSize.z / _worldSettings.ChunkSize.y;

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

        private void SetupBlocks()
        {
            _blocks = new Block[_worldSettings.WorldSize.x * _worldSettings.WorldSize.y * _worldSettings.WorldSize.z];
            for (var x = 0; x < _worldSettings.WorldSize.x; x++)
            {
                for (var y = 0; y < _worldSettings.WorldSize.y; y++)
                {
                    for (var z = 0; z < _worldSettings.WorldSize.z; z++)
                    {
                        ref var block = ref GetBlock(x, y, z);
                        block.Void = true;

                        block.Position = new Vector3Int(x, y, z);
                        
                        block.Top = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, _worldSettings.WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Bottom = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, _worldSettings.WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Left = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, _worldSettings.WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Right = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, _worldSettings.WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Forward = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, _worldSettings.WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Back = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, _worldSettings.WorldAtlas.Layers[0].Textures.Length)
                        };
                        
                        block.TopForwardRightVertexHeight = 1f;
                        block.TopForwardLeftVertexHeight = 1f;
                        block.TopBackRightVertexHeight = 1f;
                        block.TopBackLeftVertexHeight = 1f;
                    }
                }
            }

            var halfHeight = _worldSettings.WorldSize.y / 2;
            for (var x = 0; x < _worldSettings.WorldSize.x; x++)
            {
                for (var y = 0; y < halfHeight; y++)
                {
                    for (var z = 0; z < _worldSettings.WorldSize.z; z++)
                    {
                        ref var block = ref GetBlock(x, y, z);
                        block.Void = false;
                    }
                }
            }
        }

        public void GenerateChunkMeshes()
        {
            foreach (var chunk in _chunks)
            {
                chunk.GenerateMesh();
            }
        }

        public ref Block GetBlock(Vector3Int position)
        {
            return ref GetBlock(position.x, position.y, position.z);
        }

        public ref Block GetBlock(int x, int y, int z)
        {
            try
            {
                return ref _blocks[x + y * _worldSettings.WorldSize.x + z * _worldSettings.WorldSize.x * _worldSettings.WorldSize.y];
            }
            catch (Exception e)
            { 
                Debug.LogError($"Index out of bounds: x[{x}], y[{y}], z[{z}]; world ({_worldSettings.WorldSize.x}, {_worldSettings.WorldSize.y}, {_worldSettings.WorldSize.z}); formula is {x + y * _worldSettings.WorldSize.x + z * _worldSettings.WorldSize.x * _worldSettings.WorldSize.y} (max is {_worldSettings.WorldSize.x * _worldSettings.WorldSize.y * _worldSettings.WorldSize.z})");
                throw;
            }
        }
        
        public WorldSettings WorldSettings => _worldSettings;
    }
}