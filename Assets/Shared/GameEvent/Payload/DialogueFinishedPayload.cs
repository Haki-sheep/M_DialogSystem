namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话图播放结束载荷
    /// </summary>
    public struct DialogueFinishedPayload
    {
        public int graphId;
        public DialogueGraph graph;
    }
}
