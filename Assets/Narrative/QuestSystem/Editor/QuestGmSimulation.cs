#if UNITY_EDITOR
using Miemie.DialogSystem;
using UnityEngine;

namespace Miemie.DialogSystem.Quest.Editor
{
  /// <summary>
  /// GM 面板按目标类型模拟一次游戏事件
  /// </summary>
  static class QuestGmSimulation
  {
    /// <summary>
    /// 模拟按钮显示文本
    /// </summary>
    public static string ButtonLabel(QuestObjective objective)
    {
      if (objective == null) return "模拟";
      return objective.type switch
      {
        EQuestObjectiveType.对话 => "对话",
        EQuestObjectiveType.击杀 => "击杀",
        EQuestObjectiveType.收集 => "收集",
        EQuestObjectiveType.到达 => "到达",
        _ => "模拟",
      };
    }

    /// <summary>
    /// 按目标类型触发一次游戏事件
    /// </summary>
    public static void FireOnce(QuestObjective objective)
    {
      if (objective == null) return;
      switch (objective.type)
      {
        case EQuestObjectiveType.对话:
          GameNotify.DialogueEvent(objective.dialogueGraph, objective.dialogueEventKey);
          break;
        case EQuestObjectiveType.击杀:
          GameNotify.Kill(objective.targetKey);
          break;
        case EQuestObjectiveType.收集:
          GameNotify.Collect(objective.targetKey);
          break;
        case EQuestObjectiveType.到达:
          GameNotify.EnterZone(objective.targetKey);
          break;
      }
    }
  }
}
#endif
