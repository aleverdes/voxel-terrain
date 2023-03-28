using System;
using UnityEngine;

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
        [HideInInspector] [SerializeField] private Block[,,] _blocks;
        
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
            GenerateEmptyMesh();
            
            _blocks = new Block[WorldSize.x, WorldSize.y, WorldSize.z];
        }

        private void SetupComponents()
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

        private void GenerateEmptyMesh()
        {
            _mesh = new Mesh();
            
            var xSize = WorldSize.x;
            var zSize = WorldSize.z;
            
            var vertices = new Vector3[(WorldSize.x + 1) * (WorldSize.z + 1)];
            var uv = new Vector2[vertices.Length];
            var tangents = new Vector4[vertices.Length];
            var tangent = new Vector4(1f, 0f, 0f, -1f);
            
            for (int i = 0, z = 0; z <= zSize; z++)
            {
                for (var x = 0; x <= xSize; x++, i++)
                {
                    vertices[i] = new Vector3(x * BlockSize, 0, z * BlockSize);
                    uv[i] = new Vector2((float)x / xSize, (float)z / zSize);
                    tangents[i] = tangent;
                }
            }

            _mesh.vertices = vertices;
            _mesh.uv = uv;
            _mesh.tangents = tangents;

            var triangles = new int[xSize * zSize * 6];
            for (int ti = 0, vi = 0, y = 0; y < zSize; y++, vi++)
            {
                for (var x = 0; x < xSize; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                    triangles[ti + 5] = vi + xSize + 2;
                }
            }

            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
            
            _meshFilter.sharedMesh = _mesh;
        }
    }
}