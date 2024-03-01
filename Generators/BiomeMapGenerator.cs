using Sirenix.OdinInspector;
using UnityEngine;

namespace TravkinGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Generators/BiomeMap", fileName = "Biome Map Generator")]
    public class BiomeMapGenerator : ScriptableObject
    {
        [SerializeField] private BiomeDatabase _biomeDatabase;
        
        [Header("Noises")]
        [SerializeField] private NoiseProvider _temperatureNoise;
        [SerializeField] private NoiseProvider _humidityNoise;
        [SerializeField] private NoiseProvider _altitudeNoise;
        
        public VoxelBiomeState GetVoxelBiomeState(int seed, Vector3 position)
        {
            var temperature = _temperatureNoise.GetNoiseWithSeed(seed, position.x, position.y, position.z);
            var humidity = _humidityNoise.GetNoiseWithSeed(seed, position.x, position.y, position.z);
            var altitude = _altitudeNoise.GetNoiseWithSeed(seed, position.x, position.y, position.z);

            var voxelBiomeState = new VoxelBiomeState
            {
                BestBiome = null,
                AllBiomes = new BiomeWeight[_biomeDatabase.GetCount()]
            };
            
            var target = new Vector3(temperature, humidity, altitude);
            
            var sum = 0f;
            var bestWeight = float.MinValue;
            for (var i = 0; i < _biomeDatabase.GetCount(); i++)
            {
                var biome = _biomeDatabase[i];
                var current = new Vector3(biome.Temperature, biome.Humidity, biome.Altitude);
                var weight = Vector3.Distance(current, target);
                voxelBiomeState.AllBiomes[i] = new BiomeWeight()
                {
                    BiomeDescriptor = biome,
                    Weight = weight
                };

                if (weight > bestWeight)
                {
                    voxelBiomeState.BestBiome = biome;
                    bestWeight = weight;
                }
                
                sum += weight;
            }

            for (var j = 0; j < voxelBiomeState.AllBiomes.Length; j++) 
                voxelBiomeState.AllBiomes[j].Weight = 1f - voxelBiomeState.AllBiomes[j].Weight / sum;
            
            return voxelBiomeState;
        }
        
#if UNITY_EDITOR
        [Button("Preview Biome Map")]
        private void PreviewBiomeMap()
        {
            BiomeMapPreview.ShowWindow(this);
        }
#endif
    }
}