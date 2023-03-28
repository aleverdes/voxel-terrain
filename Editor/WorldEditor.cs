using System;
using System.Collections.Generic;
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
            
            var color = new Color(1, 0, 0, 0.5f);
            Handles.color = color;
            
            var cellPosition = mouseWorldPosition;
            cellPosition.x = Target.BlockSize * 0.5f + Mathf.FloorToInt(mouseWorldPosition.x); 
            cellPosition.y = Target.BlockSize + Mathf.FloorToInt(mouseWorldPosition.y - 0.5f); 
            cellPosition.z = Target.BlockSize * 0.5f + Mathf.FloorToInt(mouseWorldPosition.z); 
            
            var cellSize = Target.BlockSize * 0.5f;
            
            Handles.DrawAAConvexPolygon(
                cellPosition + new Vector3(cellSize, 0f, cellSize), 
                cellPosition + new Vector3(-cellSize, 0f, cellSize), 
                cellPosition + new Vector3(-cellSize, 0f, -cellSize), 
                cellPosition + new Vector3(cellSize, 0f, -cellSize),
                cellPosition + new Vector3(cellSize, 0f, cellSize));

            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Set Layer"), false, GenericMenuSetLayer, 1);
                    menu.AddItem(new GUIContent("Up Height"), false, GenericMenuSetLayer, 2);
                    menu.ShowAsContext();
                }
            }
        }

        private void ForceRedrawSceneView()
        {
            SceneView.RepaintAll();
        }

        private void GenericMenuSetLayer(object userData)
        {
            
        }
    }
}