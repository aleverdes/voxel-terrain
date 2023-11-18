using UnityEngine;

namespace AleVerDes.Voxels
{
    public interface INoiseProvider
    {
        void Normalize();
        float GetNoise(Vector2 position);
        float GetNoise(float x, float z);
        float GetNoise(Vector3 position);
        float GetNoise(float x, float y, float z);
    }
}