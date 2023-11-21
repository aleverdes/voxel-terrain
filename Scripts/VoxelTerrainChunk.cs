using System.Collections.Generic;
using AleVerDes.UnityUtils;
using UnityEngine;

namespace AleVerDes.Voxels
{
    public class VoxelTerrainChunk : ScriptableObject
    {
        [HideInInspector] public byte[] BlockVoxels;
        [HideInInspector] public byte[] BlockNoiseWeights;
        
        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<Vector2> _uvs = new List<Vector2>();
        private readonly List<Vector4> _tangents = new List<Vector4>();
        private readonly List<int> _triangles = new List<int>();
        private readonly List<Vector3> _normals = new List<Vector3>();

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

            var voxelTerrain = generationData.VoxelTerrain;
            var voxelTerrainSettings = voxelTerrain.Settings;
            var blockSize = voxelTerrainSettings.BlockSize;
            var chunkSize = voxelTerrainSettings.ChunkSize;
            var verticesNoise = voxelTerrain.VerticesNoise;
            var atlas = voxelTerrainSettings.TextureAtlas;

            _normals.Clear();
            _vertices.Clear();
            _uvs.Clear();
            _tangents.Clear();
            _triangles.Clear();

            var trianglesCount = 0;

            var chunkOffset = generationData.ChunkPosition * chunkSize;
            
            for (var x = 0; x < chunkSize.x; x++)
            for (var y = 0; y < chunkSize.y; y++)
            for (var z = 0; z < chunkSize.z; z++)
            {
                ref var blockVoxel = ref GetBlockVoxelIndex(new Vector3Int(x, y, z), chunkSize);

                if (blockVoxel == 0)
                    continue;

                float GetNoiseWeight(Vector3Int blockPosition, Vector3Int delta)
                {
                    var noiseWeight = GetNoiseWeightForBlock(blockPosition);
                    noiseWeight = Mathf.Min(noiseWeight, GetNoiseWeightForBlock(blockPosition + new Vector3Int(delta.x, delta.y, delta.z)));
                    noiseWeight = Mathf.Min(noiseWeight, GetNoiseWeightForBlock(blockPosition + new Vector3Int(0, delta.y, 0)));
                    noiseWeight = Mathf.Min(noiseWeight, GetNoiseWeightForBlock(blockPosition + new Vector3Int(0, 0, delta.z)));
                    noiseWeight = Mathf.Min(noiseWeight, GetNoiseWeightForBlock(blockPosition + new Vector3Int(delta.x, delta.y, 0)));
                    noiseWeight = Mathf.Min(noiseWeight, GetNoiseWeightForBlock(blockPosition + new Vector3Int(delta.x, 0, delta.z)));
                    noiseWeight = Mathf.Min(noiseWeight, GetNoiseWeightForBlock(blockPosition + new Vector3Int(delta.x, 0, 0)));
                        
                    return noiseWeight;

                    float GetNoiseWeightForBlock(Vector3Int bp)
                    {
                        return generationData.VoxelTerrain.IsBlockExistsInChunks(chunkOffset + bp)
                            ? generationData.VoxelTerrain.GetBlockNoiseWeight(chunkOffset + bp) / 255f
                            : 1f;
                    }
                }
                
                var vertexOffset = GetVertexOffset(verticesNoise, chunkOffset + new Vector3Int(x, y, z), blockSize);
                var position = new Vector3Int(x, y, z);
                
                vertexOffset.RTB *= GetNoiseWeight(position, new Vector3Int(1, 1, -1)); 
                vertexOffset.RTF *= GetNoiseWeight(position, new Vector3Int(1, 1, 1));
                vertexOffset.RBB *= GetNoiseWeight(position, new Vector3Int(1, -1, -1));
                vertexOffset.RBF *= GetNoiseWeight(position, new Vector3Int(1, -1, 1));
                vertexOffset.LTB *= GetNoiseWeight(position, new Vector3Int(-1, 1, -1));
                vertexOffset.LTF *= GetNoiseWeight(position, new Vector3Int(-1, 1, 1));
                vertexOffset.LBB *= GetNoiseWeight(position, new Vector3Int(-1, -1, -1));
                vertexOffset.LBF *= GetNoiseWeight(position, new Vector3Int(-1, -1, 1));

                vertexOffset.RTB = new(vertexOffset.RTB.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.RTB.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.RTB.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.RTF = new(vertexOffset.RTF.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.RTF.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.RTF.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.RBB = new(vertexOffset.RBB.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.RBB.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.RBB.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.RBF = new(vertexOffset.RBF.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.RBF.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.RBF.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.LTB = new(vertexOffset.LTB.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.LTB.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.LTB.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.LTF = new(vertexOffset.LTF.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.LTF.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.LTF.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.LBB = new(vertexOffset.LBB.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.LBB.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.LBB.z * voxelTerrainSettings.VerticesNoiseScale.z);
                vertexOffset.LBF = new(vertexOffset.LBF.x * voxelTerrainSettings.VerticesNoiseScale.x, vertexOffset.LBF.y * voxelTerrainSettings.VerticesNoiseScale.y, vertexOffset.LBF.z * voxelTerrainSettings.VerticesNoiseScale.z);

                var v000 = chunkOffset + new Vector3(x, y, z) + vertexOffset.LBB;
                var v001 = chunkOffset + new Vector3(x, y, z + 1) + vertexOffset.LBF;
                var v011 = chunkOffset + new Vector3(x, y + 1, z + 1) + vertexOffset.LTF;
                var v010 = chunkOffset + new Vector3(x, y + 1, z) + vertexOffset.LTB;
                var v100 = chunkOffset + new Vector3(x + 1, y, z) + vertexOffset.RBB;
                var v101 = chunkOffset + new Vector3(x + 1, y, z + 1) + vertexOffset.RBF;
                var v111 = chunkOffset + new Vector3(x + 1, y + 1, z + 1) + vertexOffset.RTF;
                var v110 = chunkOffset + new Vector3(x + 1, y + 1, z) + vertexOffset.RTB;  
                
                var textureData = atlas.GetVoxelTexturesUV(blockVoxel - 1, x + y + z);
                var uvPositions = new UVPositions
                {
                    Top = textureData.Top.WithY(-textureData.Top.y),
                    Bottom = textureData.Bottom.WithY(-textureData.Bottom.y),
                    Side = textureData.Side.WithY(-textureData.Side.y),
                };
                var uvSize = atlas.TextureSizeInAtlas;

                var left = chunkOffset + new Vector3Int(x - 1, y, z);
                if (!voxelTerrain.IsSolidBlock(left) && voxelTerrain.IsBlockExistsInChunks(left))
                {
                    _vertices.AddRange(new[]
                    {
                        Mul(v000, blockSize),
                        Mul(v001, blockSize),
                        Mul(v011, blockSize),
                        Mul(v010, blockSize),
                    });

                    AddTriangles(_triangles, ref trianglesCount);
                    AddUVs(_uvs, uvPositions, uvSize, FaceDirection.Left);
                    AddTangents(_tangents);
                    _normals.Add(Vector3.left);
                    _normals.Add(Vector3.left);
                    _normals.Add(Vector3.left);
                    _normals.Add(Vector3.left);
                }

                var right = chunkOffset + new Vector3Int(x + 1, y, z);
                if (!voxelTerrain.IsSolidBlock(right) && voxelTerrain.IsBlockExistsInChunks(right))
                {
                    _vertices.AddRange(new[]
                    {
                        Mul(v101, blockSize),
                        Mul(v100, blockSize),
                        Mul(v110, blockSize),
                        Mul(v111, blockSize),
                    });

                    AddTriangles(_triangles, ref trianglesCount);
                    AddUVs(_uvs, uvPositions, uvSize, FaceDirection.Right);
                    AddTangents(_tangents);
                    _normals.Add(Vector3.right);
                    _normals.Add(Vector3.right);
                    _normals.Add(Vector3.right);
                    _normals.Add(Vector3.right);
                }

                var top = chunkOffset + new Vector3Int(x, y + 1, z);
                if (!voxelTerrain.IsSolidBlock(top))
                {
                    _vertices.AddRange(new[]
                    {
                        Mul(v010, blockSize),
                        Mul(v011, blockSize),
                        Mul(v111, blockSize),
                        Mul(v110, blockSize),
                    });

                    AddTriangles(_triangles, ref trianglesCount);
                    AddUVs(_uvs, uvPositions, uvSize, FaceDirection.Top);
                    AddTangents(_tangents);
                    _normals.Add(Vector3.up);
                    _normals.Add(Vector3.up);
                    _normals.Add(Vector3.up);
                    _normals.Add(Vector3.up);
                }

                var bottom = chunkOffset + new Vector3Int(x, y - 1, z);
                if (!voxelTerrain.IsSolidBlock(bottom) && voxelTerrain.IsBlockExistsInChunks(bottom))
                {
                    _vertices.AddRange(new[]
                    {
                        Mul(v001, blockSize),
                        Mul(v000, blockSize),
                        Mul(v100, blockSize),
                        Mul(v101, blockSize),
                    });

                    AddTriangles(_triangles, ref trianglesCount);
                    AddUVs(_uvs, uvPositions, uvSize, FaceDirection.Bottom);
                    AddTangents(_tangents);
                    _normals.Add(Vector3.down);
                    _normals.Add(Vector3.down);
                    _normals.Add(Vector3.down);
                    _normals.Add(Vector3.down);
                }

                var back = chunkOffset + new Vector3Int(x, y, z - 1);
                if (!voxelTerrain.IsSolidBlock(back) && voxelTerrain.IsBlockExistsInChunks(back))
                {
                    _vertices.AddRange(new[]
                    {
                        Mul(v100, blockSize),
                        Mul(v000, blockSize),
                        Mul(v010, blockSize),
                        Mul(v110, blockSize),
                    });

                    AddTriangles(_triangles, ref trianglesCount);
                    AddUVs(_uvs, uvPositions, uvSize, FaceDirection.Back);
                    AddTangents(_tangents);
                    _normals.Add(Vector3.back);
                    _normals.Add(Vector3.back);
                    _normals.Add(Vector3.back);
                    _normals.Add(Vector3.back);
                }

                var front = chunkOffset + new Vector3Int(x, y, z + 1);
                if (!voxelTerrain.IsSolidBlock(front) && voxelTerrain.IsBlockExistsInChunks(front))
                {
                    _vertices.AddRange(new[]
                    {
                        Mul(v001, blockSize),
                        Mul(v101, blockSize),
                        Mul(v111, blockSize),
                        Mul(v011, blockSize),
                    });

                    AddTriangles(_triangles, ref trianglesCount);
                    AddUVs(_uvs, uvPositions, uvSize, FaceDirection.Front);
                    AddTangents(_tangents);
                    _normals.Add(Vector3.forward);
                    _normals.Add(Vector3.forward);
                    _normals.Add(Vector3.forward);
                    _normals.Add(Vector3.forward);
                }
            }

            mesh.SetVertices(_vertices);
            mesh.SetUVs(0, _uvs);
            mesh.SetTangents(_tangents);
            mesh.SetTriangles(_triangles, 0);
            mesh.SetNormals(_normals);
        }

        private static VertexOffset GetVertexOffset(NoiseProvider noiseGenerator, Vector3Int blockPosition, Vector3 blockSize)
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
                    x = 2f * noiseGenerator.GetNoise(Mul(vertexPosition, blockSize)) - 1,
                    y = 2f * noiseGenerator.GetNoise(Mul(vertexPosition, blockSize) + 111f * Vector3.one) - 1,
                    z = 2f * noiseGenerator.GetNoise(Mul(vertexPosition, blockSize) - 111f * Vector3.one) - 1,
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

        private static Vector3 Mul(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
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