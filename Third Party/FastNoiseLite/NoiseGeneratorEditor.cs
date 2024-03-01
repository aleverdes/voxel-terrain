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
        
        private NoiseGenerator _target => (NoiseGenerator) target;

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
            
            EditorGUI.BeginDisabledGroup(_target.IsBaked);
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck()) 
                Redraw();
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Normalize"))
            {
                _target.Normalize();
                EditorUtility.SetDirty(_target);
            }

            if (_target.SettingApplied && GUILayout.Button("Reset Noise Generator Settings"))
                _target.ResetNoiseGeneratorSettings();

            if (_target.IsBaked)
            {
                if (GUILayout.Button("Clear Baked Noise"))
                {
                    _target.ClearBakedData();
                    EditorUtility.SetDirty(_target);
                }
            }
            else
            {
                if (GUILayout.Button("Bake Noise"))
                {
                    _target.Bake();
                    EditorUtility.SetDirty(_target);
                }
            }
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
            var noise = _target.GetNoise(x, y);
            var result = Color.white * noise;
            result.a = 1f;
            return result;
        }
    }
}
#endif