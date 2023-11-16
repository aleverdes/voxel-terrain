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
        
        public RectInt Rect => new RectInt(_chunkPosition, _world.WorldSettings.ChunkSize);

        private Vector2Int WorldSize => _world.WorldSettings.WorldSize;
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
                _mesh = new Mesh();
            else
                _mesh.Clear();

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tangents = new List<Vector4>();
            var triangles = new List<int>();
            
            var upTangent = new Vector4(1f, 0f, 0f, -1f);

            var trianglesCount = 0;
            
            for (var x = _chunkPosition.x; x < _chunkPosition.x + ChunkSize.x; x++)
            {
                for (var z = _chunkPosition.y; z < _chunkPosition.y + ChunkSize.y; z++)
                {
                    if (_world.IsCellAvoided(_world.GetCellIndex(x, z)))
                        continue;

                    vertices.AddRange(new []
                    {
                        new Vector3(x, _world.GetVertexHeight(x, z), z) * BlockSize,
                        new Vector3(x, _world.GetVertexHeight(x, z + 1), z + 1) * BlockSize,
                        new Vector3(x + 1, _world.GetVertexHeight(x + 1, z + 1), z + 1) * BlockSize,
                        new Vector3(x + 1, _world.GetVertexHeight(x + 1, z), z) * BlockSize,
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
                        
                    var uvPosition = Atlas.TexturesPositions[_world.GetCellTexture(x, z)];
                    var uvSize = Atlas.TextureSizeInAtlas;

                    uvs.AddRange(new []
                    {
                        uvPosition + new Vector2(0, 1f - uvSize.y),
                        uvPosition + new Vector2(0, 1),
                        uvPosition + new Vector2(uvSize.x, 1),
                        uvPosition + new Vector2(uvSize.x, 1f - uvSize.y),
                    });

                    tangents.AddRange(new []
                    {
                        upTangent,
                        upTangent,
                        upTangent,
                        upTangent
                    });
                }
            }

            _mesh.vertices = vertices.ToArray();
            _mesh.uv = uvs.ToArray();
            _mesh.tangents = tangents.ToArray();
            _mesh.triangles = triangles.ToArray();
            
            _mesh.RecalculateNormals();

            if (!_meshFilter) 
                _meshFilter = _meshObject.AddComponent<MeshFilter>();

            if (!_meshCollider) 
                _meshCollider = _meshObject.AddComponent<MeshCollider>();

            if (!_meshRenderer) 
                _meshRenderer = _meshObject.AddComponent<MeshRenderer>();

            _meshRenderer.sharedMaterial = _world.WorldSettings.WorldMaterial;
            
            _meshCollider.sharedMesh = _mesh;
            _meshFilter.sharedMesh = _mesh;
        }
    }
}