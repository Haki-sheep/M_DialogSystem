using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话条件
    /// 也就是当你点击某根线以后右侧面板上点击添加条件按钮出现的参数
    /// </summary>
    [Serializable]
    public class DialogueCondition
    {
        /// <summary> 条件类型 </summary>
        public ECondition eCondition;
        /// <summary> 参数名 </summary>
        public string key;

        /// <summary> 浮点阈值 </summary>
        [ShowIf(nameof(IsFloatCondition))]
        public float targetFloat;

        /// <summary> 整数阈值 </summary>
        [ShowIf(nameof(IsIntCondition))]
        public int targetInt;

        /// <summary> 无条件 </summary>
        public bool NoneContion => eCondition == ECondition.None;

        /// <summary> 是否是浮点数条件 </summary>
        bool IsFloatCondition => eCondition is ECondition.FloatGreater or ECondition.FloatLess;

        /// <summary> 是否是整数条件 </summary>
        bool IsIntCondition => eCondition is ECondition.IntGreater or ECondition.IntLess
            or ECondition.IntEquals or ECondition.IntNotEquals;

        /// <summary>
        /// 判断条件是否满足
        /// </summary>
        public bool MeetCondition(DialogueVariables vars)
        {
            if (NoneContion || vars == null)
                return true;

            return eCondition switch
            {
                ECondition.BoolTrue => vars.GetBool(key),
                ECondition.BoolFalse => !vars.GetBool(key),
                ECondition.FloatGreater => vars.GetFloat(key) > targetFloat,
                ECondition.FloatLess => vars.GetFloat(key) < targetFloat,
                ECondition.IntGreater => vars.GetInt(key) > targetInt,
                ECondition.IntLess => vars.GetInt(key) < targetInt,
                ECondition.IntEquals => vars.GetInt(key) == targetInt,
                ECondition.IntNotEquals => vars.GetInt(key) != targetInt,
                _ => false,
            };
        }
    }
}
