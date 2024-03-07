using Unity.Mathematics;
using UnityEngine;

namespace TaigaGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Noise/Noise Texture", fileName = "Noise Texture")]
    public class NoiseTexture : NoiseProvider
    {
        [SerializeField] private Texture2D _texture;
        [SerializeField] private Result _result = Result.Clamp;
        
        public override void Normalize()
        {
        }

        public override float GetNoise(float x, float y, float z)
        {
            x = _result == Result.Clamp 
                ? Mathf.Clamp(x, 0, _texture.width - 1)
                : Mathf.Abs(x % _texture.width);
            
            y = _result == Result.Clamp 
                ? Mathf.Clamp(y, 0, _texture.height - 1)
                : Mathf.Abs(y % _texture.height);
            
            return _texture.GetPixel((int) x, (int) y).r;
        }

        public override float GetNoiseWithSeed(int seed, float x, float y, float z)
        {
            return GetNoise(x, y, z);
        }
        
        public int2 GetSize() => new int2(_texture.width, _texture.height);

        private enum Result
        {
            Clamp,
            Repeat
        }
    }
}