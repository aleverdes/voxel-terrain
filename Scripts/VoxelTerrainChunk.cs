using System.Collections.Generic;
using AleVerDes.UnityUtils;
using UnityEngine;

namespace AleVerDes.Voxels
{
    [CreateAssetMenu(fileName = "Voxel Terrain Chunk", menuName = "Voxels/Terrain Chunk")]
    public class VoxelTerrainChunk : ScriptableObject
    {
        [HideInInspector] public byte[] BlockVoxels;
        [HideInInspector] public byte[] BlockNoiseWeights;

        public void Initialize(Vector3Int chunkSize)
        {
            BlockVoxels = new byte[chunkSize.x * chunkSize.y * chunkSize.z];
            for (var i = 0; i < BlockVoxels.Length; i++) 
                BlockVoxels[i] = 0;

            BlockNoiseWeights = new byte[chunkSize.x * chunkSize.y * chunkSize.z];
            for (var i = 0; i < BlockNoiseWeights.Length; i++) 
                BlockNoiseWeights[i] = 255;
        }
        
        public int GetBlockIndex(Vector3Int blockPosition, Vector3Int chunkSize)
        {
            return blockPosition.x + blockPosition.y * chunkSize.x + blockPosition.z * chunkSize.x * chunkSize.y;
        }
        
        public ref byte GetBlockVoxelIndex(Vector3Int blockPosition, Vector3Int chunkSize)
        {
            return ref BlockVoxels[GetBlockIndex(blockPosition, chunkSize)];
        }
        
        public ref byte GetBlockNoiseWeightIndex(Vector3Int blockPosition, Vector3Int chunkSize)
        {
            return ref BlockNoiseWeights[GetBlockIndex(blockPosition, chunkSize)];
        }

        public Mesh UpdateMesh(Mesh mesh, GenerationData generationData)
        {
            if (!mesh)
                mesh = new Mesh();
            else
                mesh.Clear();

            var voxelTerrain = generationData.VoxelTerrain;
            var voxelTerrainSettings = voxelTerrain.Settings;
            var blockSize = voxelTerrainSettings.BlockSize;
            var chunkSize = voxelTerrainSettings.ChunkSize;
            var verticesNoise = voxelTerrain.VerticesNoise;
            var atlas = voxelTerrainSettings.TextureAtlas;

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tangents = new List<Vector4>();
            var triangles = new List<int>();
            var trianglesCount = 0;

            var chunkOffset = generationData.ChunkPosition * chunkSize;
            
            for (var x = 0; x < chunkSize.x; x++)
            for (var y = 0; y < chunkSize.y; y++)
            for (var z = 0; z < chunkSize.z; z++)
            {
                ref var blockVoxel = ref GetBlockVoxelIndex(new Vector3Int(x, y, z), chunkSize);

                if (blockVoxel == 0)
                    continue;

                var textureData = atlas.GetVoxelTexturesUV(blockVoxel - 1, x + y + z);
                var uvPositions = new UVPositions
                {
                    Top = textureData.Top.WithY(-textureData.Top.y),
                    Bottom = textureData.Bottom.WithY(-textureData.Bottom.y),
                    Side = textureData.Side.WithY(-textureData.Side.y),
                };
                var uvSize = atlas.TextureSizeInAtlas;
                
                var left = chunkOffset + new Vector3Int(x - 1, y, z);
                if (x > 0 && voxelTerrain.GetBlockVoxelIndex(left, chunkSize) == 0 || x == 0)
                {
                    vertices.AddRange(new[]
                    {
                        new Vector3(x, y, z) * blockSize.z,
                        new Vector3(x, y, z + 1) * blockSize.z,
                        new Vector3(x, y + 1, z + 1) * blockSize.z,
                        new Vector3(x, y + 1, z) * blockSize.z,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Left);
                    AddTangents(tangents);
                }

                var right = chunkOffset + new Vector3Int(x + 1, y, z);
                if (x < chunkSize.x - 1 && voxelTerrain.GetBlockVoxelIndex(right, chunkSize) == 0 || x == chunkSize.x - 1)
                {
                    vertices.AddRange(new[]
                    {
                        new Vector3(x + 1, y, z + 1) * blockSize.z,
                        new Vector3(x + 1, y, z) * blockSize.z,
                        new Vector3(x + 1, y + 1, z) * blockSize.z,
                        new Vector3(x + 1, y + 1, z + 1) * blockSize.z,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Right);
                    AddTangents(tangents);
                }

                var top = chunkOffset + new Vector3Int(x, y + 1, z);
                if (y < chunkSize.y - 1 && voxelTerrain.GetBlockVoxelIndex(top, chunkSize) == 0 || y == chunkSize.y - 1)
                {
                    vertices.AddRange(new[]
                    {
                        new Vector3(x, y + 1, z) * blockSize.y,
                        new Vector3(x, y + 1, z + 1) * blockSize.y,
                        new Vector3(x + 1, y + 1, z + 1) * blockSize.y,
                        new Vector3(x + 1, y + 1, z) * blockSize.y,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Top);
                    AddTangents(tangents);
                }

                var bottom = chunkOffset + new Vector3Int(x, y - 1, z);
                if (y > 0 && voxelTerrain.GetBlockVoxelIndex(bottom, chunkSize) == 0 || y == 0)
                {
                    vertices.AddRange(new[]
                    {
                        new Vector3(x, y, z + 1) * blockSize.y,
                        new Vector3(x, y, z) * blockSize.y,
                        new Vector3(x + 1, y, z) * blockSize.y,
                        new Vector3(x + 1, y, z + 1) * blockSize.y,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Bottom);
                    AddTangents(tangents);
                }

                var back = chunkOffset + new Vector3Int(x, y, z - 1);
                if (z > 0 && voxelTerrain.GetBlockVoxelIndex(back, chunkSize) == 0 || z == 0)
                {
                    vertices.AddRange(new[]
                    {
                        new Vector3(x + 1, y, z) * blockSize.x,
                        new Vector3(x, y, z) * blockSize.x,
                        new Vector3(x, y + 1, z) * blockSize.x,
                        new Vector3(x + 1, y + 1, z) * blockSize.x,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Back);
                    AddTangents(tangents);
                }

                var front = chunkOffset + new Vector3Int(x, y, z + 1);
                if (z < chunkSize.z - 1 && voxelTerrain.GetBlockVoxelIndex(front, chunkSize) == 0 || z == chunkSize.z - 1)
                {
                    vertices.AddRange(new[]
                    {
                        new Vector3(x, y, z + 1) * blockSize.x,
                        new Vector3(x + 1, y, z + 1) * blockSize.x,
                        new Vector3(x + 1, y + 1, z + 1) * blockSize.x,
                        new Vector3(x, y + 1, z + 1) * blockSize.x,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Front);
                    AddTangents(tangents);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.tangents = tangents.ToArray();
            mesh.triangles = triangles.ToArray();
            
            mesh.RecalculateNormals();

            return mesh;
        }

        private static void AddTangents(List<Vector4> tangents)
        {
            var upTangent = new Vector4(1f, 0f, 0f, -1f);
            tangents.AddRange(new []
            {
                upTangent,
                upTangent,
                upTangent,
                upTangent
            });
        }

        private static void AddTriangles(List<int> triangles, ref int trianglesCount)
        {
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
        }

        private static void AddUVs(List<Vector2> uvs, UVPositions uvPositions, Vector2 uvSize, FaceDirection faceDirection)
        {
            switch (faceDirection)
            {
                case FaceDirection.Top:
                    uvs.AddRange(new []
                    {
                        uvPositions.Top + new Vector2(0, 1f - uvSize.y),
                        uvPositions.Top + new Vector2(0, 1),
                        uvPositions.Top + new Vector2(uvSize.x, 1),
                        uvPositions.Top + new Vector2(uvSize.x, 1f - uvSize.y),
                    });
                    break;
                case FaceDirection.Bottom:
                    uvs.AddRange(new []
                    {
                        uvPositions.Bottom + new Vector2(0, 1f - uvSize.y),
                        uvPositions.Bottom + new Vector2(0, 1),
                        uvPositions.Bottom + new Vector2(uvSize.x, 1),
                        uvPositions.Bottom + new Vector2(uvSize.x, 1f - uvSize.y),
                    });
                    break;
                default:
                    uvs.AddRange(new []
                    {
                        uvPositions.Side + new Vector2(uvSize.x, 1f - uvSize.y),
                        uvPositions.Side + new Vector2(0, 1f - uvSize.y),
                        uvPositions.Side + new Vector2(0, 1),
                        uvPositions.Side + new Vector2(uvSize.x, 1),
                    });
                    break;
            }
        }

        public struct GenerationData
        {
            public Vector3Int ChunkPosition;
            public VoxelTerrain VoxelTerrain;
        }
        
        private struct UVPositions
        {
            public Vector2 Side;
            public Vector2 Top;
            public Vector2 Bottom;
        }
    }
}