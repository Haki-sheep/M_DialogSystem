#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话编辑器常用资产路径
    /// </summary>
    static class DialogueEditorPaths
    {
        public const string GraphAssetPath = "Assets/DialogSystem/NodeSo";
        public const string ExportFolder = "Assets/DialogSystem/Export";

        public static string ExportFolderAbsolute =>
            Path.Combine(Path.GetDirectoryName(Application.dataPath), ExportFolder.Replace('/', Path.DirectorySeparatorChar));

        public static void EnsureGraphAssetFolder()
        {
            if (AssetDatabase.IsValidFolder(GraphAssetPath))
                return;

            if (!AssetDatabase.IsValidFolder("Assets/DialogSystem"))
                AssetDatabase.CreateFolder("Assets", "DialogSystem");

            AssetDatabase.CreateFolder("Assets/DialogSystem", "NodeSo");
        }

        public static void EnsureExportFolder()
        {
            if (AssetDatabase.IsValidFolder(ExportFolder))
                return;

            if (!AssetDatabase.IsValidFolder("Assets/DialogSystem"))
                AssetDatabase.CreateFolder("Assets", "DialogSystem");

            AssetDatabase.CreateFolder("Assets/DialogSystem", "Export");
        }
    }
}
#endif
