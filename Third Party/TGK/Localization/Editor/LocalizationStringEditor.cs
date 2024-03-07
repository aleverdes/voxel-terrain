#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TaigaGames.Localization
{
    [CustomEditor(typeof(LocalizableString))]
    public class LocalizationStringEditor : Editor
    {
        private SerializedProperty _english;
        private SerializedProperty _russian;
        private SerializedProperty _czech;
        private SerializedProperty _danish;
        private SerializedProperty _dutch;
        private SerializedProperty _german;
        private SerializedProperty _greek;
        private SerializedProperty _finnish;
        private SerializedProperty _french;
        private SerializedProperty _italian;
        private SerializedProperty _japanese;
        private SerializedProperty _korean;
        private SerializedProperty _norwegian;
        private SerializedProperty _polish;
        private SerializedProperty _portuguese;
        private SerializedProperty _romanian;
        private SerializedProperty _spanish;
        private SerializedProperty _swedish;
        private SerializedProperty _turkish;
        private SerializedProperty _simplifiedChinese;
        private SerializedProperty _traditionalChinese;
        
        private void OnEnable()
        {
            _english = serializedObject.FindProperty("English");
            _russian = serializedObject.FindProperty("Russian");
            _czech = serializedObject.FindProperty("Czech");
            _danish = serializedObject.FindProperty("Danish");
            _dutch = serializedObject.FindProperty("Dutch");
            _german = serializedObject.FindProperty("German");
            _greek = serializedObject.FindProperty("Greek");
            _finnish = serializedObject.FindProperty("Finnish");
            _french = serializedObject.FindProperty("French");
            _italian = serializedObject.FindProperty("Italian");
            _japanese = serializedObject.FindProperty("Japanese");
            _korean = serializedObject.FindProperty("Korean");
            _norwegian = serializedObject.FindProperty("Norwegian");
            _polish = serializedObject.FindProperty("Polish");
            _portuguese = serializedObject.FindProperty("Portuguese");
            _romanian = serializedObject.FindProperty("Romanian");
            _spanish = serializedObject.FindProperty("Spanish");
            _swedish = serializedObject.FindProperty("Swedish");
            _turkish = serializedObject.FindProperty("Turkish");
            _simplifiedChinese = serializedObject.FindProperty("SimplifiedChinese");
            _traditionalChinese = serializedObject.FindProperty("TraditionalChinese");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            DrawMultilineTextArea("English", _english);
            DrawMultilineTextArea("Russian", _russian);
            DrawMultilineTextArea("Czech", _czech);
            DrawMultilineTextArea("Danish", _danish);
            DrawMultilineTextArea("Dutch", _dutch);
            DrawMultilineTextArea("German", _german);
            DrawMultilineTextArea("Greek", _greek);
            DrawMultilineTextArea("Finnish", _finnish);
            DrawMultilineTextArea("French", _french);
            DrawMultilineTextArea("Italian", _italian);
            DrawMultilineTextArea("Japanese", _japanese);
            DrawMultilineTextArea("Korean", _korean);
            DrawMultilineTextArea("Norwegian", _norwegian);
            DrawMultilineTextArea("Polish", _polish);
            DrawMultilineTextArea("Portuguese", _portuguese);
            DrawMultilineTextArea("Romanian", _romanian);
            DrawMultilineTextArea("Spanish", _spanish);
            DrawMultilineTextArea("Swedish", _swedish);
            DrawMultilineTextArea("Turkish", _turkish);
            DrawMultilineTextArea("Simplified Chinese", _simplifiedChinese);
            DrawMultilineTextArea("Traditional Chinese", _traditionalChinese);
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private static void DrawMultilineTextArea(string label, SerializedProperty serializedProperty)
        {
            EditorGUILayout.LabelField(label);
            serializedProperty.stringValue = EditorGUILayout.TextArea(serializedProperty.stringValue, Styles.Multiline, GUILayout.MinHeight(Styles.MinMultilineHeight));
        }
        
        private static class Styles
        {
            public const float MinMultilineHeight = 64;
            
            public static readonly GUIStyle Multiline = new(GUI.skin.textArea)
            {
                wordWrap = true
            };
        }
    }
}
#endif