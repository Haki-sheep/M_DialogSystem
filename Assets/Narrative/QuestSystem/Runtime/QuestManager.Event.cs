using Miemie.DialogSystem;

namespace Miemie.DialogSystem.Quest
{
  public partial class QuestManager
  {
    /// <summary>
    /// 订阅玩法事件
    /// </summary>
    private void ListenGameEvents()
    {
      GameEventBus.Bus.AddEventListener<EnemyKilledEventData>(GameEventKey.EnemyKilled, OnEnemyKilled);
      GameEventBus.Bus.AddEventListener<ItemCollectedEventData>(GameEventKey.ItemCollected, OnItemCollected);
      GameEventBus.Bus.AddEventListener<ZoneEnteredEventData>(GameEventKey.ZoneEntered, OnZoneEntered);
      GameEventBus.Bus.AddEventListener<DialogueFinishedEventData>(GameEventKey.DialogueEventTriggered, OnDialogueEvent);
    }

    /// <summary>
    /// 停止订阅玩法事件
    /// </summary>
    private void StopListenGameEvents()
    {
      GameEventBus.Bus.RemoveListener<EnemyKilledEventData>(GameEventKey.EnemyKilled, OnEnemyKilled);
      GameEventBus.Bus.RemoveListener<ItemCollectedEventData>(GameEventKey.ItemCollected, OnItemCollected);
      GameEventBus.Bus.RemoveListener<ZoneEnteredEventData>(GameEventKey.ZoneEntered, OnZoneEntered);
      GameEventBus.Bus.RemoveListener<DialogueFinishedEventData>(GameEventKey.DialogueEventTriggered, OnDialogueEvent);
    }

    /// <summary>
    /// 收到击杀事件
    /// </summary>
    private void OnEnemyKilled(EnemyKilledEventData eventData)
    {
      AdvanceByKey(EQuestObjectiveType.击杀, eventData.enemyKey, eventData.count);
    }

    /// <summary>
    /// 收到收集事件
    /// </summary>
    private void OnItemCollected(ItemCollectedEventData eventData)
    {
      AdvanceByKey(EQuestObjectiveType.收集, eventData.itemKey, eventData.count);
    }

    /// <summary>
    /// 收到进入区域事件
    /// </summary>
    private void OnZoneEntered(ZoneEnteredEventData eventData)
    {
      AdvanceByKey(EQuestObjectiveType.到达, eventData.zoneKey, 1);
    }

    /// <summary>
    /// 收到对话事件
    /// </summary>
    private void OnDialogueEvent(DialogueFinishedEventData eventData)
    {
      AdvanceDialogue(eventData.graph, eventData.eventKey, 1);
    }
  }
}
