using System;
using UnityEngine;

namespace TravkinGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Noise/Noise Combinator", fileName = "Noise Combinator")]
    public class NoiseCombinator : NoiseProvider
    {
        [SerializeField] private Noise[] _noises;
        
        [HideInInspector] [SerializeField] private Vector2 _minMaxNoise = new Vector2(float.MaxValue, float.MinValue);
        
        public override void Normalize()
        {
            const int iterations = 256;
            
            _minMaxNoise = new Vector2(float.MaxValue, float.MinValue);
            for (var i = 0; i < iterations; i++)
            for (var j = 0; j < iterations; j++)
            {
                var noise = GetNoise(j, i);
                _minMaxNoise.x = Mathf.Min(_minMaxNoise.x, noise);
                _minMaxNoise.y = Mathf.Max(_minMaxNoise.y, noise);
            }
        }

        public override float GetNoise(float x, float y, float z)
        {
            var result = 0f;
            foreach (var noise in _noises)
            {
                var n = noise.NoiseProvider.GetNoise(x, y, z);
                n = Mathf.InverseLerp(noise.InverseLerp.x, noise.InverseLerp.y, n);
                switch (noise.ActionWithPrevious)
                {
                    case ActionType.Add:
                        result += n;
                        break;
                    case ActionType.Multiply:
                        result *= n;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return result;
        }

        public override float GetNoiseWithSeed(int seed, float x, float y, float z)
        {
            var result = 0f;
            foreach (var noise in _noises)
            {
                var n = noise.NoiseProvider.GetNoiseWithSeed(seed, x, y, z);
                n = Mathf.InverseLerp(noise.InverseLerp.x, noise.InverseLerp.y, n);
                switch (noise.ActionWithPrevious)
                {
                    case ActionType.Add:
                        result += n;
                        break;
                    case ActionType.Multiply:
                        result *= n;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return result;
        }

        [Serializable]
        private class Noise
        {
            public NoiseProvider NoiseProvider;
            public Vector2 InverseLerp = new Vector2(0f, 1f);
            public ActionType ActionWithPrevious = ActionType.Add;
        }

        private enum ActionType
        {
            Add,
            Multiply,
        }
    }
}