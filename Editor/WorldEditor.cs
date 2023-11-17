using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AleVerDes.UnityUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace AleVerDes.VoxelTerrain
{
    [CustomEditor(typeof(World))]
    public class WorldEditor : Editor
    {
        private World Target => target as World;

        private readonly List<Vector2Int> _selectedCells = new();
        private readonly List<Vector2Int> _hoveredVertices = new();
        private readonly List<Vector2Int> _hoveredCells = new();

        private static readonly Color SelectorColor = new Color(1, 0, 0, 0.66f);
        private static readonly Color SelectedCellColor = new Color(1, 0, 0, 0.33f);

        private Vector3 _mouseWorldPosition;
        private Vector2Int _hoveredCellPosition;

        private VisualElement _rootVisualElement;
        private VisualTreeAsset _visualTreeAsset;
        private Dictionary<WorldTool, Button> _toolbarButton = new();
        
        private float _heightChangingBrushSize = 0.5f;
        private float _paintingBrushSize = 0.5f;

        private bool _leftMouseButtonIsPressed;

        private float _editorDeltaTime;
        private float _lastTimeSinceStartup;
        
        private SerializedProperty _worldSettingsProperty;
        
        private Dictionary<VoxelTerrainLayer, bool> _atlasLayerFoldouts;
        private Texture2D _selectedAtlasLayerTexture;
        private byte _selectedCellTextureIndex;
        private VoxelTerrainLayer _selectedAtlasLayer;
        private int _selectedGridElement;

        private bool _initialized;

        private WorldTool Tool
        {
            get => Target.LastWorldTool;
            set => Target.LastWorldTool = value;
        }

        private void Initialize()
        {
            if (_initialized)
                return;
            
            if (!Target.WorldSettings)
                return;

            _worldSettingsProperty ??= serializedObject.FindProperty("_worldSettings");

            var anyShown = false;
            _atlasLayerFoldouts = new Dictionary<VoxelTerrainLayer, bool>();
            foreach (var layer in Target.WorldSettings.WorldAtlas.Layers)
            {
                _atlasLayerFoldouts.Add(layer, !anyShown);
                anyShown = true;
            }

            _initialized = true;
        }
        
        public void OnEnable()
        {
            EditorApplication.update += ForceRedrawSceneView;
            EditorApplication.update += SetEditorDeltaTime;
            SceneView.duringSceneGui += OnScene;
            
            _heightChangingBrushSize = Target.WorldSettings.BlockSize;
            _paintingBrushSize = Target.WorldSettings.BlockSize;
        }

        public void OnDisable()
        {
            EditorApplication.update -= ForceRedrawSceneView;
            EditorApplication.update -= SetEditorDeltaTime;
            SceneView.duringSceneGui -= OnScene;
        }

        public override void OnInspectorGUI()
        {
            Initialize();
            
            EditorGUILayout.PropertyField(_worldSettingsProperty);
            
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("-")) 
                SetTool(WorldTool.None);

            if (GUILayout.Button("S")) 
                SetTool(WorldTool.Selection);

            if (GUILayout.Button("P")) 
                SetTool(WorldTool.Painting);

            if (GUILayout.Button("H")) 
                SetTool(WorldTool.Height);

            if (GUILayout.Button("x")) 
                SetTool(WorldTool.CellDeleting);

            if (GUILayout.Button("+")) 
                SetTool(WorldTool.CellRestoring);

            EditorGUILayout.EndHorizontal();

            if (Tool == WorldTool.Painting)
                InspectorPainting();
        }

        private void InspectorPainting()
        {
            foreach (var atlasLayer in _atlasLayerFoldouts.Keys.ToArray())
            {
                _atlasLayerFoldouts[atlasLayer] = EditorGUILayout.BeginFoldoutHeaderGroup(_atlasLayerFoldouts[atlasLayer], atlasLayer.name);

                if (_atlasLayerFoldouts[atlasLayer])
                {
                    const int elementsPerRow = 5;
                    const float widthPerElement = 64f;
                    const float heightPerElement = 64f;

                    var maxGridWidth = atlasLayer.Textures.Length > elementsPerRow ? widthPerElement * elementsPerRow : widthPerElement * atlasLayer.Textures.Length;
                    var maxGridHeight = Mathf.CeilToInt((float)atlasLayer.Textures.Length / elementsPerRow) * heightPerElement;


                    var index = _selectedAtlasLayer ? _selectedGridElement : -1;
                    _selectedGridElement = GUILayout.SelectionGrid(index, atlasLayer.Textures, elementsPerRow, GUILayout.MaxWidth(maxGridWidth), GUILayout.MaxHeight(maxGridHeight));

                    if (_selectedGridElement >= 0)
                    {
                        _selectedAtlasLayer = atlasLayer;
                        _selectedAtlasLayerTexture = atlasLayer.Textures[_selectedGridElement];
                    }
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            _selectedCellTextureIndex = 0;
            int selectedCellTextureIndex = 0;
            foreach (var (atlasLayer, _) in _atlasLayerFoldouts)
            {
                foreach (var texture in atlasLayer.Textures)
                {
                    if (texture == _selectedAtlasLayerTexture)
                        _selectedCellTextureIndex = (byte) selectedCellTextureIndex;

                    selectedCellTextureIndex++;
                }
            }
        }

        private void SetTool(WorldTool tool)
        {
            Tool = tool;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private void OnScene(SceneView sceneView)
        {
            if (Tool == WorldTool.None)
                return;

            if (!Target)
                return;

            if (!Target.WorldSettings)
                return;

            Selection.activeGameObject = Target.gameObject;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));


            UpdateMouseWorldPosition();
            
            ProcessSelectedBlocks();
            
            if (Tool != WorldTool.None && Tool != WorldTool.Height && Tool != WorldTool.Painting)
            {
                ProcessSelectionTool();
            }
            else if (Tool == WorldTool.Painting)
            {
                ProcessPaintingTool();
            }
            else if (Tool == WorldTool.Height)
            {
                ProcessHeightTool();
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
            Handles.color = SelectedCellColor;
            foreach (var selectedBlock in _selectedCells)
            {
                DrawCellFace(selectedBlock);
            };
        }

        private Vector2Int GetCellByWorldPosition(Vector3 worldPosition)
        {
            var cellPosition = worldPosition;
            
            cellPosition.x = worldPosition.x / Target.WorldSettings.BlockSize; 
            cellPosition.z = worldPosition.z / Target.WorldSettings.BlockSize;

            cellPosition += SceneView.lastActiveSceneView.camera.transform.forward * 0.001f;

            cellPosition.x = Mathf.Clamp(cellPosition.x, 0, Target.WorldSettings.WorldSize.x - 1);
            cellPosition.z = Mathf.Clamp(cellPosition.z, 0, Target.WorldSettings.WorldSize.y - 1);
            
            return ToVector2Int(cellPosition);
        }

        private IEnumerable<Vector2Int> GetCellsByWorldPositionInRadius(Vector3 worldPosition, float radius)
        {
            var hoveredCells = new HashSet<Vector2Int>();
            var processed = new HashSet<Vector2Int>();
            var toProcess = new List<Vector2Int>();
            var toAdd = new List<Vector2Int>();
            
            var hoveredCellPosition = GetCellByWorldPosition(worldPosition);
            hoveredCells.Add(hoveredCellPosition);
            processed.Add(hoveredCellPosition);
            
            var neighbours = VoxelTerrainUtils.GetNeighbours(hoveredCellPosition);
            toProcess.AddRange(neighbours);

            var processing = false;
            do
            {
                foreach (var processingCell in toProcess)
                {
                    if (!processed.Add(processingCell))
                        continue;
                    
                    if (processingCell.x > Target.WorldSettings.WorldSize.x - 1 || processingCell.x < 0)
                        continue;
                    
                    if (processingCell.y > Target.WorldSettings.WorldSize.y - 1 || processingCell.y < 0)
                        continue;
                    
                    var cellWorldPosition = GetCellWorldPosition(processingCell);
                    var distance = Vector3.Distance(new Vector3(worldPosition.x, 0f, worldPosition.z), cellWorldPosition + 0.5f * Target.WorldSettings.BlockSize * new Vector3(1f, 0f, 1f));
                    if (distance <= radius)
                    {
                        hoveredCells.Add(processingCell);
                        toAdd.AddRange(VoxelTerrainUtils.GetNeighbours(processingCell));
                    }
                }

                toProcess.Clear();
                toProcess.AddRange(toAdd);
                toAdd.Clear();
                processing = toProcess.Count > 0;
                
            } while (processing);
            
            
            return hoveredCells;
        }
        
        private IEnumerable<Vector2Int> GetVerticesByWorldPositionInRadius(Vector3 worldPosition, float radius)
        {
            var hoveredVertices = new HashSet<Vector2Int>();
            var processed = new HashSet<Vector2Int>();
            var toProcess = new List<Vector2Int>();
            var toAdd = new List<Vector2Int>();
            
            var hoveredCellPosition = GetCellByWorldPosition(worldPosition);
            toProcess.Add(hoveredCellPosition);
            
            var neighbours = VoxelTerrainUtils.GetNeighbours(hoveredCellPosition);
            toProcess.AddRange(neighbours);

            worldPosition.y = 0;
            
            var processing = false;
            do
            {
                foreach (var processingCell in toProcess)
                {
                    if (!processed.Add(processingCell))
                        continue;
                    
                    if (processingCell.x > Target.WorldSettings.WorldSize.x - 1 || processingCell.x < 0)
                        continue;
                    
                    if (processingCell.y > Target.WorldSettings.WorldSize.y - 1 || processingCell.y < 0)
                        continue;


                    var continueProcessing = false;
                    var backLeftVertex = GetVertexWorldPosition(processingCell);
                    backLeftVertex.y = 0;
                    if (Vector3.Distance(worldPosition, backLeftVertex) <= radius)
                    {
                        hoveredVertices.Add(processingCell);
                        continueProcessing = true;
                    }

                    var backRightVertex = GetVertexWorldPosition(processingCell + Vector2Int.right);
                    backRightVertex.y = 0;
                    if (Vector3.Distance(worldPosition, backRightVertex) <= radius)
                    {
                        hoveredVertices.Add(processingCell + Vector2Int.right);
                        continueProcessing = true;
                    }
                    
                    var forwardLeftVertex = GetVertexWorldPosition(processingCell + Vector2Int.up);
                    forwardLeftVertex.y = 0;
                    if (Vector3.Distance(worldPosition, forwardLeftVertex) <= radius)
                    {
                        hoveredVertices.Add(processingCell + Vector2Int.up);
                        continueProcessing = true;
                    }
                    
                    var forwardRightVertex = GetVertexWorldPosition(processingCell + Vector2Int.one);
                    forwardRightVertex.y = 0;
                    if (Vector3.Distance(worldPosition, forwardRightVertex) <= radius)
                    {
                        hoveredVertices.Add(processingCell + Vector2Int.one);
                        continueProcessing = true;
                    }

                    if (continueProcessing) 
                        toAdd.AddRange(VoxelTerrainUtils.GetNeighbours(processingCell));
                }

                toProcess.Clear();
                toProcess.AddRange(toAdd);
                toAdd.Clear();
                processing = toProcess.Count > 0;
                
            } while (processing);
            
            
            return hoveredVertices;
        }

        private void ProcessSelectionTool()
        {
            Handles.color = SelectorColor;
            _hoveredCellPosition = GetCellByWorldPosition(_mouseWorldPosition);
            DrawCellFace(_hoveredCellPosition);
        }

        private void ProcessPaintingTool()
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, _paintingBrushSize, 3f);
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, 0.01f, 3f);
            DrawHoveredEdges();
        }

        private void ProcessHeightTool()
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, _heightChangingBrushSize, 3f);
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, 0.01f, 3f);
            DrawHoveredVertices();
        }

        private void DrawHoveredEdges()
        {
            _hoveredCells.Clear();
            _hoveredCells.AddRange(GetCellsByWorldPositionInRadius(_mouseWorldPosition, _paintingBrushSize));
            foreach (var hoveredCell in _hoveredCells) 
                DrawCellEdges(hoveredCell);
        }
        
        private void DrawHoveredVertices()
        {
            _hoveredVertices.Clear();
            _hoveredVertices.AddRange(GetVerticesByWorldPositionInRadius(_mouseWorldPosition, _heightChangingBrushSize));

            var normal = SceneView.lastActiveSceneView.camera.transform.forward;
            foreach (var vertex in _hoveredVertices)
            {
                Handles.DrawSolidDisc(GetVertexWorldPosition(vertex), normal, 0.05f);
            }
        }

        private void DrawCellEdges(Vector2Int cellPosition)
        {
            if (cellPosition.x > Target.WorldSettings.WorldSize.x - 1 || cellPosition.x < 0)
                return;

            if (cellPosition.y > Target.WorldSettings.WorldSize.y - 1 || cellPosition.y < 0)
                return;

            var t = Target.WorldSettings.BlockSize;
            var position = (Vector2) cellPosition;
            position *= t;

            Handles.DrawAAPolyLine(new[]
            {
                position.ToX0Z().WithY(Target.GetVertexHeight(cellPosition + Vector2Int.one)) + new Vector3(t, 0, t),
                position.ToX0Z().WithY(Target.GetVertexHeight(cellPosition + Vector2Int.up)) + new Vector3(0, 0, t),
                position.ToX0Z().WithY(Target.GetVertexHeight(cellPosition + Vector2Int.zero)) + new Vector3(0, 0, 0),
                position.ToX0Z().WithY(Target.GetVertexHeight(cellPosition + Vector2Int.right)) + new Vector3(t, 0, 0),
                position.ToX0Z().WithY(Target.GetVertexHeight(cellPosition + Vector2Int.one)) + new Vector3(t, 0, t)
            });
        }

        private void DrawCellFace(Vector2Int cellPosition)
        {
            if (cellPosition.x > Target.WorldSettings.WorldSize.x - 1 || cellPosition.x < 0)
                return;

            if (cellPosition.y > Target.WorldSettings.WorldSize.y - 1 || cellPosition.y < 0)
                return;

            var t = Target.WorldSettings.BlockSize;
            var position = (Vector2) cellPosition;
            position *= t;

            Handles.DrawAAConvexPolygon(new[]
            {
                position.ToX0Z().WithY(Target.GetVertexHeight(cellPosition + Vector2Int.one)) + new Vector3(t, 0, t),
                position.ToX0Z().WithY(Target.GetVertexHeight(cellPosition + Vector2Int.up)) + new Vector3(0, 0, t),
                position.ToX0Z().WithY(Target.GetVertexHeight(cellPosition + Vector2Int.zero)) + new Vector3(0, 0, 0),
                position.ToX0Z().WithY(Target.GetVertexHeight(cellPosition + Vector2Int.right)) + new Vector3(t, 0, 0),
                position.ToX0Z().WithY(Target.GetVertexHeight(cellPosition + Vector2Int.one)) + new Vector3(t, 0, t)
            });
        }
        
        private Vector3 GetVertexWorldPosition(Vector2Int vertex)
        {
            return new Vector3(vertex.x, Target.GetVertexHeight(vertex), vertex.y) * Target.WorldSettings.BlockSize;
        }

        private Vector3 GetCellWorldPosition(Vector2Int block)
        {
            return new Vector3(block.x, 0, block.y) * Target.WorldSettings.BlockSize;
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
                    _leftMouseButtonIsPressed = true;
                    
                    if (modeControlKey)
                    {
                        var menu = new GenericMenu();

                        foreach (var layer in Target.WorldSettings.WorldAtlas.Layers)
                        {
                            menu.AddItem(new GUIContent("Set Layer/" + layer.name), false, SetLayer, layer);
                        }
                        menu.AddItem(new GUIContent("Set Height/Up"), false, UpHeight, 0);
                        menu.AddItem(new GUIContent("Set Height/Down"), false, DownHeight, 0);
                        menu.ShowAsContext();
                    }
                    else
                    {
                        if (Tool == WorldTool.Selection)
                        {
                            if (modeShiftKey)
                            {
                                if (_selectedCells.Contains(_hoveredCellPosition))
                                {
                                    _selectedCells.Remove(_hoveredCellPosition);
                                }
                                else
                                {
                                    _selectedCells.Add(_hoveredCellPosition);
                                }
                            }
                            else
                            {
                                _selectedCells.Clear();
                                _selectedCells.Add(_hoveredCellPosition);
                            }
                        }
                        else if (Tool == WorldTool.CellDeleting)
                        {
                            Target.SetCellAvoided(Target.GetCellIndex(_hoveredCellPosition), true);
                            Target.GenerateChunkMeshes(new[] {_hoveredCellPosition});;
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                        }
                        else if (Tool == WorldTool.CellRestoring)
                        {
                            Target.SetCellAvoided(Target.GetCellIndex(_hoveredCellPosition), false);
                            Target.GenerateChunkMeshes(new[] {_hoveredCellPosition});;
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                        }
                    }
                }
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                if (Event.current.button == 0)
                {
                    _leftMouseButtonIsPressed = false;
                }
            }
            else if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                if (Tool == WorldTool.Height)
                {
                    if (Event.current.keyCode is KeyCode.RightBracket)
                    {
                        _heightChangingBrushSize += 0.2f * Target.WorldSettings.BlockSize;
                        _heightChangingBrushSize = Mathf.Min(_heightChangingBrushSize, 0.66f * Mathf.Max(Target.WorldSettings.ChunkSize.x, Target.WorldSettings.ChunkSize.y));
                    }
                    else if (Event.current.keyCode is KeyCode.LeftBracket)
                    {
                        _heightChangingBrushSize -= 0.2f * Target.WorldSettings.BlockSize;
                        _heightChangingBrushSize = Mathf.Max(_heightChangingBrushSize, 0.33f * Target.WorldSettings.BlockSize);
                    }
                }
                else if (Tool == WorldTool.Painting)
                {
                    if (Event.current.keyCode is KeyCode.RightBracket)
                    {
                        _paintingBrushSize += 0.2f * Target.WorldSettings.BlockSize;
                        _paintingBrushSize = Mathf.Min(_paintingBrushSize, 0.66f * Mathf.Max(Target.WorldSettings.ChunkSize.x, Target.WorldSettings.ChunkSize.y));
                    }
                    else if (Event.current.keyCode is KeyCode.LeftBracket)
                    {
                        _paintingBrushSize -= 0.2f * Target.WorldSettings.BlockSize;
                        _paintingBrushSize = Mathf.Max(_paintingBrushSize, 0.33f * Target.WorldSettings.BlockSize);
                    }
                }
            }
            else if (Event.current.type == EventType.KeyUp)
            {
            }

            if (_leftMouseButtonIsPressed)
            {
                if (Tool == WorldTool.Selection)
                {
                    
                }
                else if (Tool == WorldTool.Painting)
                {
                    if (!_selectedAtlasLayerTexture)
                        return;
                    
                    foreach (var hoveredCell in _hoveredCells)
                    {
                        ref var cellTexture = ref Target.GetCellTexture(hoveredCell);
                        cellTexture = _selectedCellTextureIndex;
                    }
                    Target.GenerateChunkMeshes(_hoveredCells);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
                else if (Tool == WorldTool.Height)
                {
                    var dt = 0.5f * (!modeShiftKey).ToSign() * _editorDeltaTime;
                    foreach (var vertexIndex in _hoveredVertices)
                    {
                        ref var vertexHeight = ref Target.GetVertexHeight(vertexIndex);
                        vertexHeight = Mathf.Clamp(vertexHeight + dt, Target.WorldSettings.HeightLimits.x, Target.WorldSettings.HeightLimits.y);
                    }
                    Target.GenerateChunkMeshes(_hoveredVertices);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
        }

        private static void ForceRedrawSceneView()
        {
            SceneView.RepaintAll();
        }

        private void SetLayer(object userData)
        {
            
        }

        private void UpHeight(object userData)
        {
        }

        private void DownHeight(object userData)
        {
        }

        private static Vector2Int ToVector2Int(Vector3 vector3)
        {
            return new Vector2Int((int) vector3.x, (int) vector3.z);
        }
        
        private void SetEditorDeltaTime()
        {
            if (_lastTimeSinceStartup == 0f) 
                _lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            _editorDeltaTime = (float) EditorApplication.timeSinceStartup - _lastTimeSinceStartup;
            _lastTimeSinceStartup = (float) EditorApplication.timeSinceStartup;
        }
    }
}