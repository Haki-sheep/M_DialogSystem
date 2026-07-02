namespace Miemie.DialogSystem
{
  /// <summary>
  /// 玩法层统一通知
  /// </summary>
  public static class GameNotify
  {
    /// <summary>
    /// 击杀敌人
    /// </summary>
    public static void Kill(string enemyKey, int count = 1)
    {
      GameEventBus.Bus.TriggerEvent(GameEventKey.EnemyKilled,
        new EnemyKilledEventData { enemyKey = enemyKey, count = count });
    }

    /// <summary>
    /// 收集物品
    /// </summary>
    public static void Collect(string itemKey, int count = 1)
    {
      GameEventBus.Bus.TriggerEvent(GameEventKey.ItemCollected,
        new ItemCollectedEventData { itemKey = itemKey, count = count });
    }

    /// <summary>
    /// 进入区域
    /// </summary>
    public static void EnterZone(string zoneKey)
    {
      GameEventBus.Bus.TriggerEvent(GameEventKey.ZoneEntered,
        new ZoneEnteredEventData { zoneKey = zoneKey });
    }

    /// <summary>
    /// 对话结束
    /// </summary>
    public static void DialogueFinished(DialogueGraph graph)
    {
      DialogueEvent(graph, DialogueFinishedEventData.GraphFinishedEventKey);
    }

    /// <summary>
    /// 对话事件
    /// </summary>
    public static void DialogueEvent(DialogueGraph graph, string eventKey)
    {
      GameEventBus.Bus.TriggerEvent(GameEventKey.DialogueEventTriggered,
        new DialogueFinishedEventData { graph = graph, eventKey = eventKey });
    }
  }
}
