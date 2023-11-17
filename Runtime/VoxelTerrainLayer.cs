using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AleVerDes.VoxelTerrain
{
    [CreateAssetMenu(fileName = "Terrain Layer", menuName = "Voxel Terrain/Terrain Layer", order = 1)]
    public class VoxelTerrainLayer : ScriptableObject
    {
        public Texture2D[] Textures;
        public AudioClip[] StepSounds;

#if UNITY_EDITOR
        public void Reset()
        {
            var assetPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(assetPath))
                return;
            FindTextures(assetPath);
            FindAudioClips(assetPath);
        }

        private void FindTextures(string assetPath)
        {
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
        
        private void FindAudioClips(string assetPath)
        {
            var folderPath = Path.GetDirectoryName(assetPath);
            var audioClipGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });
            
            StepSounds = new AudioClip[audioClipGuids.Length];

            for (var i = 0; i < audioClipGuids.Length; i++)
            {
                var audioClipGuid = audioClipGuids[i];
                var stepSound = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(audioClipGuid));
                StepSounds[i] = stepSound;
            }
        } 
#endif
    }
}