using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AffenCode.VoxelTerrain
{
    [CreateAssetMenu(fileName = "Atlas Layer", menuName = "Voxel Terrain/Atlas Layer", order = 1)]
    public class AtlasLayer : ScriptableObject
    {
        public Texture2D[] Textures;

#if UNITY_EDITOR
        public void Reset()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }
            
            var folderPath = Path.GetDirectoryName(assetPath);
            var textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
            
            Textures = new Texture2D[textureGuids.Length];

            for (var i = 0; i < textureGuids.Length; i++)
            {
                var textureGuid = textureGuids[i];
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textureGuid));
                Textures[i] = texture;
            }
        }
#endif
    }
}