#if UNITY_EDITOR
using Miemie.DialogSystem;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
  /// <summary>
  /// 调试用手动触发游戏事件 仅编辑器可用
  /// </summary>
  public class QuestDebugTrigger : MonoBehaviour
  {
    [Header("匹配 Key")]
    [SerializeField] private string enemyKey = "Slime";
    [SerializeField] private string wolfKey = "Wolf";
    [SerializeField] private string bossKey = "Boss";
    [SerializeField] private string itemKey = "Herb";
    [SerializeField] private string zoneKey = "Forest";
    [SerializeField] private DialogueGraph dialogueGraph;
    [SerializeField] private string dialogueEventKey = DialogueFinishedEventData.GraphFinishedEventKey;

    [Header("按键 K/L/J/T/U/I")]
    [SerializeField] private KeyCode killKey = KeyCode.K;
    [SerializeField] private KeyCode collectKey = KeyCode.L;
    [SerializeField] private KeyCode zoneKeyCode = KeyCode.J;
    [SerializeField] private KeyCode dialogueKey = KeyCode.T;
    [SerializeField] private KeyCode wolfKillKey = KeyCode.U;
    [SerializeField] private KeyCode bossKillKey = KeyCode.I;

    #region 输入

    /// <summary>
    /// 检测按键触发调试事件
    /// </summary>
    private void Update()
    {
      if (Input.GetKeyDown(killKey))
        GameNotify.Kill(enemyKey);
      if (Input.GetKeyDown(wolfKillKey))
        GameNotify.Kill(wolfKey);
      if (Input.GetKeyDown(bossKillKey))
        GameNotify.Kill(bossKey);
      if (Input.GetKeyDown(collectKey))
        GameNotify.Collect(itemKey);
      if (Input.GetKeyDown(zoneKeyCode))
        GameNotify.EnterZone(zoneKey);
      if (Input.GetKeyDown(dialogueKey))
        GameNotify.DialogueEvent(dialogueGraph, dialogueEventKey);
    }

    #endregion
  }
}
#endif
