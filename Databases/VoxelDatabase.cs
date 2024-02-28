using System;
using Sirenix.OdinInspector;
using TravkinGames.Utils;
using UnityEngine;

namespace TravkinGames.Voxels
{
    [CreateAssetMenu(menuName = "Voxels/Voxel Database", fileName = "Voxel Database")]
    public class VoxelDatabase : BaseDatabase<VoxelDescriptor>
    {
    }
}