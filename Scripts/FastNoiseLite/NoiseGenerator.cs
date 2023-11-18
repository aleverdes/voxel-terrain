using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AleVerDes.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Noise", fileName = "Noise")]
    public class NoiseGenerator : ScriptableObject
    {
        public int Seed = 1337;
        public float Frequency = 0.01f;
        public FastNoiseLite.NoiseType NoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
        public FastNoiseLite.RotationType3D RotationType3D = FastNoiseLite.RotationType3D.None;

        [Space]
        public FastNoiseLite.FractalType FractalType = FastNoiseLite.FractalType.None;
        public int Octaves = 3;
        public float Lacunarity = 2f;
        public float Gain = 0.5f;
        public float WeightedStrength = 0f;
        public float PingPongStrength = 2f;
    
        [Space]
        public FastNoiseLite.CellularDistanceFunction CellularDistanceFunction = FastNoiseLite.CellularDistanceFunction.EuclideanSq;
        public FastNoiseLite.CellularReturnType CellularReturnType = FastNoiseLite.CellularReturnType.Distance;
        public float CellularJitterModifier = 1f;

        [Space]
        public FastNoiseLite.DomainWarpType DomainWarpType = FastNoiseLite.DomainWarpType.OpenSimplex2;
        public FastNoiseLite.TransformType3D WarpTransformType3D = FastNoiseLite.TransformType3D.DefaultOpenSimplex2;
        public float DomainWarpAmp = 1f;
        
        [HideInInspector] [SerializeField] private Vector2 _minMaxNoise = new Vector2(float.MaxValue, float.MinValue);

        private FastNoiseLite _noiseGenerator = new FastNoiseLite();

        [Button("Normalize")]
        public void Normalize()
        {
            const int iterations = 256;
            _minMaxNoise = _noiseGenerator.NormalizeNoise(new Vector2(iterations, iterations));
        }
        
        public float GetNoise(Vector2 position)
        {
            _noiseGenerator.ApplySettings(this);
            return _noiseGenerator.GetNormalizedNoise(position, _minMaxNoise);
        }
        
        public float GetNoise(float x, float z)
        {
            _noiseGenerator.ApplySettings(this);
            return _noiseGenerator.GetNormalizedNoise(x, z, _minMaxNoise);
        }
        
        public float GetNoise(Vector3 position)
        {
            _noiseGenerator.ApplySettings(this);
            return _noiseGenerator.GetNormalizedNoise(position, _minMaxNoise);
        }
        
        public float GetNoise(float x, float y, float z)
        {
            _noiseGenerator.ApplySettings(this);
            return _noiseGenerator.GetNormalizedNoise(x, y, z, _minMaxNoise);
        }
    }
}