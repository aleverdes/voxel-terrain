using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TravkinGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Noise/Noise Generator", fileName = "Noise Generator")]
    public class NoiseGenerator : NoiseProvider
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

        [Space] 
        [Header("Post-Proccessing")] 
        public List<PostProcessingStep> Steps;

        [Space] 
        [Header("Baking")] 
        [ReadOnly, SerializeField] private bool _baked;
        [SerializeField] private Vector3Int _bakingResolution = new Vector3Int(256, 256, 256);
        [HideInInspector] [SerializeField] private byte[] _bakedNoise;
        
        [HideInInspector] [SerializeField] private Vector2 _minMaxNoise = new Vector2(float.MaxValue, float.MinValue);

        private FastNoiseLite _noiseGenerator = new FastNoiseLite();

        public bool IsBaked => _baked;

        [Button("Normalize")]
        public override void Normalize()
        {
            const int iterations = 256;
            _minMaxNoise = _noiseGenerator.NormalizeNoise(new Vector2(iterations, iterations));
        }
        
        [Button("Bake Noise")]
        public void Bake()
        {
            _bakedNoise = new byte[_bakingResolution.x * _bakingResolution.y * _bakingResolution.z];
            
            for (var x = 0; x < _bakingResolution.x; x++)
            for (var y = 0; y < _bakingResolution.y; y++)
            for (var z = 0; z < _bakingResolution.z; z++)
            {
                var noise = GetNoise(x, y, z);
                _bakedNoise[x + y * _bakingResolution.x + z * _bakingResolution.x * _bakingResolution.y] = (byte) (Mathf.Clamp01(noise) * 255);
            }
            
            _baked = true;
        }

        [Button("Clear Baked Data")]
        public void ClearBakedData()
        {
            _bakedNoise = null;
            _baked = false;
        }
        
        public override float GetNoise(float x, float y, float z)
        {
            if (_baked)
            {
                var xInt = (int) Mathf.Repeat(Mathf.FloorToInt(x), _bakingResolution.x);
                var yInt = (int) Mathf.Repeat(Mathf.FloorToInt(y), _bakingResolution.y);
                var zInt = (int) Mathf.Repeat(Mathf.FloorToInt(z), _bakingResolution.z);
                return _bakedNoise[xInt + yInt * _bakingResolution.x + zInt * _bakingResolution.x * _bakingResolution.y] / 255f;
            }
            
            _noiseGenerator.ApplySettings(this);
            return CalculateNoise(x, y, z);
        }

        public override float GetNoiseWithSeed(int seed, float x, float y, float z)
        {
            if (_baked)
            {
                var xInt = (int) Mathf.Repeat(Mathf.FloorToInt(x), _bakingResolution.x);
                var yInt = (int) Mathf.Repeat(Mathf.FloorToInt(y), _bakingResolution.y);
                var zInt = (int) Mathf.Repeat(Mathf.FloorToInt(z), _bakingResolution.z);
                return _bakedNoise[xInt + yInt * _bakingResolution.x + zInt * _bakingResolution.x * _bakingResolution.y] / 255f;
            }
            
            _noiseGenerator.ApplySettings(this);
            _noiseGenerator.SetSeed(seed);
            return CalculateNoise(x, y, z);
        }

        private float CalculateNoise(float x, float y, float z)
        {
            if (_baked)
            {
                var xInt = (int) Mathf.Repeat(Mathf.FloorToInt(x), _bakingResolution.x);
                var yInt = (int) Mathf.Repeat(Mathf.FloorToInt(y), _bakingResolution.y);
                var zInt = (int) Mathf.Repeat(Mathf.FloorToInt(z), _bakingResolution.z);
                return _bakedNoise[xInt + yInt * _bakingResolution.x + zInt * _bakingResolution.x * _bakingResolution.y] / 255f;
            }
            
            var noise = _noiseGenerator.GetNormalizedNoise(x, y, z, _minMaxNoise);
            for (var i = 0; i < Steps.Count; i++) 
                noise = Steps[i].Execute(noise);

            return noise;
        }

        [Serializable]
        public class PostProcessingStep
        {
            public bool Enabled = true;
            public PostProcessingType Type;
            public float Value1;
            public float Value2;
            public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            public float Execute(float input)
            {
                if (!Enabled)
                    return input;

                return Type switch
                {
                    PostProcessingType.None => input,
                    PostProcessingType.Power => Mathf.Pow(input, Value1),
                    PostProcessingType.Invert => 1 - input,
                    PostProcessingType.Abs => Mathf.Abs(input),
                    PostProcessingType.Clamp => Mathf.Clamp(input, Value1, Value2),
                    PostProcessingType.Normalize => Mathf.InverseLerp(Value1, Value2, input),
                    PostProcessingType.Scale => input * Value1,
                    PostProcessingType.Offset => input + Value1,
                    PostProcessingType.Curve => Curve.Evaluate(input),
                    PostProcessingType.SmoothStep => Mathf.SmoothStep(Value1, Value2, input),
                    PostProcessingType.Step => input > Value1 ? 1 : 0,
                    PostProcessingType.Lerp => Mathf.Lerp(Value1, Value2, input),
                    _ => input
                };
            }
        }

        public enum PostProcessingType
        {
            None,
            Power,
            Invert,
            Abs,
            Clamp,
            Normalize,
            Scale,
            Offset,
            Curve,
            SmoothStep,
            Step,
            Lerp,
        }
    }
}