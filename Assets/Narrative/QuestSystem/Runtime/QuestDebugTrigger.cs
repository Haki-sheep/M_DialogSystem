using Miemie.DialogSystem;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
  /// <summary>
  /// 调试用手动触发游戏事件
  /// </summary>
  public class QuestDebugTrigger : MonoBehaviour
  {
    [Header("匹配 Key")]
    [SerializeField] string enemyKey = "Slime";
    [SerializeField] string wolfKey = "Wolf";
    [SerializeField] string bossKey = "Boss";
    [SerializeField] string itemKey = "Herb";
    [SerializeField] string zoneKey = "Forest";
    [SerializeField] int dialogueGraphId = 100;

    [Header("按键 K/L/J/T/U/I")]
    [SerializeField] KeyCode killKey = KeyCode.K;
    [SerializeField] KeyCode collectKey = KeyCode.L;
    [SerializeField] KeyCode zoneKeyCode = KeyCode.J;
    [SerializeField] KeyCode dialogueKey = KeyCode.T;
    [SerializeField] KeyCode wolfKillKey = KeyCode.U;
    [SerializeField] KeyCode bossKillKey = KeyCode.I;

    #region 输入

    void Update()
    {
      if (Input.GetKeyDown(killKey))
        FireKill(enemyKey);
      if (Input.GetKeyDown(wolfKillKey))
        FireKill(wolfKey);
      if (Input.GetKeyDown(bossKillKey))
        FireKill(bossKey);
      if (Input.GetKeyDown(collectKey))
        GameEventBus.Bus.TriggerEvent(GameEventKey.ItemCollected,
          new ItemCollectedPayload { itemKey = itemKey, count = 1 });
      if (Input.GetKeyDown(zoneKeyCode))
        GameEventBus.Bus.TriggerEvent(GameEventKey.ZoneEntered,
          new ZoneEnteredPayload { zoneKey = zoneKey });
      if (Input.GetKeyDown(dialogueKey))
        GameEventBus.Bus.TriggerEvent(GameEventKey.DialogueGraphFinished,
          new DialogueFinishedPayload { graphId = dialogueGraphId });
    }

    void FireKill(string key)
    {
      Debug.Log($"[QuestDebug] 击杀 {key}");
      GameEventBus.Bus.TriggerEvent(GameEventKey.EnemyKilled,
        new EnemyKilledPayload { enemyKey = key, count = 1 });
    }

    #endregion
  }
}
