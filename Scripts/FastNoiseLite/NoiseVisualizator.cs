#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AleVerDes.Voxels
{
    public class NoiseVisualizator : EditorWindow
    {
        private NoiseGenerator _noiseGenerator;
        private Texture2D _outputTexture;
        private Vector2Int _outputTextureSize = new Vector2Int(512, 512);
        
        [MenuItem("Tools/Noise Visualization")]
        public static void ShowWindow()
        {
            var window = CreateWindow<NoiseVisualizator>();
            window.minSize = new Vector2(512, 600);

            if (Selection.activeObject is NoiseGenerator noiseGenerator)
                window._noiseGenerator = noiseGenerator;
            
            window._outputTexture = new Texture2D(window._outputTextureSize.x, window._outputTextureSize.y);
        }

        private void OnGUI()
        {
            if (_outputTexture == null)
                _outputTexture = new Texture2D(_outputTextureSize.x, _outputTextureSize.y);
            
            EditorGUI.BeginChangeCheck();
            _noiseGenerator = (NoiseGenerator) EditorGUILayout.ObjectField("Noise Generator:", _noiseGenerator, typeof(NoiseGenerator), false);
            _outputTextureSize = EditorGUILayout.Vector2IntField("Output Texture Size:", _outputTextureSize);
            if (EditorGUI.EndChangeCheck())
            {
                _outputTexture = new Texture2D(_outputTextureSize.x, _outputTextureSize.y);
                Redraw();
            }

            if (GUILayout.Button("Redraw"))
                Redraw();
            
            if (_outputTexture) 
                EditorGUI.DrawTextureTransparent(new Rect(0, 100, _outputTextureSize.x, _outputTextureSize.y), _outputTexture);
        }

        private void Redraw()
        {
            for (var i = 0; i < _outputTextureSize.y; i++)
            for (var j = 0; j < _outputTextureSize.x; j++)
                _outputTexture.SetPixel(j, i, GetNoiseColor(j, i));

            _outputTexture.Apply();
        }

        private Color GetNoiseColor(float x, float y)
        {
            var noise = _noiseGenerator.GetNoise(x, y);
            var result = Color.white * noise;
            result.a = 1f;
            return result;
        }
    }
}
#endif