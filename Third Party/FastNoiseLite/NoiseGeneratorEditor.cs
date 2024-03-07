#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TaigaGames.Voxels
{
    [CustomEditor(typeof(NoiseGenerator))]
    public class NoiseGeneratorEditor : Editor
    {
        private Texture2D _outputTexture;
        private Vector2Int _outputTextureSize = new Vector2Int(256, 256);

        public override bool HasPreviewGUI() => true;
        
        private NoiseGenerator Target => (NoiseGenerator) target;

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
            
            EditorGUI.BeginDisabledGroup(Target.IsBaked);
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck()) 
                Redraw();
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Normalize"))
            {
                Target.Normalize();
                EditorUtility.SetDirty(Target);
            }

            if (Target.IsBaked)
            {
                if (GUILayout.Button("Clear Baked Noise"))
                {
                    Target.ClearBakedData();
                    EditorUtility.SetDirty(Target);
                }
            }
            else
            {
                if (GUILayout.Button("Bake Noise"))
                {
                    Target.Bake();
                    EditorUtility.SetDirty(Target);
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
            for (var x = 0; x < _outputTextureSize.x; x++)
            for (var y = 0; y < _outputTextureSize.y; y++)
                _outputTexture.SetPixel(x, y, GetNoiseColor(x, y));

            _outputTexture.Apply();
        }

        private Color GetNoiseColor(float x, float y)
        {
            var noise = Target.GetNoise(x, y);
            var result = Color.white * noise;
            result.a = 1f;
            return result;
        }
    }
}
#endif