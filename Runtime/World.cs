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
        public float BlockSize = 1f;
        public Material WorldMaterial;
        public Atlas WorldAtlas;

        [Header("Mesh")]
        [SerializeField] private Mesh _mesh;
        [SerializeField] private GameObject _meshObject;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private MeshCollider _meshCollider;

        [Header("Blocks")] 
        [HideInInspector] [SerializeField] private Block[] _blocks;
        
        public Mesh Mesh => _mesh;
        public GameObject MeshObject => _meshObject;
        public MeshFilter MeshFilter => _meshFilter;
        public MeshRenderer MeshRenderer => _meshRenderer;
        public MeshCollider MeshCollider => _meshCollider;

        [ContextMenu("Setup")]
        public void Setup()
        {
            SetupComponents();
            ResetMesh();
        }

        public void ResetMesh()
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
                        
                        block.Top = new Face()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Bottom = new Face()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Left = new Face()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Right = new Face()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Forward = new Face()
                        {
                            Draw = true,
                            LayerIndex = 0,
                            LayerTextureIndex = (byte)Random.Range(0, WorldAtlas.Layers[0].Textures.Length)
                        };
                        block.Back = new Face()
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
            
            _mesh = new Mesh();
            GenerateMesh();
            _meshFilter.sharedMesh = _mesh;
        }

        public void SetupComponents()
        {
            if (_meshObject)
            {
                return;
            }

            _meshObject = new GameObject("World Mesh");
            _meshObject.transform.SetParent(transform);
            _meshObject.transform.localPosition = Vector3.zero;
            _meshObject.transform.localRotation = Quaternion.identity;
            _meshObject.transform.localScale = Vector3.one;

            _meshFilter = _meshObject.AddComponent<MeshFilter>();
            _meshCollider = _meshObject.AddComponent<MeshCollider>();
            _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
            _meshRenderer.sharedMaterial = WorldMaterial;
        }

        public void GenerateMesh()
        {
            var xSize = WorldSize.x;
            var ySize = WorldSize.y;
            var zSize = WorldSize.z;

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tangents = new List<Vector4>();
            var triangles = new List<int>();
            
            var upTangent = new Vector4(1f, 0f, 0f, -1f);

            var trianglesCount = 0;
            
            for (int x = 0; x < WorldSize.x; x++)
            {
                for (int y = 0; y < WorldSize.y; y++)
                {
                    for (int z = 0; z < WorldSize.z; z++)
                    {
                        ref var block = ref GetBlock(x, y, z);

                        if (block.Void)
                        {
                            continue;
                        }

                        var face = default(Face);

                        face = block.Left;
                        if (face.Draw && (block.Position.x > 0 && GetBlock(x - 1, y, z).Void || block.Position.x == 0))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x, y, z) * BlockSize,
                                new Vector3(x, y, z + 1) * BlockSize,
                                new Vector3(x, y + 1, z + 1) * BlockSize,
                                new Vector3(x, y + 1, z) * BlockSize,
                            });
                            
                            triangles.AddRange(new []
                            {
                                trianglesCount * 4,
                                trianglesCount * 4 + 1,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 3,
                                trianglesCount * 4,
                            });
                            trianglesCount++;
                            
                            var atlasTextureData = WorldAtlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
                            uvs.AddRange(new []
                            {
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1f - atlasTextureData.UvSize.y),
                                atlasTextureData.UvPosition + new Vector2(0, 1f - atlasTextureData.UvSize.y),
                                atlasTextureData.UvPosition + new Vector2(0, 1),
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1),
                            });

                            tangents.AddRange(new []
                            {
                                upTangent,
                                upTangent,
                                upTangent,
                                upTangent
                            });
                        }

                        face = block.Right;
                        if (face.Draw && (block.Position.x < xSize - 1 && GetBlock(x + 1, y, z).Void || block.Position.x == xSize - 1))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x + 1, y, z + 1) * BlockSize,
                                new Vector3(x + 1, y, z) * BlockSize,
                                new Vector3(x + 1, y + 1, z) * BlockSize,
                                new Vector3(x + 1, y + 1, z + 1) * BlockSize,
                            });
                            
                            triangles.AddRange(new []
                            {
                                trianglesCount * 4,
                                trianglesCount * 4 + 1,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 3,
                                trianglesCount * 4,
                            });
                            trianglesCount++;
                            
                            var atlasTextureData = WorldAtlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
                            uvs.AddRange(new []
                            {
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1f - atlasTextureData.UvSize.y),
                                atlasTextureData.UvPosition + new Vector2(0, 1f - atlasTextureData.UvSize.y),
                                atlasTextureData.UvPosition + new Vector2(0, 1),
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1),
                            });

                            tangents.AddRange(new []
                            {
                                upTangent,
                                upTangent,
                                upTangent,
                                upTangent
                            });
                        }

                        face = block.Top;
                        if (face.Draw && (block.Position.y < ySize - 1 && GetBlock(x, y + 1, z).Void || block.Position.y == ySize - 1))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x, y + 1, z) * BlockSize,
                                new Vector3(x, y + 1, z + 1) * BlockSize,
                                new Vector3(x + 1, y + 1, z + 1) * BlockSize,
                                new Vector3(x + 1, y + 1, z) * BlockSize,
                            });
                            
                            triangles.AddRange(new []
                            {
                                trianglesCount * 4,
                                trianglesCount * 4 + 1,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 3,
                                trianglesCount * 4,
                            });
                            trianglesCount++;
                            
                            var atlasTextureData = WorldAtlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
                            uvs.AddRange(new []
                            {
                                atlasTextureData.UvPosition + new Vector2(0, 1f - atlasTextureData.UvSize.y),
                                atlasTextureData.UvPosition + new Vector2(0, 1),
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1),
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1f - atlasTextureData.UvSize.y),
                            });

                            tangents.AddRange(new []
                            {
                                upTangent,
                                upTangent,
                                upTangent,
                                upTangent
                            });
                        }

                        face = block.Bottom;
                        if (face.Draw && (block.Position.y > 0 && GetBlock(x, y - 1, z).Void || block.Position.y == 0))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x, y, z + 1) * BlockSize,
                                new Vector3(x, y, z) * BlockSize,
                                new Vector3(x + 1, y, z) * BlockSize,
                                new Vector3(x + 1, y, z + 1) * BlockSize,
                            });
                            
                            triangles.AddRange(new []
                            {
                                trianglesCount * 4,
                                trianglesCount * 4 + 1,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 3,
                                trianglesCount * 4,
                            });
                            trianglesCount++;
                            
                            var atlasTextureData = WorldAtlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
                            uvs.AddRange(new []
                            {
                                atlasTextureData.UvPosition + new Vector2(0, 1f - atlasTextureData.UvSize.y),
                                atlasTextureData.UvPosition + new Vector2(0, 1),
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1),
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1f - atlasTextureData.UvSize.y),
                            });

                            tangents.AddRange(new []
                            {
                                upTangent,
                                upTangent,
                                upTangent,
                                upTangent
                            });
                        }

                        face = block.Back;
                        if (face.Draw && (block.Position.z > 0 && GetBlock(x, y, z - 1).Void || block.Position.z == 0))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x + 1, y, z) * BlockSize,
                                new Vector3(x, y, z) * BlockSize,
                                new Vector3(x, y + 1, z) * BlockSize,
                                new Vector3(x + 1, y + 1, z) * BlockSize,
                            });
                            
                            triangles.AddRange(new []
                            {
                                trianglesCount * 4,
                                trianglesCount * 4 + 1,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 3,
                                trianglesCount * 4,
                            });
                            trianglesCount++;
                            
                            var atlasTextureData = WorldAtlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
                            uvs.AddRange(new []
                            {
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1f - atlasTextureData.UvSize.y),
                                atlasTextureData.UvPosition + new Vector2(0, 1f - atlasTextureData.UvSize.y),
                                atlasTextureData.UvPosition + new Vector2(0, 1),
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1),
                            });

                            tangents.AddRange(new []
                            {
                                upTangent,
                                upTangent,
                                upTangent,
                                upTangent
                            });
                        }
                        
                        face = block.Forward;
                        if (face.Draw && (block.Position.z < zSize - 1 && GetBlock(x, y, z + 1).Void || block.Position.z == zSize - 1))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x, y, z + 1) * BlockSize,
                                new Vector3(x + 1, y, z + 1) * BlockSize,
                                new Vector3(x + 1, y + 1, z + 1) * BlockSize,
                                new Vector3(x, y + 1, z + 1) * BlockSize,
                            });
                            
                            triangles.AddRange(new []
                            {
                                trianglesCount * 4,
                                trianglesCount * 4 + 1,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 2,
                                trianglesCount * 4 + 3,
                                trianglesCount * 4,
                            });
                            trianglesCount++;
                            
                            var atlasTextureData = WorldAtlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
                            uvs.AddRange(new []
                            {
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1f - atlasTextureData.UvSize.y),
                                atlasTextureData.UvPosition + new Vector2(0, 1f - atlasTextureData.UvSize.y),
                                atlasTextureData.UvPosition + new Vector2(0, 1),
                                atlasTextureData.UvPosition + new Vector2(atlasTextureData.UvSize.x, 1),
                            });

                            tangents.AddRange(new []
                            {
                                upTangent,
                                upTangent,
                                upTangent,
                                upTangent
                            });
                        }
                        
                        //
                    }
                }
            }
            
            _mesh.vertices = vertices.ToArray();
            _mesh.uv = uvs.ToArray();
            _mesh.tangents = tangents.ToArray();
            _mesh.triangles = triangles.ToArray();
            
            _mesh.RecalculateNormals();
            _meshCollider.sharedMesh = _mesh;
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
                Debug.LogError($"Wrong bounce: x[{x}], y[{y}], z[{z}]; world ({WorldSize.x}, {WorldSize.y}, {WorldSize.z}); formula is {x + y * WorldSize.x + z * WorldSize.x * WorldSize.y} (max is {WorldSize.x * WorldSize.y * WorldSize.z})");
                throw;
            }
        }
    }
}