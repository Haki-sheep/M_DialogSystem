#if UNITY_EDITOR
using UnityEditor;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 选项节点选项增删
    /// </summary>
    static class DialogueNodeChoiceEditorUtility
    {
        public static void AddChoice(DialogueNode node)
        {
            if (node == null || !node.IsOptionNode)
                return;

            int nextIndex = (node.ChoiceList?.Count ?? 0) + 1;
            node.AddChoiceNode(new DialogueOptionTransition { labelText = $"选项{nextIndex}" });
            EditorUtility.SetDirty(node);
        }

        public static bool TryRemoveLastChoice(DialogueNode node)
        {
            if (node == null || !node.TryRemoveLastChoice())
                return false;

            EditorUtility.SetDirty(node);
            return true;
        }
    }
}
#endif
