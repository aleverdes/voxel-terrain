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

        public void UpdateMesh(ref Mesh mesh, GenerationData generationData)
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

                var vertexOffset = GetVertexOffset(verticesNoise, chunkOffset + new Vector3Int(x, y, z));
                var noiseWeight = GetBlockNoiseWeightIndex(new Vector3Int(x, y, z), chunkSize);
                var noiseWeightNormalized = noiseWeight / 255f;
                vertexOffset.RTB *= noiseWeightNormalized; 
                vertexOffset.RTF *= noiseWeightNormalized;
                vertexOffset.RBB *= noiseWeightNormalized;
                vertexOffset.RBF *= noiseWeightNormalized;
                vertexOffset.LTB *= noiseWeightNormalized;
                vertexOffset.LTF *= noiseWeightNormalized;
                vertexOffset.LBB *= noiseWeightNormalized;
                vertexOffset.LBF *= noiseWeightNormalized;
                vertexOffset.RTB = new(vertexOffset.RTB.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.RTB.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.RTB.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.RTF = new(vertexOffset.RTF.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.RTF.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.RTF.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.RBB = new(vertexOffset.RBB.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.RBB.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.RBB.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.RBF = new(vertexOffset.RBF.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.RBF.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.RBF.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.LTB = new(vertexOffset.LTB.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.LTB.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.LTB.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.LTF = new(vertexOffset.LTF.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.LTF.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.LTF.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.LBB = new(vertexOffset.LBB.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.LBB.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.LBB.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.LBF = new(vertexOffset.LBF.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.LBF.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.LBF.z * voxelTerrainSettings.VerticesNoiseScale.z);

                var v000 = chunkOffset + new Vector3Int(x, y, z) + vertexOffset.LBB;
                var v001 = chunkOffset + new Vector3Int(x, y, z + 1) + vertexOffset.LBF;
                var v011 = chunkOffset + new Vector3Int(x, y + 1, z + 1) + vertexOffset.LTF;
                var v010 = chunkOffset + new Vector3Int(x, y + 1, z) + vertexOffset.LTB;
                var v100 = chunkOffset + new Vector3Int(x + 1, y, z) + vertexOffset.RBB;
                var v101 = chunkOffset + new Vector3Int(x + 1, y, z + 1) + vertexOffset.RBF;
                var v111 = chunkOffset + new Vector3Int(x + 1, y + 1, z + 1) + vertexOffset.RTF;
                var v110 = chunkOffset + new Vector3Int(x + 1, y + 1, z) + vertexOffset.RTB;  
                
                var textureData = atlas.GetVoxelTexturesUV(blockVoxel - 1, x + y + z);
                var uvPositions = new UVPositions
                {
                    Top = textureData.Top.WithY(-textureData.Top.y),
                    Bottom = textureData.Bottom.WithY(-textureData.Bottom.y),
                    Side = textureData.Side.WithY(-textureData.Side.y),
                };
                var uvSize = atlas.TextureSizeInAtlas;

                var left = chunkOffset + new Vector3Int(x - 1, y, z);
                if (x > 0 && !voxelTerrain.IsBlockExists(left) || x == 0)
                {
                    vertices.AddRange(new[]
                    {
                        v000 * blockSize.z,
                        v001 * blockSize.z,
                        v011 * blockSize.z,
                        v010 * blockSize.z,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Left);
                    AddTangents(tangents);
                }

                var right = chunkOffset + new Vector3Int(x + 1, y, z);
                if (x < chunkSize.x - 1 && !voxelTerrain.IsBlockExists(right) || x == chunkSize.x - 1)
                {
                    vertices.AddRange(new[]
                    {
                        v101 * blockSize.z,
                        v100 * blockSize.z,
                        v110 * blockSize.z,
                        v111 * blockSize.z,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Right);
                    AddTangents(tangents);
                }

                var top = chunkOffset + new Vector3Int(x, y + 1, z);
                if (y < chunkSize.y - 1 && !voxelTerrain.IsBlockExists(top) || y == chunkSize.y - 1)
                {
                    vertices.AddRange(new[]
                    {
                        v010 * blockSize.y,
                        v011 * blockSize.y,
                        v111 * blockSize.y,
                        v110 * blockSize.y,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Top);
                    AddTangents(tangents);
                }

                var bottom = chunkOffset + new Vector3Int(x, y - 1, z);
                if (y > 0 && !voxelTerrain.IsBlockExists(bottom) || y == 0)
                {
                    vertices.AddRange(new[]
                    {
                        v001 * blockSize.y,
                        v000 * blockSize.y,
                        v100 * blockSize.y,
                        v101 * blockSize.y,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Bottom);
                    AddTangents(tangents);
                }

                var back = chunkOffset + new Vector3Int(x, y, z - 1);
                if (z > 0 && !voxelTerrain.IsBlockExists(back) || z == 0)
                {
                    vertices.AddRange(new[]
                    {
                        v100 * blockSize.x,
                        v000 * blockSize.x,
                        v010 * blockSize.x,
                        v110 * blockSize.x,
                    });

                    AddTriangles(triangles, ref trianglesCount);
                    AddUVs(uvs, uvPositions, uvSize, FaceDirection.Back);
                    AddTangents(tangents);
                }

                var front = chunkOffset + new Vector3Int(x, y, z + 1);
                if (z < chunkSize.z - 1 && !voxelTerrain.IsBlockExists(front) || z == chunkSize.z - 1)
                {
                    vertices.AddRange(new[]
                    {
                        v001 * blockSize.x,
                        v101 * blockSize.x,
                        v111 * blockSize.x,
                        v011 * blockSize.x,
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
        }

        private static VertexOffset GetVertexOffset(NoiseProvider noiseGenerator, Vector3Int blockPosition)
        {
            var rtf = GetNoisedVertex(blockPosition + Vector3Int.right + Vector3Int.forward + Vector3Int.up);
            var rtb = GetNoisedVertex(blockPosition + Vector3Int.right + Vector3Int.up);
            var rbf = GetNoisedVertex(blockPosition + Vector3Int.right + Vector3Int.forward);
            var rbb = GetNoisedVertex(blockPosition + Vector3Int.right);
            var ltf = GetNoisedVertex(blockPosition + Vector3Int.forward + Vector3Int.up);
            var lbf = GetNoisedVertex(blockPosition + Vector3Int.forward);
            var ltb = GetNoisedVertex(blockPosition + Vector3Int.up);
            var lbb = GetNoisedVertex(blockPosition);
            return new VertexOffset
            {
                RTF = rtf,
                RTB = rtb,
                RBB = rbb,
                RBF = rbf,
                LTF = ltf,
                LTB = ltb,
                LBB = lbb,
                LBF = lbf,
            };

            Vector3 GetNoisedVertex(Vector3Int vertexPosition)
            {
                return new Vector3
                {
                    x = 2f * noiseGenerator.GetNoise(vertexPosition) - 1,
                    y = 2f * noiseGenerator.GetNoise(vertexPosition + 111f * Vector3.one) - 1,
                    z = 2f * noiseGenerator.GetNoise(vertexPosition - 111f * Vector3.one) - 1,
                };
            }
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

        private struct VertexOffset
        {
            public Vector3 RTF;
            public Vector3 RTB;
            public Vector3 RBF;
            public Vector3 RBB;
            
            public Vector3 LTF;
            public Vector3 LTB;
            public Vector3 LBF;
            public Vector3 LBB;
        }
    }
}