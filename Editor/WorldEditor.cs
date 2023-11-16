using System;
using System.Collections.Generic;
using System.IO;
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
        
        private WorldTool Tool
        {
            get => Target.LastWorldTool;
            set => Target.LastWorldTool = value;
        }
        
        public void OnEnable()
        {
            EditorApplication.update += ForceRedrawSceneView;
            EditorApplication.update += SetEditorDeltaTime;
            SceneView.duringSceneGui += OnScene;

            _rootVisualElement = new VisualElement();
            
            var monoScript = MonoScript.FromScriptableObject(this);
            var scriptPath = AssetDatabase.GetAssetPath(monoScript);
            var folderPath = Path.GetDirectoryName(scriptPath);
            
            var templatePath = Path.Combine(folderPath, "Visual/VoxelTerrainEditorTemplate.uxml");
            _visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(templatePath);

            var stylesPath = Path.Combine(folderPath, "Visual/VoxelTerrainEditorStyles.uss");
            var styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylesPath);
            _rootVisualElement.styleSheets.Add(styles);
            
            _heightChangingBrushSize = Target.WorldSettings.BlockSize;
        }

        public void OnDisable()
        {
            EditorApplication.update -= ForceRedrawSceneView;
            EditorApplication.update -= SetEditorDeltaTime;
            SceneView.duringSceneGui -= OnScene;
        }

        public override VisualElement CreateInspectorGUI()
        {
            _rootVisualElement.Clear();

            _visualTreeAsset.CloneTree(_rootVisualElement);

            SetupToolbar();
            
            return _rootVisualElement;
        }

        private void SetupToolbar()
        {
            _toolbarButton = new Dictionary<WorldTool, Button>()
            { 
                { WorldTool.Selection, _rootVisualElement.Q<Button>("toolbar-button--selection") },
                { WorldTool.Painting, _rootVisualElement.Q<Button>("toolbar-button--painting") },
                { WorldTool.Height, _rootVisualElement.Q<Button>("toolbar-button--height") },
                { WorldTool.CellRestoring, _rootVisualElement.Q<Button>("toolbar-button--restoring") },
                { WorldTool.CellDeleting, _rootVisualElement.Q<Button>("toolbar-button--deleting") },
            };
            
            _toolbarButton[WorldTool.Selection].RegisterCallback<ClickEvent>(SetSelectionTool);
            _toolbarButton[WorldTool.Painting].RegisterCallback<ClickEvent>(SetPaintingTool);
            _toolbarButton[WorldTool.Height].RegisterCallback<ClickEvent>(SetHeightTool);
            _toolbarButton[WorldTool.CellRestoring].RegisterCallback<ClickEvent>(SetCellRestoringTool);
            _toolbarButton[WorldTool.CellDeleting].RegisterCallback<ClickEvent>(SetCellDeletingTool);
            
            UpdateToolbar();
        }

        private void SetCellRestoringTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.CellRestoring);
        }

        private void SetCellDeletingTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.CellDeleting);
        }

        private void SetHeightTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.Height);
        }

        private void SetSelectionTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.Selection);
        }

        private void SetPaintingTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.Painting);
        }

        private void SetTool(WorldTool tool)
        {
            Tool = tool;
            UpdateInspector();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private void UpdateInspector()
        {
            UpdateToolbar();
        }

        private void UpdateToolbar()
        {
            foreach (var (worldTool, toolbarButton) in _toolbarButton)
            {
                if (worldTool == Tool)
                {
                    toolbarButton.AddToClassList("pressed");
                }
                else
                {
                    toolbarButton.RemoveFromClassList("pressed");
                }
            }
        }
        
        private void OnScene(SceneView sceneView)
        {
            if (Tool == WorldTool.None)
            {
                return;
            }
            
            if (!Target)
            {
                return;
            }
            
            if (!Target.WorldSettings)
            {
                return;
            }
            
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
                    if (Vector3.Distance(new Vector3(worldPosition.x, 0f, worldPosition.z), backLeftVertex) <= radius)
                    {
                        hoveredVertices.Add(processingCell);
                        continueProcessing = true;
                    }

                    var backRightVertex = GetVertexWorldPosition(processingCell + Vector2Int.right);
                    if (Vector3.Distance(new Vector3(worldPosition.x, 0f, worldPosition.z), backRightVertex) <= radius)
                    {
                        hoveredVertices.Add(processingCell + Vector2Int.right);
                        continueProcessing = true;
                    }
                    
                    var forwardLeftVertex = GetVertexWorldPosition(processingCell + Vector2Int.up);
                    if (Vector3.Distance(new Vector3(worldPosition.x, 0f, worldPosition.z), forwardLeftVertex) <= radius)
                    {
                        hoveredVertices.Add(processingCell + Vector2Int.up);
                        continueProcessing = true;
                    }
                    
                    var forwardRightVertex = GetVertexWorldPosition(processingCell + Vector2Int.one);
                    if (Vector3.Distance(new Vector3(worldPosition.x, 0f, worldPosition.z), forwardRightVertex) <= radius)
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
            var hoveredCells = GetCellsByWorldPositionInRadius(_mouseWorldPosition, _paintingBrushSize);
            foreach (var hoveredCell in hoveredCells) 
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
                position.ToX0Z() + new Vector3(t, 0, t),
                position.ToX0Z() + new Vector3(0, 0, t),
                position.ToX0Z() + new Vector3(0, 0, 0),
                position.ToX0Z() + new Vector3(t, 0, 0),
                position.ToX0Z() + new Vector3(t, 0, t)
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
                position.ToX0Z() + new Vector3(t, 0, t),
                position.ToX0Z() + new Vector3(0, 0, t),
                position.ToX0Z() + new Vector3(0, 0, 0),
                position.ToX0Z() + new Vector3(t, 0, 0),
                position.ToX0Z() + new Vector3(t, 0, t)
            });
        }

        private Vector3 GetVertexWorldPosition(Vector2Int vertex)
        {
            return (new Vector2(vertex.x, vertex.y) * Target.WorldSettings.BlockSize).ToX0Z();
        }

        private Vector3 GetCellWorldPosition(Vector2Int block)
        {
            return (new Vector2(block.x, block.y) * Target.WorldSettings.BlockSize).ToX0Z();
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
                
                
                // if (Tool == WorldTool.VertexHeight)
                // {
                //     var dt = 0.5f * modeShiftKey.ToSign() * _editorDeltaTime;
                //     foreach (var (blockPosition, verticesExisting) in _blockTopVerticesExisting)
                //     {
                //         ref var block = ref Target.GetBlock(blockPosition);
                //         if (verticesExisting.BackLeft)
                //             block.TopVerticesHeights.BackLeft = Mathf.Clamp01(block.TopVerticesHeights.BackLeft + dt);
                //         if (verticesExisting.BackRight)
                //             block.TopVerticesHeights.BackRight = Mathf.Clamp01(block.TopVerticesHeights.BackRight + dt);
                //         if (verticesExisting.ForwardLeft)
                //             block.TopVerticesHeights.ForwardLeft = Mathf.Clamp01(block.TopVerticesHeights.ForwardLeft + dt);
                //         if (verticesExisting.ForwardRight)
                //             block.TopVerticesHeights.ForwardRight = Mathf.Clamp01(block.TopVerticesHeights.ForwardRight + dt);
                //     }
                //     Target.GenerateChunkMeshes(_blockTopVerticesExisting.Keys);
                //     EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                // }
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