using System.Collections.Generic;
using AleVerDes.UnityUtils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AleVerDes.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Voxel Database", fileName = "Voxel Database")]
    public class VoxelDatabase : ScriptableObject
    {
        [SerializeField] private List<Voxel> _voxels;
        
        public IEnumerable<Voxel> Voxels => _voxels;
        
        public Voxel this[int index] => _voxels[index];
        
        public int Count => _voxels.Count;
        
        public void Add(Voxel voxel)
        {
            _voxels.Add(voxel);
        }
        
        public void Remove(Voxel voxel)
        {
            _voxels.Remove(voxel);
        }
        
        public void RemoveAt(int index)
        {
            _voxels.RemoveAt(index);
        }
        
        public void Clear()
        {
            _voxels.Clear();
        }
        
        public bool Contains(Voxel voxel)
        {
            return _voxels.Contains(voxel);
        }
        
        public int IndexOf(Voxel voxel)
        {
            return _voxels.IndexOf(voxel);
        }

        [Button("Add all voxels in project")]
        public void AddAllVVoxelsInProject()
        {
            var voxels = AssetDatabaseUtils.FindObjects<Voxel>();
            foreach (var voxel in voxels)
                if (!_voxels.Contains(voxel))
                    _voxels.Add(voxel);
        }
    }
}