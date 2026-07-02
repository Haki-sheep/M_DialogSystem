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
        /// <summary> 新建对话图/节点的默认落点 不参与菜单扫描 </summary>
        public const string GraphAssetPath = "Assets/Narrative/DialogSystem/NodeSo";
        public const string LayoutAssetPath = "Assets/Narrative/DialogSystem/NodeSo/DialogueGraphLayouts.asset";
        public const string ExportFolder = "Assets/Narrative/DialogSystem/Export";

        public static string ExportFolderAbsolute =>
            Path.Combine(Path.GetDirectoryName(Application.dataPath), ExportFolder.Replace('/', Path.DirectorySeparatorChar));

        public static void EnsureGraphAssetFolder()
        {
            if (AssetDatabase.IsValidFolder(GraphAssetPath))
                return;

            EnsureFolder("Assets", "Narrative");
            EnsureFolder("Assets/Narrative", "DialogSystem");
            AssetDatabase.CreateFolder("Assets/Narrative/DialogSystem", "NodeSo");
        }

        public static void EnsureExportFolder()
        {
            if (AssetDatabase.IsValidFolder(ExportFolder))
                return;

            EnsureFolder("Assets", "Narrative");
            EnsureFolder("Assets/Narrative", "DialogSystem");
            AssetDatabase.CreateFolder("Assets/Narrative/DialogSystem", "Export");
        }

        static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
