using UnityEngine;

namespace TravkinGames.Voxels
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
            var xInt = Mathf.FloorToInt(x * _texture.width);
            xInt = _result == Result.Clamp ? Mathf.Clamp(xInt, 0, _texture.width) : Mathf.Abs(xInt % _texture.width);

            var yInt = Mathf.FloorToInt(z * _texture.height);
            yInt = _result == Result.Clamp ? Mathf.Clamp(yInt, 0, _texture.height) : Mathf.Abs(yInt % _texture.height);
            
            return _texture.GetPixel(xInt, yInt).r;
        }

        public override float GetNoiseWithSeed(int seed, float x, float y, float z)
        {
            return GetNoise(x, y, z);
        }

        private enum Result
        {
            Clamp,
            Repeat
        }
    }
}