using System;
using System.Collections.Generic;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 旧版连线 仅迁移
    /// </summary>
    [Serializable]
    public class DialogueLink
    {
        public DialogueNode toNode;

        [SerializeField]
        DialogueCondition condition = new();

        [SerializeField]
        List<DialogueCondition> conditions = new();

        public List<DialogueCondition> GetEffectiveConditions()
        {
            if (conditions == null)
                conditions = new List<DialogueCondition>();

            if (conditions.Count == 0 && condition != null && !condition.NoneContion)
                conditions.Add(condition);

            return conditions;
        }
    }
}
