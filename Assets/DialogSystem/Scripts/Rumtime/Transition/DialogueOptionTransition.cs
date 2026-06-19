using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 选项跳转 选项节点上每条出口对应一个实例
    /// </summary>
    [Serializable]
    public class DialogueOptionTransition
    {
        public string labelText;
        public DialogueNode toNode;

        [SerializeField]
        [FormerlySerializedAs("conditions")]
        List<DialogueCondition> conditionList = new();

        public List<DialogueCondition> ConditionList => conditionList;

        /// <summary>
        /// 判断选项是否可通过
        /// </summary>
        public bool CanPass(DialogueVariables vars)
        {
            if (conditionList == null || conditionList.Count == 0)
                return true;

            foreach (var item in conditionList)
            {
                if (item == null || item.NoneContion)
                    continue;
                if (!item.MeetCondition(vars))
                    return false;
            }
            return true;
        }
    }
}
