using UnityEngine;

namespace TravkinGames.Voxels
{
    public static class FastNoiseLiteExtensions
    {
        public static void ApplySettings(this FastNoiseLite noise, NoiseGenerator settings)
        {
            noise.SetSeed(settings.Seed);
            noise.SetNoiseType(settings.NoiseType);
            noise.SetFractalType(settings.FractalType);
            noise.SetFractalOctaves(settings.Octaves);
            noise.SetFractalLacunarity(settings.Lacunarity);
            noise.SetFractalGain(settings.Gain);
            noise.SetFrequency(settings.Frequency);
            noise.SetFractalWeightedStrength(settings.WeightedStrength);
            noise.SetFractalPingPongStrength(settings.PingPongStrength);
            noise.SetCellularDistanceFunction(settings.CellularDistanceFunction);
            noise.SetCellularReturnType(settings.CellularReturnType);
            noise.SetCellularJitter(settings.CellularJitterModifier);
            noise.SetDomainWarpType(settings.DomainWarpType);
            noise.SetDomainWarpAmp(settings.DomainWarpAmp);
            noise.SetRotationType3D(settings.RotationType3D);
        }
        
        public static Vector2 NormalizeNoise(this FastNoiseLite noiseGenerator, Vector2 size)
        {
            var minMaxNoise = new Vector2(float.MaxValue, float.MinValue);
            for (var i = 0; i < size.y; i++)
            for (var j = 0; j < size.x; j++)
            {
                var noise = noiseGenerator.GetNoise(j, i);
                minMaxNoise.x = Mathf.Min(minMaxNoise.x, noise);
                minMaxNoise.y = Mathf.Max(minMaxNoise.y, noise);
            }

            return minMaxNoise;
        }

        public static float GetNormalizedNoise(this FastNoiseLite noise, Vector2 position, Vector2 minMax)
        {
            return noise.GetNormalizedNoise(position.x, position.y, minMax);
        }

        public static float GetNormalizedNoise(this FastNoiseLite noise, float x, float z, Vector2 minMax)
        {
            return Mathf.InverseLerp(minMax.x, minMax.y, noise.GetNoise(x, z));
        }

        public static float GetNormalizedNoise(this FastNoiseLite noise, Vector3 position, Vector2 minMax)
        {
            return noise.GetNormalizedNoise(position.x, position.y, position.z, minMax);
        }

        public static float GetNormalizedNoise(this FastNoiseLite noise, float x, float y, float z, Vector2 minMax)
        {
            return Mathf.InverseLerp(minMax.x, minMax.y, noise.GetNoise(x, y, z));
        }
    }
}