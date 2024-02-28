#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TravkinGames.Voxels
{
    public class BiomeMapPreview : EditorWindow
    {
        private BiomeMapGenerator _biomeMapGenerator;
        private BiomeMapGenerator _prevBiomeMapGenerator;
        
        private int _seed;
        private int _prevSeed;
        
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
            
            EditorGUI.DrawPreviewTexture(new Rect(0, 64, position.width, position.height), _previewTexture);
        }

        private void Redraw()
        {
            for (var i = 0; i < 256; i++)
            for (var j = 0; j < 256; j++)
            {
                var biomesState = _biomeMapGenerator.GetBiomesState(_seed, new Vector2Int(j, i));
                var maxWeight = biomesState[0].biomeWeight;
                var maxBiome = biomesState[0].biomeDescriptor;
                for (var k = 1; k < biomesState.Length; k++)
                    if (biomesState[k].biomeWeight > maxWeight)
                        (maxBiome, maxWeight) = (biomesState[k].biomeDescriptor, biomesState[k].biomeWeight);
                _previewTexture.SetPixel(j, i, maxBiome.BiomeMapColor);
            }
            _previewTexture.Apply();
        }
    }
}
#endif