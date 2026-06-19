using System;
using System.Collections.Generic;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话跳转 普通节点单出口
    /// </summary>
    [Serializable]
    public class DialogueTransition
    {
        /// <summary> 跳向的节点 </summary>
        public DialogueNode toNode;

        /// <summary> 条件列表 </summary>
        [SerializeField]
        List<DialogueCondition> conditionList = new();

        public List<DialogueCondition> ConditionList => conditionList;

        /// <summary>
        /// 判断跳转是否可通过
        /// </summary>
        public bool CanPass(DialogueVariables vars)
        {
            if (conditionList == null || conditionList.Count == 0)
                return true;

            foreach (var item in conditionList)
            {
                // 如果条件为空或为无条件 则跳过
                if (item == null || item.NoneContion)
                    continue;

                // 如果条件不满足 则返回false
                if (!item.MeetCondition(vars))
                    return false;
            }

            return true;
        }
    }
}
