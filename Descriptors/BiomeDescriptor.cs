using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TaigaGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Descriptors/Biome", fileName = "Biome Descriptor")]
    public class BiomeDescriptor : ScriptableObject
    {
        [SerializeField] private Color _biomeMapColor = Color.white;
        [SerializeField] private VoxelDescriptor _defaultVoxel;
        [SerializeField] private List<VoxelMap> _voxels;
        [SerializeField] private NoiseProvider _landscapeNoise;
        [SerializeField] private Vector2Int _minMaxHeight = new Vector2Int(8, 16);
        
        [Header("Definitions")]
        [SerializeField, Range(0, 1f)] private float _temperature;
        [SerializeField, Range(0, 1f)] private float _humidity;
        [SerializeField, Range(0, 1f)] private float _altitude;
        
        public Color BiomeMapColor => _biomeMapColor;
        public NoiseProvider LandscapeNoise => _landscapeNoise;
        
        public float Temperature => _temperature;
        public float Humidity => _humidity;
        public float Altitude => _altitude;
        
        public bool IsVoxelExists(Vector3 position, float noise)
        {
            if (position.y < _minMaxHeight.x)
                return true;
            if (position.y > _minMaxHeight.y)
                return false;
            var t = Mathf.InverseLerp(_minMaxHeight.x, _minMaxHeight.y, position.y);
            return noise > t;
        }
        
        public VoxelDescriptor GetVoxel(int seed, Vector3 position)
        {
            var bestVoxel = _defaultVoxel;
            var bestNoise = float.MinValue;
            foreach (var voxelMap in _voxels)
            {
                var currentNoise = voxelMap.Noise.GetNoiseWithSeed(seed, position.x, position.y, position.z);
                if (currentNoise > bestNoise)
                {
                    bestVoxel = voxelMap.Voxel;
                    bestNoise = currentNoise;
                }
            }

            return bestVoxel;
        }

        [Button("Generate random definitions")]
        private void GenerateRandomDefinitions()
        {
            _temperature = Random.value;
            _humidity = Random.value;
            _altitude = Random.value;
            _biomeMapColor = new Color(_temperature, _humidity, _altitude, 1f);
        }
        
        [Serializable]
        private struct VoxelMap
        {
            public VoxelDescriptor Voxel;
            public NoiseProvider Noise;
        }
    }
}