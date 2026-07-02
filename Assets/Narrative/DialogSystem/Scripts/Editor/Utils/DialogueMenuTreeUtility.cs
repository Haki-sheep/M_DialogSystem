#if UNITY_EDITOR
namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 左侧树菜单文本工具
    /// </summary>
    static class DialogueMenuTreeUtility
    {
        public static string BuildNodeHeader(DialogueNode node) =>
            $"[{node.NodeId}] {node.name}";

        public static string SanitizeMenuPath(string path) =>
            path.Replace("/", "／").Replace("\n", " ").Replace("\r", string.Empty);

        public static string Truncate(string text, int max)
        {
            if (string.IsNullOrEmpty(text))
                return "(空)";
            return text.Length <= max ? text : text.Substring(0, max) + "…";
        }
    }
}
#endif
