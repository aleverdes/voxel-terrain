using UnityEngine;

namespace TaigaGames.Voxels
{
    public abstract class NoiseProvider : ScriptableObject, INoiseProvider
    {
        public abstract void Normalize();

        public float GetNoise(Vector2 position)
        {
            return GetNoise(position.x, 0, position.y);
        }

        public float GetNoise(float x, float y)
        {
            return GetNoise(x, y, 0);
        }

        public float GetNoise(Vector3 position)
        {
            return GetNoise(position.x, position.y, position.z);
        }

        public abstract float GetNoise(float x, float y, float z);
        
        public abstract float GetNoiseWithSeed(int seed, float x, float y, float z);
    }
}