using System.IO;
using UnityEngine;
namespace TaigaGames.Utils
{
    public static class UnityProject
    {
        private static string _path = null;
        
        public static string Path => _path ??= new DirectoryInfo(Application.dataPath).Parent?.FullName.Replace("\\", "/");
        public static string Name => System.IO.Path.GetDirectoryName(Path);
        public static string Assets => $"{Path}/Assets";
        public static string Library => $"{Path}/Library";
        public static string Packages => $"{Path}/Packages";
    }
}
