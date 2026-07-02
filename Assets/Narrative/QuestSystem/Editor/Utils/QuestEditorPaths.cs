#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Quest.Editor
{
    /// <summary>
    /// 任务编辑器资产路径
    /// </summary>
    static class QuestEditorPaths
    {
        /// <summary> 新建任务图的默认落点 不参与菜单扫描 </summary>
        public const string GraphAssetPath = "Assets/Narrative/QuestSystem/QuestSo";
        public const string LayoutAssetPath = "Assets/Narrative/QuestSystem/QuestSo/QuestGraphLayouts.asset";

        public static void EnsureGraphAssetFolder()
        {
            if (AssetDatabase.IsValidFolder(GraphAssetPath))
                return;

            EnsureFolder("Assets", "Narrative");
            EnsureFolder("Assets/Narrative", "QuestSystem");
            AssetDatabase.CreateFolder("Assets/Narrative/QuestSystem", "QuestSo");
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
