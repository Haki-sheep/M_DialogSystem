using System;
using System.Collections.Generic;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话选项
    /// </summary>
    [Serializable]
    public class DialogueChoice
    {
        public string labelText;
        public DialogueNode toNode;

        [SerializeField]
        List<DialogueCondition> conditions = new();

        public List<DialogueCondition> Conditions => conditions;

        /// <summary>
        /// 获取有效条件列表
        /// </summary>
        public List<DialogueCondition> GetEffectiveConditions()
        {
            EnsureConditionsList();
            return conditions;
        }

        /// <summary>
        /// 判断选项是否可通过
        /// </summary>
        public bool CanPass(DialogueVariables vars)
        {
            EnsureConditionsList();

            if (conditions == null || conditions.Count == 0)
                return true;

            foreach (var item in conditions)
            {
                if (item == null || item.NoneContion)
                    continue;
                if (!item.MeetCondition(vars))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 收集本选项上的触发器条件
        /// </summary>
        public void CollectTriggerKeys(List<string> result)
        {
            EnsureConditionsList();
            if (conditions == null || result == null)
                return;

            foreach (var item in conditions)
            {
                if (item != null && item.IsTriggerCondition && !string.IsNullOrEmpty(item.key))
                    result.Add(item.key);
            }
        }

        void EnsureConditionsList()
        {
            if (conditions == null)
                conditions = new List<DialogueCondition>();
        }
    }
}
