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
        
        public (BiomeDescriptor biomeDescriptor, float biomeWeight)[] GetBiomesState(int seed, Vector2Int position)
        {
            var temperature = _temperatureNoise.GetNoiseWithSeed(seed, position.x, position.y, 0);
            var humidity = _humidityNoise.GetNoiseWithSeed(seed, position.x, position.y, 0);
            var altitude = _altitudeNoise.GetNoiseWithSeed(seed, position.x, position.y, 0);

            var biomesState = new (BiomeDescriptor biomeDescriptor, float biomeWeight)[_biomeDatabase.GetCount()];
            
            var target = new Vector3(temperature, humidity, altitude);
            
            var i = 0;
            var sum = 0f;
            foreach (var biome in _biomeDatabase.GetElements())
            {
                var current = new Vector3(biome.Temperature, biome.Humidity, biome.Altitude);
                var weight = Vector3.Distance(current, target);
                biomesState[i] = (biome, weight);
                sum += weight;
                i++;
            }

            for (var j = 0; j < biomesState.Length; j++) 
                biomesState[j].biomeWeight = 1f - biomesState[j].biomeWeight / sum;
            
            return biomesState;
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