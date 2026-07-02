using System;
using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 任务目标声明
    /// </summary>
    [Serializable]
    public class QuestObjectiveDef
    {
        public EQuestObjectiveType objectiveType;

        /// <summary> 对话图 GraphId 等数值目标 </summary>
        public int targetId;

        /// <summary> 敌人 物品 区域等字符串目标 </summary>
        public string targetKey;

        public int requiredCount = 1;

        /// <summary> 追踪 UI 文案 可覆盖默认描述 </summary>
        public string description;
    }
}
