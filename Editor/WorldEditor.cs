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

        private readonly List<Vector3Int> _selectedBlocks = new();
        private readonly Dictionary<Vector3Int, TopVerticesExisting> _blockTopVerticesExisting = new();

        private static readonly Color SelectorColor = new Color(1, 0, 0, 0.66f);
        private static readonly Color SelectedBlockColor = new Color(1, 0, 0, 0.33f);

        private Vector3 _mouseWorldPosition;
        private Vector3Int _hoveredBlockPosition;

        private VisualElement _rootVisualElement;
        private VisualTreeAsset _visualTreeAsset;
        private Dictionary<WorldTool, Button> _toolbarButton = new();
        
        private float _heightChangingBrushSize = 0.5f;

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
                { WorldTool.AddBlock, _rootVisualElement.Q<Button>("toolbar-button--add-block") },
                { WorldTool.RemoveBlock, _rootVisualElement.Q<Button>("toolbar-button--remove-block") },
                { WorldTool.SelectBlock, _rootVisualElement.Q<Button>("toolbar-button--select-block") },
                { WorldTool.SelectFace, _rootVisualElement.Q<Button>("toolbar-button--select-face") },
                { WorldTool.PaintBlock, _rootVisualElement.Q<Button>("toolbar-button--paint-block") },
                { WorldTool.PaintFace, _rootVisualElement.Q<Button>("toolbar-button--paint-face") },
                { WorldTool.VertexHeight, _rootVisualElement.Q<Button>("toolbar-button--vertex-height") },
            };
            
            _toolbarButton[WorldTool.AddBlock].RegisterCallback<ClickEvent>(SetAddBlockTool);
            _toolbarButton[WorldTool.RemoveBlock].RegisterCallback<ClickEvent>(SetRemoveBlockTool);
            _toolbarButton[WorldTool.SelectBlock].RegisterCallback<ClickEvent>(SetSelectBlockTool);
            _toolbarButton[WorldTool.SelectFace].RegisterCallback<ClickEvent>(SetSelectFaceTool);
            _toolbarButton[WorldTool.PaintBlock].RegisterCallback<ClickEvent>(SetPaintBlockTool);
            _toolbarButton[WorldTool.PaintFace].RegisterCallback<ClickEvent>(SetPaintFaceTool);
            _toolbarButton[WorldTool.VertexHeight].RegisterCallback<ClickEvent>(SetVertexHeightTool);
            
            UpdateToolbar();
        }

        private void SetAddBlockTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.AddBlock);
        }

        private void SetRemoveBlockTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.RemoveBlock);
        }

        private void SetSelectBlockTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.SelectBlock);
        }

        private void SetSelectFaceTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.SelectFace);
        }

        private void SetPaintBlockTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.PaintBlock);
        }

        private void SetPaintFaceTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.PaintFace);
        }

        private void SetVertexHeightTool(ClickEvent clickEvent)
        {
            SetTool(WorldTool.VertexHeight);
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
            
            if (Tool != WorldTool.None && Tool != WorldTool.VertexHeight)
            {
                ProcessBlockSelection();
            }
            else if (Tool == WorldTool.VertexHeight)
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
            
            _hoveredBlockPosition = GetHoveredBlockPosition(_mouseWorldPosition);

            DrawBlockSelection(_hoveredBlockPosition);
        }

        private Vector3Int GetHoveredBlockPosition(Vector3 mouseWorldPosition)
        {
            var cellPosition = mouseWorldPosition;
            
            cellPosition.x = mouseWorldPosition.x / Target.WorldSettings.BlockSize; 
            cellPosition.y = mouseWorldPosition.y / Target.WorldSettings.BlockSize; 
            cellPosition.z = mouseWorldPosition.z / Target.WorldSettings.BlockSize;

            cellPosition += SceneView.lastActiveSceneView.camera.transform.forward * 0.001f;

            cellPosition.x = Mathf.Clamp(cellPosition.x, 0, Target.WorldSettings.WorldSize.x - 1);
            cellPosition.y = Mathf.Clamp(cellPosition.y, 0, Target.WorldSettings.WorldSize.y - 1);
            cellPosition.z = Mathf.Clamp(cellPosition.z, 0, Target.WorldSettings.WorldSize.z - 1);
            
            return ToVector3Int(cellPosition);
        }

        private void ProcessVertexHeight()
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, _heightChangingBrushSize, 3f);
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, 0.01f, 3f);
            DrawHoveredVertices();
        }
        
        private void DrawHoveredVertices()
        {
            _blockTopVerticesExisting.Clear();

            var hoveredVertices = new HashSet<Vector3>();
            var blocksToProcessing = new HashSet<Vector3Int>();
            var processedBlocks = new HashSet<Vector3Int>();
            
            var hoveredBlock = GetHoveredBlockPosition(_mouseWorldPosition);
            blocksToProcessing.Add(hoveredBlock);

            bool anyVertexProcessed;
            do
            {
                anyVertexProcessed = false;
                var toAdd = new List<Vector3Int>();
                foreach (var blockToProcessing in blocksToProcessing)
                {
                    CheckBlock(blockToProcessing);
                    var neighbours = VoxelTerrainUtils.GetNeighbours(blockToProcessing);
                    foreach (var neighbour in neighbours)
                    {
                        CheckBlock(neighbour);
                    }

                    void CheckBlock(Vector3Int blockToCheck)
                    {
                        if (blockToCheck.x < 0 || blockToCheck.y < 0 || blockToCheck.z < 0)
                        {
                            processedBlocks.Add(blockToCheck);
                            return;
                        }

                        if (blockToCheck.x > Target.WorldSettings.WorldSize.x - 1 || blockToCheck.y > Target.WorldSettings.WorldSize.y - 1 || blockToCheck.z > Target.WorldSettings.WorldSize.z - 1)
                        {
                            processedBlocks.Add(blockToCheck);
                            return;
                        }

                        if (!processedBlocks.Add(blockToCheck))
                            return;

                        var vertices = new TopVerticesExisting();
                        
                        if (TryAddVertex(BlockVertexPosition.TopForwardRight))
                        {
                            vertices.ForwardRight = true;
                            anyVertexProcessed = true;
                        }

                        if (TryAddVertex(BlockVertexPosition.TopForwardLeft))
                        {
                            vertices.ForwardLeft = true;
                            anyVertexProcessed = true;
                        }

                        if (TryAddVertex(BlockVertexPosition.TopBackRight))
                        {
                            vertices.BackRight = true;
                            anyVertexProcessed = true;
                        }

                        if (TryAddVertex(BlockVertexPosition.TopBackLeft))
                        {
                            vertices.BackLeft = true;
                            anyVertexProcessed = true;
                        }
                        
                        _blockTopVerticesExisting.Add(blockToCheck, vertices);

                        if (anyVertexProcessed)
                            toAdd.AddRange(VoxelTerrainUtils.GetNeighbours(blockToCheck));
                    
                        bool TryAddVertex(BlockVertexPosition blockVertexPosition)
                        {
                            var vertexWorldPosition = GetVertexWorldPosition(blockToCheck, blockVertexPosition);
                            var distance = Vector3.Distance(vertexWorldPosition, _mouseWorldPosition) < _heightChangingBrushSize;
                            if (distance)
                                hoveredVertices.Add(vertexWorldPosition);
                            return distance;
                        }
                    }
                }

                foreach (var vector3Int in toAdd)
                {
                    blocksToProcessing.Add(vector3Int);
                }
            } while (blocksToProcessing.Count > 0 && anyVertexProcessed);

            var normal = SceneView.lastActiveSceneView.camera.transform.forward;
            foreach (var vertex in hoveredVertices)
            {
                Handles.DrawSolidDisc(vertex, normal, 0.05f);
            }
        }

        /// <summary>
        /// Return vertex position in the world
        /// </summary>
        /// <param name="blockPosition">Block position in the world</param>
        /// <param name="vertexPosition">Vertex position</param>
        /// <returns>Vertex world position</returns>
        private Vector3 GetVertexWorldPosition(Vector3Int blockPosition, BlockVertexPosition vertexPosition)
        {
            var block = Target.GetBlock(blockPosition + Vector3Int.up);
            var blockWorldPosition = GetBlockWorldPosition(blockPosition);
            var vertexLocalPosition = Vector3.zero;
            var blockSize = Target.WorldSettings.BlockSize;
            switch (vertexPosition)
            {
                case BlockVertexPosition.TopForwardRight:
                    vertexLocalPosition += new Vector3(1, block.TopVerticesHeights.ForwardRight, 1) * blockSize;
                    break;
                case BlockVertexPosition.TopForwardLeft:
                    vertexLocalPosition += new Vector3(0, block.TopVerticesHeights.ForwardLeft, 1) * blockSize;
                    break;
                case BlockVertexPosition.TopBackRight:
                    vertexLocalPosition += new Vector3(1, block.TopVerticesHeights.BackRight, 0) * blockSize;
                    break;
                case BlockVertexPosition.TopBackLeft:
                    vertexLocalPosition += new Vector3(0, block.TopVerticesHeights.BackLeft, 0) * blockSize;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(vertexPosition), vertexPosition, null);
            }

            return blockWorldPosition + vertexLocalPosition;
        }

        private Vector3 GetBlockWorldPosition(Vector3Int block)
        {
            return new Vector3(block.x, block.y, block.z) * Target.WorldSettings.BlockSize;
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
                        if (Tool == WorldTool.SelectBlock)
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
                        else if (Tool == WorldTool.AddBlock)
                        {
                            ref var block = ref Target.GetBlock(_hoveredBlockPosition + Vector3Int.up);
                            block.Void = false;
                            Target.GenerateChunkMeshes(new Vector3Int[] {_hoveredBlockPosition + Vector3Int.up});;
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                        }
                        else if (Tool == WorldTool.RemoveBlock)
                        {
                            ref var block = ref Target.GetBlock(_hoveredBlockPosition);
                            block.Void = true;
                            Target.GenerateChunkMeshes(new Vector3Int[] {_hoveredBlockPosition});;
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
                if (Tool == WorldTool.VertexHeight)
                {
                    if (Event.current.keyCode is KeyCode.RightBracket)
                    {
                        _heightChangingBrushSize += 0.2f * Target.WorldSettings.BlockSize;
                        _heightChangingBrushSize = Mathf.Min(_heightChangingBrushSize, 0.5f * Mathf.Max(Target.WorldSettings.ChunkSize.x, Target.WorldSettings.ChunkSize.y));
                    }
                    else if (Event.current.keyCode is KeyCode.LeftBracket)
                    {
                        _heightChangingBrushSize -= 0.2f * Target.WorldSettings.BlockSize;
                        _heightChangingBrushSize = Mathf.Max(_heightChangingBrushSize, 0.5f * Target.WorldSettings.BlockSize);
                    }
                }
            }
            else if (Event.current.type == EventType.KeyUp)
            {
            }

            if (_leftMouseButtonIsPressed)
            {
                if (Tool == WorldTool.VertexHeight)
                {
                    var dt = 0.5f * modeShiftKey.ToSign() * _editorDeltaTime;
                    foreach (var (blockPosition, verticesExisting) in _blockTopVerticesExisting)
                    {
                        ref var block = ref Target.GetBlock(blockPosition);
                        if (verticesExisting.BackLeft)
                            block.TopVerticesHeights.BackLeft = Mathf.Clamp01(block.TopVerticesHeights.BackLeft + dt);
                        if (verticesExisting.BackRight)
                            block.TopVerticesHeights.BackRight = Mathf.Clamp01(block.TopVerticesHeights.BackRight + dt);
                        if (verticesExisting.ForwardLeft)
                            block.TopVerticesHeights.ForwardLeft = Mathf.Clamp01(block.TopVerticesHeights.ForwardLeft + dt);
                        if (verticesExisting.ForwardRight)
                            block.TopVerticesHeights.ForwardRight = Mathf.Clamp01(block.TopVerticesHeights.ForwardRight + dt);
                    }
                    Target.GenerateChunkMeshes(_blockTopVerticesExisting.Keys);
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
        }

        private void DrawBlockSelection(Vector3Int blockPosition)
        {
            if (blockPosition.x > Target.WorldSettings.WorldSize.x - 1 || blockPosition.x < 0)
            {
                return;
            }

            if (blockPosition.y > Target.WorldSettings.WorldSize.y - 1 || blockPosition.y < 0)
            {
                return;
            }

            if (blockPosition.z > Target.WorldSettings.WorldSize.z - 1 || blockPosition.z < 0)
            {
                return;
            }

            ref var block = ref Target.GetBlock(blockPosition);

            if (block.Void)
            {
                return;
            }

            var t = Target.WorldSettings.BlockSize;
            var position = (Vector3) blockPosition;
            position *= t;

            var face = default(BlockFace);
            
            face = block.Top;
            if (face.Draw 
                && (block.Position.y < Target.WorldSettings.WorldSize.y - 1 && Target.GetBlock(blockPosition.x, blockPosition.y + 1, blockPosition.z).Void
                    || block.Position.y == Target.WorldSettings.WorldSize.y - 1))
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
                && (block.Position.z < Target.WorldSettings.WorldSize.z - 1 && Target.GetBlock(blockPosition.x, blockPosition.y, blockPosition.z + 1).Void
                    || block.Position.z == Target.WorldSettings.WorldSize.z - 1))
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
                && (block.Position.x < Target.WorldSettings.WorldSize.x - 1 && Target.GetBlock(blockPosition.x + 1, blockPosition.y, blockPosition.z).Void
                    || block.Position.x == Target.WorldSettings.WorldSize.x - 1))
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

        private void SetLayer(object userData)
        {
            
        }

        private void UpHeight(object userData)
        {
        }

        private void DownHeight(object userData)
        {
        }

        private Vector3Int ToVector3Int(Vector3 vector3)
        {
            return new Vector3Int((int) vector3.x, (int) vector3.y, (int) vector3.z);
        }
        
        private void SetEditorDeltaTime()
        {
            if (_lastTimeSinceStartup == 0f) 
                _lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            _editorDeltaTime = (float) EditorApplication.timeSinceStartup - _lastTimeSinceStartup;
            _lastTimeSinceStartup = (float) EditorApplication.timeSinceStartup;
        }

        private struct TopVerticesExisting
        {
            public bool ForwardRight;
            public bool ForwardLeft;
            public bool BackRight;
            public bool BackLeft;
        }
    }
}