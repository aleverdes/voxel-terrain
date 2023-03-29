using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AffenCode.VoxelTerrain
{
    [SelectionBase]
    [ExecuteInEditMode]
    public class World : MonoBehaviour
    {
        [Header("World Setting")] 
        public Vector3Int WorldSize = new Vector3Int(128, 32, 128);
        public Vector2Int ChunkSize = new Vector2Int(16, 16);
        public float BlockSize = 1f;
        public Material WorldMaterial;
        public Atlas WorldAtlas;

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
            var xSize = WorldSize.x / ChunkSize.x;
            var zSize = WorldSize.z / ChunkSize.y;

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
                    chunk.Setup(this, new Vector2Int(x * ChunkSize.x, z * ChunkSize.y));
                    _chunks[x + z * xSize] = chunk;
                }
            }
        }

        private void SetupBlocks()
        {
            _blocks = new Block[WorldSize.x * WorldSize.y * WorldSize.z];
            for (int x = 0; x < WorldSize.x; x++)
            {
                for (int y = 0; y < WorldSize.y; y++)
                {
                    for (int z = 0; z < WorldSize.z; z++)
                    {
                        ref var block = ref GetBlock(x, y, z);
                        block.Void = true;

                        block.Position = new Vector3Int(x, y, z);
                        
                        block.Top = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Bottom = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Left = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Right = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Forward = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Back = new BlockFace()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                    }
                }
            }

            var halfHeight = WorldSize.y / 2;
            for (int x = 0; x < WorldSize.x; x++)
            {
                for (int y = 0; y < halfHeight; y++)
                {
                    for (int z = 0; z < WorldSize.z; z++)
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
                return ref _blocks[x + y * WorldSize.x + z * WorldSize.x * WorldSize.y];
            }
            catch (Exception e)
            { 
                Debug.LogError($"Index out of bounds: x[{x}], y[{y}], z[{z}]; world ({WorldSize.x}, {WorldSize.y}, {WorldSize.z}); formula is {x + y * WorldSize.x + z * WorldSize.x * WorldSize.y} (max is {WorldSize.x * WorldSize.y * WorldSize.z})");
                throw;
            }
        }
    }
}