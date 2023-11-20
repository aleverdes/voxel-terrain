#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using AleVerDes.UnityUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AleVerDes.Voxels
{
    [CustomEditor(typeof(VoxelTerrain))]
    public class VoxelTerrainEditor : Editor
    {
        private VoxelTerrain VoxelTerrain => target as VoxelTerrain;

        private readonly List<Vector3Int> _hoveredBlocks = new();

        private static readonly Color HandleColor = new Color(1, 0, 0, 0.66f);

        private Vector3 _mouseWorldPosition;

        private bool _leftMouseButtonIsPressed;
        private readonly HashSet<KeyCode> _pressedKeys = new();

        private float _editorDeltaTime;
        private float _lastTimeSinceStartup;

        private SerializedProperty _voxelTerrainSettingsProperty;
        private SerializedProperty _verticesNoiseProperty;
        private SerializedProperty _selectedEditorTool;
        private SerializedProperty _paintingBrushRadiusProperty;
        private SerializedProperty _noiseWeightBrushRadiusProperty;
        private SerializedProperty _noiseWeightBrushStrengthProperty;
        private SerializedProperty _selectedPaintingVoxelProperty;

        private int _currentSettingHeight;
        private int _selectedPaintingVoxelGridElement;

        private bool _initialized;

        private VoxelTerrainEditorTool Tool => VoxelTerrain ? VoxelTerrain.SelectedEditorTool : VoxelTerrainEditorTool.None;
        private Voxel SelectedPaintingVoxel => VoxelTerrain ? VoxelTerrain.SelectedPaintingVoxel : null;

        private void Initialize()
        {
            if (_initialized)
                return;
            
            if (!VoxelTerrain.Settings)
                return;

            _voxelTerrainSettingsProperty ??= serializedObject.FindProperty("_settings");
            _verticesNoiseProperty ??= serializedObject.FindProperty("_verticesNoise");
            _selectedEditorTool ??= serializedObject.FindProperty("SelectedEditorTool");
            _paintingBrushRadiusProperty ??= serializedObject.FindProperty("PaintingBrushRadius");
            _noiseWeightBrushRadiusProperty ??= serializedObject.FindProperty("NoiseWeightBrushRadius");
            _noiseWeightBrushStrengthProperty ??= serializedObject.FindProperty("NoiseWeightBrushStrength");
            _selectedPaintingVoxelProperty ??= serializedObject.FindProperty("SelectedPaintingVoxel");

            _initialized = true;
        }
        
        public void OnEnable()
        {
            EditorApplication.update += ForceRedrawSceneView;
            EditorApplication.update += SetEditorDeltaTime;
            SceneView.duringSceneGui += OnScene;
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
            
            EditorGUILayout.PropertyField(_voxelTerrainSettingsProperty);
            EditorGUILayout.PropertyField(_verticesNoiseProperty);
            
            EditorGUILayout.Separator();
            
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(" ")) 
                SetTool(VoxelTerrainEditorTool.None);

            if (GUILayout.Button("S")) 
                SetTool(VoxelTerrainEditorTool.SetBlock);

            if (GUILayout.Button("P")) 
                SetTool(VoxelTerrainEditorTool.Painting);

            if (GUILayout.Button("N")) 
                SetTool(VoxelTerrainEditorTool.NoiseWeight);

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Separator();

            if (Tool is VoxelTerrainEditorTool.Painting or VoxelTerrainEditorTool.SetBlock)
                DrawVoxelsDatabase();

            if (Tool is VoxelTerrainEditorTool.NoiseWeight)
            {
                EditorGUI.BeginChangeCheck();
                _noiseWeightBrushStrengthProperty.floatValue = EditorGUILayout.Slider("Noise Strength", _noiseWeightBrushStrengthProperty.floatValue, 0f, 255f);
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawVoxelsDatabase()
        {
            if (!VoxelTerrain)
            {
                EditorGUILayout.HelpBox("Voxel terrain not set", MessageType.Error);
                return;
            }
            
            if (!VoxelTerrain.Settings)
            {
                EditorGUILayout.HelpBox("Voxel terrain settings not set", MessageType.Error);
                return;
            }
            
            if (!VoxelTerrain.Settings.TextureAtlas)
            {
                EditorGUILayout.HelpBox("Voxel terrain atlas not set", MessageType.Error);
                return;
            }
            
            if (!VoxelTerrain.Settings.TextureAtlas.VoxelDatabase)
            {
                EditorGUILayout.HelpBox("Voxel terrain voxel database not set", MessageType.Error);
                return;
            }

            var voxels = VoxelTerrain.Settings.TextureAtlas.VoxelDatabase.Voxels.ToArray();const int elementsPerRow = 5;
            var voxelsPreviews = voxels.Select(x => x.Variants.First().Top).ToArray();
            
            const float widthPerElement = 64f;
            const float heightPerElement = 64f;
            var maxGridWidth = voxels.Length > elementsPerRow ? widthPerElement * elementsPerRow : widthPerElement * voxels.Length;
            var maxGridHeight = Mathf.CeilToInt((float)voxels.Length / elementsPerRow) * heightPerElement;
            var layoutParams = new[] { GUILayout.MaxWidth(maxGridWidth), GUILayout.MaxHeight(maxGridHeight) };

            EditorGUI.BeginChangeCheck();
            _selectedPaintingVoxelGridElement = GUILayout.SelectionGrid(_selectedPaintingVoxelGridElement, voxelsPreviews, elementsPerRow, layoutParams);
            _selectedPaintingVoxelProperty.objectReferenceValue = _selectedPaintingVoxelGridElement >= 0 ? voxels[_selectedPaintingVoxelGridElement] : null;
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        private void SetTool(VoxelTerrainEditorTool tool)
        {
            _selectedEditorTool.enumValueIndex = (int) tool;
            serializedObject.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private void OnScene(SceneView sceneView)
        {
            Initialize();
            
            if (Tool == VoxelTerrainEditorTool.None)
                return;

            if (!VoxelTerrain)
                return;

            if (!VoxelTerrain.Settings)
                return;

            Selection.activeGameObject = VoxelTerrain.gameObject;
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            UpdateMouseWorldPosition();
            
            switch (Tool)
            {
                case VoxelTerrainEditorTool.SetBlock:
                    ProcessSetBlockTool();
                    break;
                case VoxelTerrainEditorTool.Painting:
                    ProcessPaintingTool();
                    break;
                case VoxelTerrainEditorTool.NoiseWeight:
                    ProcessNoiseWeightTool();
                    break;
                case VoxelTerrainEditorTool.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DrawHoveredBlocks();
            ProcessEvents();
        }

        private void DrawHoveredBlocks()
        {
            foreach (var hoveredBlock in _hoveredBlocks)
            {
                var blockWorldPosition = GetBlockWorldPosition(hoveredBlock);
                var blockBounds = new Bounds(blockWorldPosition + 0.5f * VoxelTerrain.Settings.BlockSize, VoxelTerrain.Settings.BlockSize);
                Handles.DrawWireCube(blockBounds.center, blockBounds.size);
            }
        }

        private void UpdateMouseWorldPosition()
        {
            Vector3 mousePosition = Event.current.mousePosition;
            var worldRay = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(worldRay, out var hit, 500f)) 
                _mouseWorldPosition = hit.point;
        }

        private Vector3Int GetBlockByWorldPosition(Vector3 worldPosition)
        {
            var blockPosition = worldPosition;
            
            blockPosition.x = worldPosition.x / VoxelTerrain.Settings.BlockSize.x;
            blockPosition.y = worldPosition.y / VoxelTerrain.Settings.BlockSize.y;
            blockPosition.z = worldPosition.z / VoxelTerrain.Settings.BlockSize.z;

            blockPosition += SceneView.lastActiveSceneView.camera.transform.forward * 0.001f;

            return ToVector3Int(blockPosition);
        }

        private IEnumerable<Vector3Int> GetBlockByWorldPositionInRadius(Vector3 worldPosition, float radius)
        {
            var hoveredBlocks = new HashSet<Vector3Int>();
            var processed = new HashSet<Vector3Int>();
            var toProcess = new List<Vector3Int>();
            var toAdd = new List<Vector3Int>();
            
            var hoveredBlockPosition = GetBlockByWorldPosition(worldPosition);
            if (!VoxelTerrain.IsBlockExistsInChunks(hoveredBlockPosition) || VoxelTerrain.GetBlockVoxelIndex(hoveredBlockPosition) == 0)
            {
                hoveredBlockPosition -= Vector3Int.up;
                if (!VoxelTerrain.IsBlockExistsInChunks(hoveredBlockPosition) || VoxelTerrain.GetBlockVoxelIndex(hoveredBlockPosition) == 0)
                {
                    hoveredBlockPosition += Vector3Int.up;
                    if (!VoxelTerrain.IsBlockExistsInChunks(hoveredBlockPosition) || VoxelTerrain.GetBlockVoxelIndex(hoveredBlockPosition) == 0)
                    {
                        hoveredBlockPosition += Vector3Int.up;
                    }
                }
            }
            if (!VoxelTerrain.IsBlockExistsInChunks(hoveredBlockPosition))
                return hoveredBlocks;
            hoveredBlocks.Add(hoveredBlockPosition);
            processed.Add(hoveredBlockPosition);
            
            var neighbours = Utils.GetHorizontalNeighbours(hoveredBlockPosition);
            toProcess.AddRange(neighbours);

            var processing = false;
            do
            {
                foreach (var processingBlock in toProcess)
                {
                    if (!processed.Add(processingBlock))
                        continue;
                    
                    if (!VoxelTerrain.IsBlockExistsInChunks(processingBlock))
                        continue;
                    
                    var blockWorldPosition = GetBlockWorldPosition(processingBlock);
                    var distance = Vector3.Distance(worldPosition, blockWorldPosition + 0.5f * VoxelTerrain.Settings.BlockSize);
                    if (distance <= radius)
                    {
                        hoveredBlocks.Add(processingBlock);
                        toAdd.AddRange(Utils.GetHorizontalNeighbours(processingBlock));
                    }
                }

                toProcess.Clear();
                toProcess.AddRange(toAdd);
                toAdd.Clear();
                processing = toProcess.Count > 0;
                
            } while (processing);
            
            
            return hoveredBlocks;
        }
        
        private void ProcessSetBlockTool()
        {
            Handles.color = HandleColor;
        }

        private void ProcessPaintingTool()
        {
            Handles.color = HandleColor;
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, _paintingBrushRadiusProperty.floatValue, 3f);
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, 0.01f, 3f);
            _hoveredBlocks.Clear();
            _hoveredBlocks.AddRange(GetBlockByWorldPositionInRadius(_mouseWorldPosition, _paintingBrushRadiusProperty.floatValue));
        }

        private void ProcessNoiseWeightTool()
        {
            Handles.color = HandleColor;
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, _noiseWeightBrushRadiusProperty.floatValue, 3f);
            Handles.DrawWireDisc(_mouseWorldPosition, Vector3.up, 0.01f, 3f);
            _hoveredBlocks.Clear();
            _hoveredBlocks.AddRange(GetBlockByWorldPositionInRadius(_mouseWorldPosition, _noiseWeightBrushRadiusProperty.floatValue));
        }
        
        private Vector3 GetBlockWorldPosition(Vector3Int block)
        {
            var blockSize = VoxelTerrain.Settings.BlockSize;
            return new Vector3(block.x * blockSize.x, block.y * blockSize.y, block.z * blockSize.z);
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
                    _leftMouseButtonIsPressed = true;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                if (Event.current.button == 0) 
                    _leftMouseButtonIsPressed = false;
            }
            else if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                _pressedKeys.Add(Event.current.keyCode);
            }
            else if (Event.current.type == EventType.KeyUp)
            {
                _pressedKeys.Remove(Event.current.keyCode);
            }

            if (_leftMouseButtonIsPressed)
            {
                if (Tool == VoxelTerrainEditorTool.SetBlock)
                {
                    
                }
                
                if (Tool == VoxelTerrainEditorTool.Painting)
                {
                    var chunks = new HashSet<int>();
                    var chunksUpdateRequired = false;
                    
                    foreach (var hoveredBlock in _hoveredBlocks)
                    {
                        ref var blockVoxelIndex = ref VoxelTerrain.GetBlockVoxelIndex(hoveredBlock);
                        var newBlockVoxelIndex = (byte)(VoxelTerrain.Settings.TextureAtlas.VoxelDatabase.IndexOf(SelectedPaintingVoxel) + 1);
                        if (blockVoxelIndex > 0 && blockVoxelIndex != newBlockVoxelIndex)
                        {
                            blockVoxelIndex = newBlockVoxelIndex;
                            chunksUpdateRequired = true;
                        }
                        chunks.Add(VoxelTerrain.GetChunkIndex(hoveredBlock));
                    }

                    if (chunksUpdateRequired)
                    {
                        VoxelTerrain.UpdateChunks(chunks);
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    }
                }
                
                if (Tool == VoxelTerrainEditorTool.NoiseWeight)
                {
                    var dt = _noiseWeightBrushStrengthProperty.floatValue * (!modeShiftKey).ToSign() * _editorDeltaTime;
                    if (Mathf.Abs(dt) < 1)
                        dt = Mathf.Sign(dt);
                    
                    var chunks = new HashSet<int>();
                    var chunksUpdateRequired = false;
                    
                    foreach (var hoveredBlock in _hoveredBlocks)
                    {
                        ref var blockNoiseWeight = ref VoxelTerrain.GetBlockNoiseWeight(hoveredBlock);
                        var newBlockNoiseWeight = (byte) Mathf.Clamp(blockNoiseWeight + dt, byte.MinValue, byte.MaxValue);
                        if (blockNoiseWeight != newBlockNoiseWeight)
                        {
                            blockNoiseWeight = newBlockNoiseWeight;
                            chunksUpdateRequired = true;
                        }
                        chunks.Add(VoxelTerrain.GetChunkIndex(hoveredBlock));
                    }
                    
                    if (chunksUpdateRequired)
                    {
                        VoxelTerrain.UpdateChunks(chunks);
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                    }
                }
            }
            
            
            if (Tool == VoxelTerrainEditorTool.NoiseWeight)
            {
                if (_pressedKeys.Contains(KeyCode.UpArrow))
                {
                    _noiseWeightBrushStrengthProperty.floatValue
                        = Mathf.Clamp(_noiseWeightBrushStrengthProperty.floatValue + 20f * _editorDeltaTime, 0, 255f);
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                if (_pressedKeys.Contains(KeyCode.DownArrow))
                {
                    _noiseWeightBrushStrengthProperty.floatValue
                        = Mathf.Clamp(_noiseWeightBrushStrengthProperty.floatValue - 20f * _editorDeltaTime, 0, 255f);
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }

                if (_pressedKeys.Contains(KeyCode.RightBracket))
                {
                    var chunkSize = Mathf.Max(VoxelTerrain.Settings.ChunkSize.x, VoxelTerrain.Settings.ChunkSize.z);
                    _noiseWeightBrushRadiusProperty.floatValue
                        = Mathf.Min(_noiseWeightBrushRadiusProperty.floatValue + 2f * _editorDeltaTime, 0.66f * chunkSize);
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
                
                if (_pressedKeys.Contains(KeyCode.LeftBracket))
                {
                    var blockSize = Mathf.Max(VoxelTerrain.Settings.BlockSize.x, VoxelTerrain.Settings.BlockSize.y, VoxelTerrain.Settings.BlockSize.z);
                    _noiseWeightBrushRadiusProperty.floatValue
                        = Mathf.Max(_noiseWeightBrushRadiusProperty.floatValue - 2f * _editorDeltaTime, 0.33f * blockSize);
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            
            if (Tool == VoxelTerrainEditorTool.Painting)
            {
                if (_pressedKeys.Contains(KeyCode.RightBracket))
                {
                    var chunkSize = Mathf.Max(VoxelTerrain.Settings.ChunkSize.x, VoxelTerrain.Settings.ChunkSize.z);
                    _paintingBrushRadiusProperty.floatValue
                        = Mathf.Min(_paintingBrushRadiusProperty.floatValue + 2f * _editorDeltaTime, 0.66f * chunkSize);
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
                
                if (_pressedKeys.Contains(KeyCode.LeftBracket))
                {
                    var blockSize = Mathf.Max(VoxelTerrain.Settings.BlockSize.x, VoxelTerrain.Settings.BlockSize.y, VoxelTerrain.Settings.BlockSize.z);
                    _paintingBrushRadiusProperty.floatValue
                        = Mathf.Max(_paintingBrushRadiusProperty.floatValue - 2f * _editorDeltaTime, 0.33f * blockSize);
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private static void ForceRedrawSceneView()
        {
            SceneView.RepaintAll();
        }
        
        private void SetEditorDeltaTime()
        {
            if (_lastTimeSinceStartup == 0f) 
                _lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            _editorDeltaTime = (float) EditorApplication.timeSinceStartup - _lastTimeSinceStartup;
            _lastTimeSinceStartup = (float) EditorApplication.timeSinceStartup;
        }

        private static Vector3Int ToVector3Int(Vector3 vector3)
        {
            return new Vector3Int((int) vector3.x,(int) vector3.y, (int) vector3.z);
        }
    }
}
#endif