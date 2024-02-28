using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TravkinGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Descriptors/Biome", fileName = "Biome Descriptor")]
    public class BiomeDescriptor : ScriptableObject
    {
        [SerializeField] private Color _biomeMapColor = Color.white;
        [SerializeField] private List<VoxelDescriptor> _voxels;
        [SerializeField] private NoiseProvider _landscapeNoise;
        
        [Header("Definitions")]
        [SerializeField, Range(0, 1f)] private float _temperature;
        [SerializeField, Range(0, 1f)] private float _humidity;
        [SerializeField, Range(0, 1f)] private float _altitude;
        
        public Color BiomeMapColor => _biomeMapColor;
        public List<VoxelDescriptor> Voxels => _voxels;
        
        public float Temperature => _temperature;
        public float Humidity => _humidity;
        public float Altitude => _altitude;

        [Button("Generate random definitions")]
        private void GenerateRandomDefinitions()
        {
            _temperature = Random.value;
            _humidity = Random.value;
            _altitude = Random.value;
            _biomeMapColor = new Color(_temperature, _humidity, _altitude, 1f);
        }
    }
}