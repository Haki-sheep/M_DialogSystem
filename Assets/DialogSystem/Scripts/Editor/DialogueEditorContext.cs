#if UNITY_EDITOR
namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 编辑器上下文，记录当前正在编辑的 Graph
    /// </summary>
    public static class DialogueEditorContext
    {
        public static DialogueGraph CurrentGraph { get; set; }
    }
}
#endif
