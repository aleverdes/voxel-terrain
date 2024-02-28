using System.IO;
namespace TravkinGames.Utils
{
    public static class FileSystemInfoEx
    {
        public static string GetAssetPath(this FileSystemInfo fileInfo)
        {
            return fileInfo.FullName.Replace('\\', '/').Replace(UnityProject.Path, "").TrimStart('/');
        }
    }
}
