#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TaigaGames.Voxels
{
    public class BiomeMapPreview : EditorWindow
    {
        private BiomeMapGenerator _biomeMapGenerator;
        private BiomeMapGenerator _prevBiomeMapGenerator;
        
        private int _seed;
        private int _prevSeed;
        
        private float _scale = 1f;
        private float _prevScale = 1f;
        
        private Texture2D _previewTexture;

        public static void ShowWindow(BiomeMapGenerator biomeMapGenerator)
        {
            var window = GetWindow<BiomeMapPreview>("Biome Map Preview");
            window._biomeMapGenerator = biomeMapGenerator;
            window._prevBiomeMapGenerator = biomeMapGenerator;
            window.Redraw();
        }
        
        private void Awake()
        {
            _previewTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
        }

        private void OnGUI()
        {
            _biomeMapGenerator = (BiomeMapGenerator) EditorGUILayout.ObjectField("Biome Map Generator", _biomeMapGenerator, typeof(BiomeMapGenerator), false);
            _seed = EditorGUILayout.IntField("Seed", _seed);
            _scale = EditorGUILayout.Slider("Scale", _scale, 0.1f, 100f);
            
            if (_biomeMapGenerator != _prevBiomeMapGenerator)
            {
                _prevBiomeMapGenerator = _biomeMapGenerator;
                Redraw();
            }
            
            if (_seed != _prevSeed)
            {
                _prevSeed = _seed;
                Redraw();
            }
            
            if (Mathf.Abs(_scale - _prevScale) > Mathf.Epsilon)
            {
                _prevScale = _scale;
                Redraw();
            }
            
            EditorGUI.DrawPreviewTexture(new Rect(0, 64, position.width, position.height), _previewTexture);
        }

        private void Redraw()
        {
            for (var x = -128; x < 128; x++)
            for (var y = -128; y < 128; y++)
            {
                var biomesState = _biomeMapGenerator.GetVoxelBiomeState(_seed, new Vector3Int((int) _scale * x, (int) _scale * y, 0));
                _previewTexture.SetPixel(x, y, biomesState.BestBiome.BiomeMapColor);
            }
            _previewTexture.Apply();
        }
    }
}
#endif