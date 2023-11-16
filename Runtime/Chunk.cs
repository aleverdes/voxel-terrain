using System.Collections.Generic;
using UnityEngine;

namespace AleVerDes.VoxelTerrain
{
    public class Chunk : MonoBehaviour
    {
        [Header("Chunk")]
        [SerializeField] private World _world;
        [SerializeField] private Vector2Int _chunkPosition;
        
        [Header("Mesh")]
        [SerializeField] private Mesh _mesh;
        [SerializeField] private GameObject _meshObject;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private MeshCollider _meshCollider;
        
        public Mesh Mesh => _mesh;
        public GameObject MeshObject => _meshObject;
        public MeshFilter MeshFilter => _meshFilter;
        public MeshRenderer MeshRenderer => _meshRenderer;
        public MeshCollider MeshCollider => _meshCollider;

        private Vector3Int WorldSize => _world.WorldSettings.WorldSize;
        private Vector2Int ChunkSize => _world.WorldSettings.ChunkSize;
        private float BlockSize => _world.WorldSettings.BlockSize;
        private Atlas Atlas => _world.WorldSettings.WorldAtlas;
        private Material Material => _world.WorldSettings.WorldMaterial;
        
        public void Setup(World world, Vector2Int chunkPosition)
        {
            if (_meshObject)
            {
                return;
            }

            _world = world;
            _chunkPosition = chunkPosition;

            _meshObject = new GameObject("Chunk Mesh");
            _meshObject.transform.SetParent(transform);
            _meshObject.transform.localPosition = Vector3.zero;
            _meshObject.transform.localRotation = Quaternion.identity;
            _meshObject.transform.localScale = Vector3.one;
        }
        
        public void GenerateMesh()
        {
            if (!_mesh)
            {
                _mesh = new Mesh();
            }
            else
            {
                _mesh.Clear();
            }
            
            var xSize = WorldSize.x;
            var ySize = WorldSize.y;
            var zSize = WorldSize.z;

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tangents = new List<Vector4>();
            var triangles = new List<int>();
            
            var upTangent = new Vector4(1f, 0f, 0f, -1f);

            var trianglesCount = 0;
            
            for (int x = _chunkPosition.x; x < _chunkPosition.x + ChunkSize.x; x++)
            {
                for (int y = 0; y < WorldSize.y; y++)
                {
                    for (int z = _chunkPosition.y; z < _chunkPosition.y + ChunkSize.y; z++)
                    {
                        ref var block = ref _world.GetBlock(x, y, z);

                        if (block.Void)
                        {
                            continue;
                        }

                        var face = default(BlockFace);
                        
                        var yh = block.TopVerticesHeights;

                        face = block.Left;
                        if (face.Draw && (block.Position.x > 0 && _world.GetBlock(x - 1, y, z).Void || block.Position.x == 0))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x, y, z) * BlockSize,
                                new Vector3(x, y, z + 1) * BlockSize,
                                new Vector3(x, y + yh.ForwardLeft, z + 1) * BlockSize,
                                new Vector3(x, y + yh.BackLeft, z) * BlockSize,
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
                            
                            var atlasTextureData = Atlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
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
                        if (face.Draw && (block.Position.x < xSize - 1 && _world.GetBlock(x + 1, y, z).Void || block.Position.x == xSize - 1))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x + 1, y, z + 1) * BlockSize,
                                new Vector3(x + 1, y, z) * BlockSize,
                                new Vector3(x + 1, y + yh.BackRight, z) * BlockSize,
                                new Vector3(x + 1, y + yh.ForwardRight, z + 1) * BlockSize,
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
                            
                            var atlasTextureData = Atlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
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
                        if (face.Draw && (block.Position.y < ySize - 1 && _world.GetBlock(x, y + 1, z).Void || block.Position.y == ySize - 1))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x, y + yh.BackLeft, z) * BlockSize,
                                new Vector3(x, y + yh.ForwardLeft, z + 1) * BlockSize,
                                new Vector3(x + 1, y + yh.ForwardRight, z + 1) * BlockSize,
                                new Vector3(x + 1, y + yh.BackRight, z) * BlockSize,
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
                            
                            var atlasTextureData = Atlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
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
                        if (face.Draw && (block.Position.y > 0 && _world.GetBlock(x, y - 1, z).Void || block.Position.y == 0))
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
                            
                            var atlasTextureData = Atlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
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
                        if (face.Draw && (block.Position.z > 0 && _world.GetBlock(x, y, z - 1).Void || block.Position.z == 0))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x + 1, y, z) * BlockSize,
                                new Vector3(x, y, z) * BlockSize,
                                new Vector3(x, y + yh.BackLeft, z) * BlockSize,
                                new Vector3(x + 1, y + yh.BackRight, z) * BlockSize,
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
                            
                            var atlasTextureData = Atlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
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
                        if (face.Draw && (block.Position.z < zSize - 1 && _world.GetBlock(x, y, z + 1).Void || block.Position.z == zSize - 1))
                        {
                            vertices.AddRange(new []
                            {
                                new Vector3(x, y, z + 1) * BlockSize,
                                new Vector3(x + 1, y, z + 1) * BlockSize,
                                new Vector3(x + 1, y + yh.ForwardRight, z + 1) * BlockSize,
                                new Vector3(x, y + yh.ForwardLeft, z + 1) * BlockSize,
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
                            
                            var atlasTextureData = Atlas.AtlasLayers[face.LayerIndex].Textures[face.LayerTextureIndex];
                            
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

            if (!_meshFilter)
            {
                _meshFilter = _meshObject.AddComponent<MeshFilter>();
            }

            if (!_meshCollider)
            {
                _meshCollider = _meshObject.AddComponent<MeshCollider>();
            }

            if (!_meshRenderer)
            {
                _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
            }
            
            _meshRenderer.sharedMaterial = _world.WorldSettings.WorldMaterial;
            
            _meshCollider.sharedMesh = _mesh;
            _meshFilter.sharedMesh = _mesh;
        }
    }
}