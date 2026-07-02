namespace Miemie.DialogSystem.Quest
{
  /// <summary>
  /// 任务生命周期
  /// </summary>
  public interface IQuestLifeCycle
  {
    /// <summary>
    /// 接受任务
    /// </summary>
    void OnAccepted(QuestLifeCycleContext context);

    /// <summary>
    /// 任务进度变化
    /// </summary>
    void OnProgressChanged(QuestLifeCycleContext context);

    /// <summary>
    /// 完成任务
    /// </summary>
    void OnCompleted(QuestLifeCycleContext context);

    /// <summary>
    /// 任务失败
    /// </summary>
    void OnFailed(QuestLifeCycleContext context);
  }
}
