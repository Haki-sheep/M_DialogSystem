using System;
using System.Collections.Generic;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话跳转
    /// 普通节点 Out 与选项节点出口共用
    /// labelText 仅选项出口使用
    /// </summary>
    [Serializable]
    public class DialogueTransition
    {
        /// <summary> 选项文本 普通跳转可留空 </summary>
        public string labelText;

        /// <summary> 跳向的节点 </summary>
        public DialogueNode toNode;

        /// <summary> 条件列表 </summary>
        [SerializeField]
        List<DialogueCondition> conditionList = new();

        public List<DialogueCondition> ConditionList => conditionList;

        /// <summary>
        /// 判断跳转是否可通过
        /// </summary>
        public bool CanPass(DialogueVariablesStore variables)
        {
            if (conditionList == null || conditionList.Count == 0)
                return true;

            foreach (var item in conditionList)
            {
                if (item == null || item.NoneContion)
                    continue;

                if (!item.MeetCondition(variables))
                    return false;
            }

            return true;
        }
    }
}
