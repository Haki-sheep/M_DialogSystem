#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Miemie.Narrative.GraphViewFrame.Editor
{
    /// <summary>
    /// 按类型搜集 Graph 资产并填充 Odin 菜单树
    /// 不绑定固定文件夹 SO 放哪都能被找到
    /// </summary>
    public static class GraphAssetMenuTreeUtility
    {
        /// <summary>
        /// 将项目内指定类型资产加入菜单树
        /// searchRoots 为空时搜索整个 Assets
        /// </summary>
        public static void AddAllAssetsByType(
            OdinMenuTree tree,
            string rootMenuName,
            Type assetType,
            string[] searchRoots = null)
        {
            if (tree == null || assetType == null)
                return;

            string typeName = assetType.Name;
            string[] guids = searchRoots == null || searchRoots.Length == 0
                ? AssetDatabase.FindAssets($"t:{typeName}")
                : AssetDatabase.FindAssets($"t:{typeName}", searchRoots);

            var paths = new List<string>(guids.Length);
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(assetPath))
                    paths.Add(assetPath);
            }

            paths.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string assetPath in paths)
            {
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
                if (asset == null)
                    continue;

                tree.Add(BuildMenuPath(rootMenuName, assetPath), asset);
            }
        }

        /// <summary>
        /// 根据资产路径生成菜单层级
        /// </summary>
        public static string BuildMenuPath(string rootMenuName, string assetPath)
        {
            const string assetsPrefix = "Assets/";
            string relative = assetPath;
            if (relative.StartsWith(assetsPrefix, StringComparison.Ordinal))
                relative = relative.Substring(assetsPrefix.Length);

            int lastSlash = relative.LastIndexOf('/');
            string folderPart = lastSlash >= 0 ? relative.Substring(0, lastSlash) : string.Empty;
            string fileName = Path.GetFileNameWithoutExtension(lastSlash >= 0 ? relative.Substring(lastSlash + 1) : relative);
            fileName = SanitizeMenuSegment(fileName);

            if (string.IsNullOrEmpty(folderPart))
                return $"{rootMenuName}/{fileName}";

            string folderMenu = string.Join("/",
                folderPart.Split('/').Where(segment => !string.IsNullOrEmpty(segment)).Select(SanitizeMenuSegment));

            return $"{rootMenuName}/{folderMenu}/{fileName}";
        }

        static string SanitizeMenuSegment(string segment) =>
            segment.Replace("/", "／").Replace("\n", " ").Replace("\r", string.Empty);
    }
}
#endif
