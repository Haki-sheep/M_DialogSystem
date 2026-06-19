using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话跳转类 
    /// 可以理解为其是连接节点的那根线
    /// 只不过线只是作为条件的载体,其持有但不做具体判断
    /// 真正在条件判断是在DialogueCondition类内
    /// 普通节点只持有一个跳转 选项节点持有多个跳转
    /// </summary>
    [Serializable]
    public class DialogueTransition
    {
        /// <summary> 跳向的节点 </summary>
        public DialogueNode toNode;

        /// <summary> 条件列表 </summary>
        [SerializeField]
        [FormerlySerializedAs("conditions")]
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
                if (item == null || item.NoneContion)
                    continue;

                if (!item.MeetCondition(vars))
                    return false;
            }

            return true;
        }
    }
}
