using Miemie.DialogSystem;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 调试用手动触发游戏事件
    /// </summary>
    public class QuestDebugTrigger : MonoBehaviour
    {
        [SerializeField]
        string enemyKey = "Slime";

        [SerializeField]
        string itemKey = "Herb";

        [SerializeField]
        string zoneKey = "Forest";

        [SerializeField]
        KeyCode killKey = KeyCode.K;

        [SerializeField]
        KeyCode collectKey = KeyCode.L;

        [SerializeField]
        KeyCode zoneKeyCode = KeyCode.J;

        #region 输入

        void Update()
        {
            if (Input.GetKeyDown(killKey))
            {
                GameEventBus.Bus.TriggerEvent(
                    GameEventKey.EnemyKilled,
                    new EnemyKilledPayload { enemyKey = enemyKey, count = 1 });
            }

            if (Input.GetKeyDown(collectKey))
            {
                GameEventBus.Bus.TriggerEvent(
                    GameEventKey.ItemCollected,
                    new ItemCollectedPayload { itemKey = itemKey, count = 1 });
            }

            if (Input.GetKeyDown(zoneKeyCode))
            {
                GameEventBus.Bus.TriggerEvent(
                    GameEventKey.ZoneEntered,
                    new ZoneEnteredPayload { zoneKey = zoneKey });
            }
        }

        #endregion
    }
}
