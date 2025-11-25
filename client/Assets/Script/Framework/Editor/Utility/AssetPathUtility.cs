using System.IO;

namespace Framework.Editor.Utility
{
    public static class AssetPathUtility
    {
        private const string ResourcesFolderName = "Resources";
        public static string AssetPathToResourcesPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return string.Empty;
            }

            int resourcesIndex = assetPath.IndexOf(ResourcesFolderName, System.StringComparison.OrdinalIgnoreCase);
            if (resourcesIndex == -1)
                return string.Empty;

            int pathStart = resourcesIndex + ResourcesFolderName.Length + 1;
            if (pathStart >= assetPath.Length)
                return string.Empty;

            string relativePath = assetPath.Substring(pathStart);
            string pathWithoutExtension = Path.ChangeExtension(relativePath, null);

            return pathWithoutExtension;
        }
    }
}
