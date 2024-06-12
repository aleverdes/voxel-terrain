using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TaigaGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Noise/Noise Texture", fileName = "Noise Texture")]
    public class NoiseTexture : NoiseProvider
    {
        [SerializeField] private Texture2D _texture;
        [SerializeField] private Result _result = Result.Clamp;
        [SerializeField, HideInInspector] private int2 _size;
        [SerializeField, HideInInspector] private float[] _pixels;
            
        public override void Normalize()
        {
            _size = new int2(_texture.width, _texture.height);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        public override float GetNoise(float x, float y, float z)
        {
            x = _result == Result.Clamp 
                ? Mathf.Clamp(x, 0, _size.x - 1)
                : Mathf.Abs(x % _size.x);
            
            y = _result == Result.Clamp 
                ? Mathf.Clamp(y, 0, _size.y - 1)
                : Mathf.Abs(y % _size.y);
            
            return _texture.GetPixel((int) x, (int) y).r;
        }

        public override float GetNoiseWithSeed(int seed, float x, float y, float z)
        {
            return GetNoise(x, y, z);
        }
        
        public int2 GetSize() => _size;

        private enum Result
        {
            Clamp,
            Repeat
        }
    }
}