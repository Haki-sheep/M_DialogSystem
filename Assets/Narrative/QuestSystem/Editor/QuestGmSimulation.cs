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
    public static string ButtonLabel(QuestObjective o)
    {
      if (o == null) return "模拟";
      return o.type switch
      {
        EQuestObjectiveType.对话 => "对话",
        EQuestObjectiveType.击杀 => "击杀",
        EQuestObjectiveType.收集 => "收集",
        EQuestObjectiveType.到达 => "到达",
        _ => "模拟",
      };
    }

    public static void FireOnce(QuestObjective o)
    {
      if (o == null) return;
      switch (o.type)
      {
        case EQuestObjectiveType.对话:
          GameEventBus.Bus.TriggerEvent(GameEventKey.DialogueGraphFinished,
            new DialogueFinishedPayload { graphId = o.targetId });
          break;
        case EQuestObjectiveType.击杀:
          GameEventBus.Bus.TriggerEvent(GameEventKey.EnemyKilled,
            new EnemyKilledPayload { enemyKey = o.targetKey, count = 1 });
          break;
        case EQuestObjectiveType.收集:
          GameEventBus.Bus.TriggerEvent(GameEventKey.ItemCollected,
            new ItemCollectedPayload { itemKey = o.targetKey, count = 1 });
          break;
        case EQuestObjectiveType.到达:
          GameEventBus.Bus.TriggerEvent(GameEventKey.ZoneEntered,
            new ZoneEnteredPayload { zoneKey = o.targetKey });
          break;
      }
    }
  }
}
#endif
