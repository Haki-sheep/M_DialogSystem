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
    public const string QuestAssetPath = "Assets/Narrative/QuestSystem/QuestSo";

    /// <summary>
    /// 确保任务资产目录存在
    /// </summary>
    public static void EnsureQuestFolder()
    {
      if (AssetDatabase.IsValidFolder(QuestAssetPath))
        return;

      if (!AssetDatabase.IsValidFolder("Assets/Narrative"))
        AssetDatabase.CreateFolder("Assets", "Narrative");
      if (!AssetDatabase.IsValidFolder("Assets/Narrative/QuestSystem"))
        AssetDatabase.CreateFolder("Assets/Narrative", "QuestSystem");
      AssetDatabase.CreateFolder("Assets/Narrative/QuestSystem", "QuestSo");
    }
  }
}
#endif
