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
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!Target.Mesh)
            {
                if (GUILayout.Button("Setup"))
                {
                    Target.Setup();    
                }
                
                return;
            }

            if (GUILayout.Button("Edit Mode"))
            {
                _editMode = !_editMode;
                Tools.hidden = _editMode;
            }
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
            
            Vector3 mousePosition = Event.current.mousePosition;
            var worldRay = HandleUtility.GUIPointToWorldRay(mousePosition);
            
            var mouseWorldPosition = Vector3.zero;
            if (Physics.Raycast(worldRay, out var hit, 500f))
            {
                mouseWorldPosition = hit.point;
            }



            Handles.color = SelectedBlockColor;
            foreach (var selectedBlock in _selectedBlocks)
            {
                Handles.DrawAAConvexPolygon(GetPolygon(selectedBlock));
            };

            if (_selectedBlocks.Count > 0)
            {
                var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                foreach (var selectedBlock in _selectedBlocks)
                {
                    max.x = Mathf.Max(selectedBlock.x, max.x);
                    max.y = Mathf.Max(selectedBlock.y, max.y);
                    max.z = Mathf.Max(selectedBlock.z, max.z);
                    
                    min.x = Mathf.Min(selectedBlock.x, min.x);
                    min.y = Mathf.Min(selectedBlock.y, min.y);
                    min.z = Mathf.Min(selectedBlock.z, min.z);
                }
                var center = 0.5f * (max + min);
                Handles.PositionHandle(center + new Vector3(0.5f, 0, 0.5f), Quaternion.identity);
            }
            
            
            
            
            Handles.color = SelectorColor;
            
            var cellPosition = mouseWorldPosition;
            cellPosition.x = Target.BlockSize * 0.5f + Mathf.FloorToInt(mouseWorldPosition.x); 
            cellPosition.y = Target.BlockSize + Mathf.FloorToInt(mouseWorldPosition.y - 0.5f); 
            cellPosition.z = Target.BlockSize * 0.5f + Mathf.FloorToInt(mouseWorldPosition.z); 
            var hoveredBlockPosition = ToVector3Int(cellPosition);
            
            Handles.DrawAAConvexPolygon(GetPolygon(hoveredBlockPosition));

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
                        var blockPosition = ToVector3Int(cellPosition);
                        if (modeShiftKey)
                        {
                            if (_selectedBlocks.Contains(blockPosition))
                            {
                                _selectedBlocks.Remove(blockPosition);
                            }
                            else
                            {
                                _selectedBlocks.Add(blockPosition);
                            }
                        }
                        else
                        {
                            _selectedBlocks.Clear();
                            _selectedBlocks.Add(blockPosition);
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