using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace AffenCode.VoxelTerrain
{
    [CustomEditor(typeof(World))]
    public class WorldEditor : Editor
    {
        private World Target => target as World;

        private List<Vector3Int> _selectedBlocks = new();

        private static readonly Color SelectorColor = new Color(1, 0, 0, 0.66f);
        private static readonly Color SelectedBlockColor = new Color(1, 0, 0, 0.33f);

        private bool _editMode;

        private Vector3 _lastMouseWorldPosition;
        private Vector3Int _hoveredBlockPosition;

        private WorldTool _tool = WorldTool.None;
        
        
        private GUIStyle _normalButtonGuiStyleLeft;
        private GUIStyle _activeButtonGuiStyleLeft;
        private GUIStyle _normalButtonGuiStyleMid;
        private GUIStyle _activeButtonGuiStyleMid;
        private GUIStyle _normalButtonGuiStyleRight;
        private GUIStyle _activeButtonGuiStyleRight;

        public void Awake()
        {
            _normalButtonGuiStyleLeft = new GUIStyle(EditorStyles.miniButtonLeft);
            _activeButtonGuiStyleLeft = new GUIStyle(EditorStyles.miniButtonLeft);
            _normalButtonGuiStyleMid = new GUIStyle(EditorStyles.miniButtonMid);
            _activeButtonGuiStyleMid = new GUIStyle(EditorStyles.miniButtonMid);
            _normalButtonGuiStyleRight = new GUIStyle(EditorStyles.miniButtonRight);
            _activeButtonGuiStyleRight = new GUIStyle(EditorStyles.miniButtonRight);
        }

        public void OnEnable()
        {
            EditorApplication.update += ForceRedrawSceneView;
            SceneView.duringSceneGui += OnScene;
        }

        public void OnDisable()
        {
            EditorApplication.update -= ForceRedrawSceneView;
            SceneView.duringSceneGui -= OnScene;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (!Target.Mesh)
            {
                if (GUILayout.Button("Setup"))
                {
                    Target.Setup();    
                }
                
                return;
            }

            _editMode = EditorGUILayout.ToggleLeft("Edit Mode", _editMode);
            Tools.hidden = _editMode;

            if (_editMode)
            {
                EditorGUILayout.Space();
                DrawEditMode();
            }
        }

        private void DrawEditMode()
        {
            InspectorDrawTools();
        }

        private void InspectorDrawTools()
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("None", _tool == WorldTool.None ? _activeButtonGuiStyleLeft : _normalButtonGuiStyleLeft))
            {
                _tool = WorldTool.None;
            }
            
            if (GUILayout.Button("Add B", _tool == WorldTool.AddBlock ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.AddBlock;
            }
            if (GUILayout.Button("Remove B", _tool == WorldTool.RemoveBlock ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.RemoveBlock;
            }
            
            if (GUILayout.Button("Select B", _tool == WorldTool.SelectBlock ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.SelectBlock;
            }
            if (GUILayout.Button("Select F", _tool == WorldTool.SelectFace ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.SelectFace;
            }
            
            if (GUILayout.Button("Paint B", _tool == WorldTool.PaintBlock ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.PaintBlock;
            }
            if (GUILayout.Button("Paint F", _tool == WorldTool.PaintFace ? _activeButtonGuiStyleRight : _normalButtonGuiStyleRight))
            {
                _tool = WorldTool.PaintFace;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Selected Tool", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_tool.ToString(), EditorStyles.label);
            

            EditorGUILayout.EndVertical();
        }

        private void OnScene(SceneView sceneView)
        {
            if (!_editMode)
            {
                return;
            }
            
            if (!Target)
            {
                return;
            }
            
            if (!Target.Mesh)
            {
                return;
            }
            
            Selection.activeGameObject = Target.gameObject;

            ProcessSelectedBlocks();
            
            if (_tool != WorldTool.None)
            {
                ProcessBlockSelection();
            }
            
            ProcessEvents();
        }

        private void ProcessSelectedBlocks()
        {
            Handles.color = SelectedBlockColor;
            foreach (var selectedBlock in _selectedBlocks)
            {
                Handles.DrawAAConvexPolygon(GetPolygon(selectedBlock));
            };
        }

        private void ProcessBlockSelection()
        {
            Vector3 mousePosition = Event.current.mousePosition;
            var worldRay = HandleUtility.GUIPointToWorldRay(mousePosition);
            var mouseWorldPosition = _lastMouseWorldPosition;
            if (Physics.Raycast(worldRay, out var hit, 500f))
            {
                mouseWorldPosition = hit.point;
                _lastMouseWorldPosition = mouseWorldPosition;
            }
            
            Handles.color = SelectorColor;
            
            var cellPosition = mouseWorldPosition;
            cellPosition.x = Target.BlockSize * 0.5f + Mathf.FloorToInt(mouseWorldPosition.x); 
            cellPosition.y = Target.BlockSize + Mathf.FloorToInt(mouseWorldPosition.y - 0.5f); 
            cellPosition.z = Target.BlockSize * 0.5f + Mathf.FloorToInt(mouseWorldPosition.z); 
            _hoveredBlockPosition = ToVector3Int(cellPosition);
            
            Handles.DrawAAConvexPolygon(GetPolygon(_hoveredBlockPosition));
        }

        private void ProcessEvents()
        {
            var modeControlKey = false;
#if UNITY_EDITOR_OSX
            modeControlKey = Event.current.command;
#else
            modeControlKey = Event.current.control;
#endif
            var modeShiftKey = Event.current.shift;
            
            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    if (modeControlKey)
                    {
                        var menu = new GenericMenu();

                        foreach (var layer in Target.WorldAtlas.Layers)
                        {

                            menu.AddItem(new GUIContent("Set Layer/" + layer.name), false, GenericMenuSetLayer, 1);
                        }
                        menu.AddItem(new GUIContent("Set Height/Up"), false, GenericMenuSetLayer, 2);
                        menu.AddItem(new GUIContent("Set Height/Down"), false, GenericMenuSetLayer, 2);
                        menu.ShowAsContext();
                    }
                    else
                    {
                        if (modeShiftKey)
                        {
                            if (_selectedBlocks.Contains(_hoveredBlockPosition))
                            {
                                _selectedBlocks.Remove(_hoveredBlockPosition);
                            }
                            else
                            {
                                _selectedBlocks.Add(_hoveredBlockPosition);
                            }
                        }
                        else
                        {
                            _selectedBlocks.Clear();
                            _selectedBlocks.Add(_hoveredBlockPosition);
                        }
                    }
                }
            }
        }

        private Vector3[] GetPolygon(Vector3 cellPosition)
        {
            var cellSize = Target.BlockSize * 0.5f;
            var blockPosition = new[]
            {
                cellPosition + new Vector3(2f * cellSize, 0f, 2f * cellSize),
                cellPosition + new Vector3(0, 0f, 2f * cellSize),
                cellPosition + new Vector3(0, 0f, 0),
                cellPosition + new Vector3(2f * cellSize, 0f, 0),
                cellPosition + new Vector3(2f * cellSize, 0f, 2f * cellSize)
            };
            return blockPosition;
        }

        private void ForceRedrawSceneView()
        {
            SceneView.RepaintAll();
        }

        private void GenericMenuSetLayer(object userData)
        {
            
        }

        private Vector3Int ToVector3Int(Vector3 vector3)
        {
            return new Vector3Int((int) vector3.x, (int) vector3.y, (int) vector3.z);
        }
    }
}