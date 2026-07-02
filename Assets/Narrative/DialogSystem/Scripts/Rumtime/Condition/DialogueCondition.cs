using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话条件
    /// 连线上的一条判断规则
    /// </summary>
    [Serializable]
    public class DialogueCondition
    {
        /// <summary> 变量名 </summary>
        public string variableName;

        /// <summary> 条件类型 </summary>
        public ECondition eCondition;

        /// <summary> 浮点目标值 </summary>
        [ShowIf(nameof(IsFloatCondition))]
        public float targetFloat;

        /// <summary> 整数目标值 </summary>
        [ShowIf(nameof(IsIntCondition))]
        public int targetInt;

        /// <summary> 无条件 </summary>
        public bool NoneContion => eCondition == ECondition.None;

        bool IsFloatCondition => eCondition is ECondition.FloatGreater or ECondition.FloatLess;

        bool IsIntCondition => eCondition is ECondition.IntGreater or ECondition.IntLess
            or ECondition.IntEquals or ECondition.IntNotEquals;

        /// <summary>
        /// 判断条件是否满足
        /// </summary>
        public bool MeetCondition(DialogueVariablesStore variables)
        {
            if (NoneContion || variables == null)
                return true;

            return eCondition switch
            {
                ECondition.BoolTrue => variables.GetBool(variableName),
                ECondition.BoolFalse => !variables.GetBool(variableName),
                ECondition.FloatGreater => variables.GetFloat(variableName) > targetFloat,
                ECondition.FloatLess => variables.GetFloat(variableName) < targetFloat,
                ECondition.IntGreater => variables.GetInt(variableName) > targetInt,
                ECondition.IntLess => variables.GetInt(variableName) < targetInt,
                ECondition.IntEquals => variables.GetInt(variableName) == targetInt,
                ECondition.IntNotEquals => variables.GetInt(variableName) != targetInt,
                _ => false,
            };
        }
    }
}
