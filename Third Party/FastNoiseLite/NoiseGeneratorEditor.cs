#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TravkinGames.Voxels
{
    [CustomEditor(typeof(NoiseGenerator))]
    public class NoiseGeneratorEditor : Editor
    {
        private Texture2D _outputTexture;
        private Vector2Int _outputTextureSize = new Vector2Int(256, 256);

        public override bool HasPreviewGUI() => true;

        private void OnEnable()
        {
            if (!_outputTexture)
            {
                _outputTexture = new Texture2D(_outputTextureSize.x, _outputTextureSize.y);
                Redraw();
            }
        }

        public override void OnInspectorGUI()
        {
            if (!_outputTexture)
            {
                _outputTexture = new Texture2D(_outputTextureSize.x, _outputTextureSize.y);
                Redraw();
            }
            
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck()) 
                Redraw();

            if (GUILayout.Button("Normalize"))
                ((NoiseGenerator)target).Normalize();
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (!_outputTexture)
            {
                _outputTexture = new Texture2D(_outputTextureSize.x, _outputTextureSize.y);
                Redraw();
            }
            
            EditorGUI.DrawTextureTransparent(r, _outputTexture);
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
            var noise = ((NoiseGenerator) target).GetNoise(x, y);
            var result = Color.white * noise;
            result.a = 1f;
            return result;
        }
    }
}
#endif