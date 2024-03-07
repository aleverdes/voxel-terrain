#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TaigaGames.Voxels
{
    [CustomEditor(typeof(NoiseTexture))]
    public class NoiseTextureEditor : Editor
    {
        private Texture2D _outputTexture;

        public override bool HasPreviewGUI() => true;
        
        private NoiseTexture Target => (NoiseTexture) target;

        private void OnEnable()
        {
            if (!_outputTexture)
            {
                _outputTexture = new Texture2D(Target.GetSize().x, Target.GetSize().y);
                Redraw();
            }
        }

        public override void OnInspectorGUI()
        {
            if (!_outputTexture)
            {
                _outputTexture = new Texture2D(_outputTexture.width, _outputTexture.height);
                Redraw();
            }
            
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck()) 
                Redraw();
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (!_outputTexture)
            {
                _outputTexture = new Texture2D(_outputTexture.width, _outputTexture.height);
                Redraw();
            }
            
            EditorGUI.DrawTextureTransparent(r, _outputTexture);
        }

        private void Redraw()
        {
            for (var x = 0; x < _outputTexture.width; x++)
            for (var y = 0; y < _outputTexture.height; y++)
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