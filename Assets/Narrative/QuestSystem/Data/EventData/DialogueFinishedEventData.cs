namespace Miemie.DialogSystem
{
  /// <summary>
  /// 对话事件数据
  /// </summary>
  public struct DialogueFinishedEventData
  {
    /// <summary>
    /// 对话结束事件Key
    /// </summary>
    public const string GraphFinishedEventKey = "GraphFinished";

    /// <summary>
    /// 对话图
    /// </summary>
    public DialogueGraph graph;

    /// <summary>
    /// 对话事件Key
    /// </summary>
    public string eventKey;
  }
}
