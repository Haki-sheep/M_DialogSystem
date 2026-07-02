using Miemie.DialogSystem.Quest;

namespace Miemie.DialogSystem
{
  /// <summary>
  /// 任务进度变化事件数据
  /// </summary>
  public struct QuestProgressChangedEventData
  {
    /// <summary>
    /// 任务ID
    /// </summary>
    public int questId;

    /// <summary>
    /// 目标序号
    /// </summary>
    public int objectiveIndex;

    /// <summary>
    /// 目标类型
    /// </summary>
    public EQuestObjectiveType eObjectiveType;

    /// <summary>
    /// 当前进度
    /// </summary>
    public int currentCount;

    /// <summary>
    /// 目标数量
    /// </summary>
    public int needCount;
  }
}
