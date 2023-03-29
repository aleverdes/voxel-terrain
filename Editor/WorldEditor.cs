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

        private Vector3 _mouseWorldPosition;
        private Vector3Int _hoveredBlockPosition;

        private WorldTool _tool = WorldTool.None;
        
        private GUIStyle _normalButtonGuiStyleLeft;
        private GUIStyle _activeButtonGuiStyleLeft;
        private GUIStyle _normalButtonGuiStyleMid;
        private GUIStyle _activeButtonGuiStyleMid;
        private GUIStyle _normalButtonGuiStyleRight;
        private GUIStyle _activeButtonGuiStyleRight;

        private float _heightChangingBrushSize = 0.5f;
        
        public void Awake()
        {
            _normalButtonGuiStyleLeft = new GUIStyle(EditorStyles.miniButtonLeft);
            _activeButtonGuiStyleLeft = new GUIStyle(EditorStyles.miniButtonLeft);
            _normalButtonGuiStyleMid = new GUIStyle(EditorStyles.miniButtonMid);
            _activeButtonGuiStyleMid = new GUIStyle(EditorStyles.miniButtonMid);
            _normalButtonGuiStyleRight = new GUIStyle(EditorStyles.miniButtonRight);
            _activeButtonGuiStyleRight = new GUIStyle(EditorStyles.miniButtonRight);

            _heightChangingBrushSize = Target.BlockSize;
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

            if (GUILayout.Button("Reset"))
            {
                Target.Setup();    
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
            switch (_tool)
            {
                case WorldTool.None:
                    break;
                case WorldTool.AddBlock:
                    InspectorDrawAddBlockTool();
                    break;
                case WorldTool.RemoveBlock:
                    InspectorDrawRemoveBlockTool();
                    break;
                case WorldTool.SelectBlock:
                    InspectorDrawSelectBlockTool();
                    break;
                case WorldTool.SelectFace:
                    InspectorDrawSelectFaceTool();
                    break;
                case WorldTool.PaintBlock:
                    InspectorDrawPaintBlockTool();
                    break;
                case WorldTool.PaintFace:
                    InspectorDrawPaintFaceTool();
                    break;
                case WorldTool.VertexHeight:
                    InspectorDrawVertexHeightTool();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InspectorDrawTools()
        {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("-", _tool == WorldTool.None ? _activeButtonGuiStyleLeft : _normalButtonGuiStyleLeft))
            {
                _tool = WorldTool.None;
            }
            if (GUILayout.Button("AB", _tool == WorldTool.AddBlock ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.AddBlock;
            }
            if (GUILayout.Button("RB", _tool == WorldTool.RemoveBlock ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.RemoveBlock;
            }
            if (GUILayout.Button("SB", _tool == WorldTool.SelectBlock ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.SelectBlock;
            }
            if (GUILayout.Button("SF", _tool == WorldTool.SelectFace ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.SelectFace;
            }
            if (GUILayout.Button("PB", _tool == WorldTool.PaintBlock ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.PaintBlock;
            }
            if (GUILayout.Button("PF", _tool == WorldTool.PaintFace ? _activeButtonGuiStyleMid : _normalButtonGuiStyleMid))
            {
                _tool = WorldTool.PaintFace;
            }
            if (GUILayout.Button("VH", _tool == WorldTool.VertexHeight ? _activeButtonGuiStyleRight : _normalButtonGuiStyleRight))
            {
                _tool = WorldTool.VertexHeight;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Selected Tool", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_tool.ToString(), EditorStyles.label);
            

            EditorGUILayout.EndVertical();
        }

        private void InspectorDrawAddBlockTool()
        {
            
        }

        private void InspectorDrawRemoveBlockTool()
        {
            
        }

        private void InspectorDrawSelectBlockTool()
        {
            
        }

        private void InspectorDrawSelectFaceTool()
        {
            
        }

        private void InspectorDrawPaintBlockTool()
        {
            
        }

        private void InspectorDrawPaintFaceTool()
        {
            
        }

        private void InspectorDrawVertexHeightTool()
        {
            
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
            
            Selection.activeGameObject = Target.gameObject;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));


            UpdateMouseWorldPosition();
            
            ProcessSelectedBlocks();
            
            if (_tool != WorldTool.None && _tool != WorldTool.VertexHeight)
            {
                ProcessBlockSelection();
            }
            else if (_tool == WorldTool.VertexHeight)
            {
                ProcessVertexHeight();
            }
            
            ProcessEvents();
        }

        private void UpdateMouseWorldPosition()
        {
            Vector3 mousePosition = Event.current.mousePosition;
            var worldRay = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(worldRay, out var hit, 500f))
            {
                _mouseWorldPosition = hit.point;
            }
        }

        private void ProcessSelectedBlocks()
        {
            Handles.color = SelectedBlockColor;
            foreach (var selectedBlock in _selectedBlocks)
            {
                DrawBlockSelection(selectedBlock);
            };
        }

        private void ProcessBlockSelection()
        {
            Handles.color = SelectorColor;
            
            var cellPosition = _mouseWorldPosition;
            
            cellPosition.x = _mouseWorldPosition.x / Target.BlockSize; 
            cellPosition.y = (_mouseWorldPosition.y - 0.001f) / Target.BlockSize; 
            cellPosition.z = _mouseWorldPosition.z / Target.BlockSize;

            cellPosition.x = Mathf.Clamp(cellPosition.x, 0, Target.WorldSize.x - 1);
            cellPosition.y = Mathf.Clamp(cellPosition.y, 0, Target.WorldSize.y - 1);
            cellPosition.z = Mathf.Clamp(cellPosition.z, 0, Target.WorldSize.z - 1);
            
            _hoveredBlockPosition = ToVector3Int(cellPosition);

            DrawBlockSelection(_hoveredBlockPosition);
        }

        private void ProcessVertexHeight()
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, _heightChangingBrushSize, 3f);
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, 0.01f, 3f);
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
                        if (_tool == WorldTool.SelectBlock)
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
                        else if (_tool == WorldTool.RemoveBlock)
                        {
                            ref var block = ref Target.GetBlock(_hoveredBlockPosition);
                            block.Void = true;
                            Target.GenerateChunkMeshes();
                        }
                    }
                }
            }
            else if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                if (_tool == WorldTool.VertexHeight)
                {
                    if (Event.current.keyCode is KeyCode.RightBracket)
                    {
                        _heightChangingBrushSize += 0.2f * Target.BlockSize;
                        _heightChangingBrushSize = Mathf.Min(_heightChangingBrushSize, 0.5f * Mathf.Max(Target.ChunkSize.x, Target.ChunkSize.y));
                    }
                    else if (Event.current.keyCode is KeyCode.LeftBracket)
                    {
                        _heightChangingBrushSize -= 0.2f * Target.BlockSize;
                        _heightChangingBrushSize = Mathf.Max(_heightChangingBrushSize, 0.5f * Target.BlockSize);
                    }
                }
            }
            else if (Event.current.type == EventType.KeyUp)
            {
            }
        }

        private void DrawBlockSelection(Vector3Int blockPosition)
        {
            if (blockPosition.x > Target.WorldSize.x - 1 || blockPosition.x < 0)
            {
                return;
            }

            if (blockPosition.y > Target.WorldSize.y - 1 || blockPosition.y < 0)
            {
                return;
            }

            if (blockPosition.z > Target.WorldSize.z - 1 || blockPosition.z < 0)
            {
                return;
            }

            ref var block = ref Target.GetBlock(blockPosition);

            if (block.Void)
            {
                return;
            }

            var t = Target.BlockSize;
            var position = (Vector3) blockPosition;
            position *= t;

            var face = default(BlockFace);
            
            face = block.Top;
            if (face.Draw 
                && (block.Position.y < Target.WorldSize.y - 1 && Target.GetBlock(blockPosition.x, blockPosition.y + 1, blockPosition.z).Void
                    || block.Position.y == Target.WorldSize.y - 1))
            {
                Handles.DrawAAConvexPolygon(new[]
                {
                    position + new Vector3(t, t, t),
                    position + new Vector3(0, t, t),
                    position + new Vector3(0, t, 0),
                    position + new Vector3(t, t, 0),
                    position + new Vector3(t, t, t)
                });
            }
            
            face = block.Bottom;
            if (face.Draw 
                && (block.Position.y > 0 && Target.GetBlock(blockPosition.x, blockPosition.y - 1, blockPosition.z).Void
                    || block.Position.y == 0))
            {
                Handles.DrawAAConvexPolygon(new[]
                {
                    position + new Vector3(t, 0, t),
                    position + new Vector3(0, 0, t),
                    position + new Vector3(0, 0, 0),
                    position + new Vector3(t, 0, 0),
                    position + new Vector3(t, 0, t)
                });
            }
            
            face = block.Forward;
            if (face.Draw 
                && (block.Position.z < Target.WorldSize.z - 1 && Target.GetBlock(blockPosition.x, blockPosition.y, blockPosition.z + 1).Void
                    || block.Position.z == Target.WorldSize.z - 1))
            {
                Handles.DrawAAConvexPolygon(new[]
                {
                    position + new Vector3(t, t, t),
                    position + new Vector3(0, t, t),
                    position + new Vector3(0, 0, t),
                    position + new Vector3(t, 0, t),
                    position + new Vector3(t, t, t)
                });
            }
            
            face = block.Back;
            if (face.Draw 
                && (block.Position.z > 0 && Target.GetBlock(blockPosition.x, blockPosition.y, blockPosition.z - 1).Void
                    || block.Position.z == 0))
            {
                Handles.DrawAAConvexPolygon(new[]
                {
                    position + new Vector3(t, t, 0),
                    position + new Vector3(0, t, 0),
                    position + new Vector3(0, 0, 0),
                    position + new Vector3(t, 0, 0),
                    position + new Vector3(t, t, 0)
                });
            }
            
            face = block.Left;
            if (face.Draw 
                && (block.Position.x > 0 && Target.GetBlock(blockPosition.x - 1, blockPosition.y, blockPosition.z).Void
                    || block.Position.x == 0))
            {
                Handles.DrawAAConvexPolygon(new[]
                {
                    position + new Vector3(0, t, t),
                    position + new Vector3(0, t, 0),
                    position + new Vector3(0, 0, 0),
                    position + new Vector3(0, 0, t),
                    position + new Vector3(0, t, t)
                });
            }
            
            face = block.Right;
            if (face.Draw 
                && (block.Position.x < Target.WorldSize.x - 1 && Target.GetBlock(blockPosition.x + 1, blockPosition.y, blockPosition.z).Void
                    || block.Position.x == Target.WorldSize.x - 1))
            {
                Handles.DrawAAConvexPolygon(new[]
                {
                    position + new Vector3(t, t, t),
                    position + new Vector3(t, t, 0),
                    position + new Vector3(t, 0, 0),
                    position + new Vector3(t, 0, t),
                    position + new Vector3(t, t, t)
                });
            }
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