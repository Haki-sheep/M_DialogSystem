using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 条件类型
    /// </summary>
    public enum E_Condition
    {
        None,
        BoolEquals,
    }

    /// <summary>
    /// 对话条件类
    /// 该类就是"条件"本身
    /// </summary>
    [Serializable]
    public class DialogueCondition
    {
        public E_Condition e_Condition;
        public string key;
        // 目标值
        public bool targetBool;

        // 无条件
        public bool NoneContion => e_Condition == E_Condition.None;

        /// <summary>
        /// 判断条件是否满足
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        public bool MeetCondition(DialogueVariables vars)
        {
            if (NoneContion || vars is null) return true;

            return e_Condition switch
            {
                E_Condition.BoolEquals => vars.GetBool(key) == targetBool,
                _ => false,
            };
        }
    }

    /// <summary>
    /// 对话连线类
    /// 该类就是"条件"与"节点"之间的连线
    /// </summary>
    [Serializable]
    public class DialogueLink
    {
        public DialogueNode toNode;
        public DialogueCondition condition;

        /// <summary>
        /// 判断是否可以通行
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        public bool CanPass(DialogueVariables vars)
        {
            return this.condition.NoneContion || this.condition.MeetCondition(vars);
        }
    }


    /// <summary>
    /// 对话选项类
    /// 该类就是"选项"本身
    /// </summary>
    [Serializable]
    public class DialogueChoice{
        public string labelText;
        public DialogueNode toNode;
        // 该条件一般用于判断是否显示该选项
        public DialogueCondition condition;

        /// <summary>
        /// 判断是否可以通行
        /// </summary>
        /// <param name="vars"></param>
        /// <returns></returns>
        public bool CanPass(DialogueVariables vars){
            return this.condition.NoneContion || this.condition.MeetCondition(vars);
        }
    }
}