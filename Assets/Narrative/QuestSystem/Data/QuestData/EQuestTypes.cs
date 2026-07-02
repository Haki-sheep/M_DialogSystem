namespace Miemie.DialogSystem.Quest
{
  /// <summary>
  /// 任务分类
  /// </summary>
  public enum EQuestCategory { 主线任务, 支线任务 }

  /// <summary>
  /// 任务接受模式
  /// </summary>
  public enum EQuestAcceptMode { 系统派发, 手动接受 }

  /// <summary>
  /// 任务状态
  /// </summary>
  public enum EQuestState { 未激活, 可用, 执行中, 提交, 失败 }

  /// <summary>
  /// 任务目标类型
  /// </summary>
  public enum EQuestObjectiveType { 对话, 击杀, 收集, 到达 }
}
