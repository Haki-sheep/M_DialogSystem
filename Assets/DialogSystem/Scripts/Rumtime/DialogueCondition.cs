using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话条件
    /// </summary>
    [Serializable]
    public class DialogueCondition
    {
        public E_Condition e_Condition;
        public string key;

        [ShowIf(nameof(UsesBoolValue))]
        public bool targetBool;

        [ShowIf(nameof(IsFloatCondition))]
        public float targetFloat;

        [ShowIf(nameof(IsIntCondition))]
        public int targetInt;

        public bool NoneContion => e_Condition == E_Condition.None;

        bool UsesBoolValue => e_Condition == E_Condition.BoolEquals;
        bool IsFloatCondition => NeedsThreshold(IsFloatThresholdType(e_Condition));
        bool IsIntCondition => NeedsThreshold(IsIntThresholdType(e_Condition));

        static bool NeedsThreshold(bool isThresholdType) => isThresholdType;

        /// <summary>
        /// 判断条件是否满足
        /// </summary>
        public bool MeetCondition(DialogueVariables vars)
        {
            if (NoneContion || vars == null)
                return true;

            return e_Condition switch
            {
                E_Condition.BoolTrue => vars.GetBool(key),
                E_Condition.BoolFalse => !vars.GetBool(key),
                E_Condition.BoolEquals => vars.GetBool(key) == targetBool,
                E_Condition.FloatGreater => vars.GetFloat(key) > targetFloat,
                E_Condition.FloatLess => vars.GetFloat(key) < targetFloat,
                E_Condition.FloatEquals => Mathf.Approximately(vars.GetFloat(key), targetFloat),
                E_Condition.FloatNotEquals => !Mathf.Approximately(vars.GetFloat(key), targetFloat),
                E_Condition.FloatGreaterOrEqual => vars.GetFloat(key) >= targetFloat,
                E_Condition.FloatLessOrEqual => vars.GetFloat(key) <= targetFloat,
                E_Condition.IntEquals => vars.GetInt(key) == targetInt,
                E_Condition.IntNotEquals => vars.GetInt(key) != targetInt,
                E_Condition.IntGreater => vars.GetInt(key) > targetInt,
                E_Condition.IntLess => vars.GetInt(key) < targetInt,
                E_Condition.IntGreaterOrEqual => vars.GetInt(key) >= targetInt,
                E_Condition.IntLessOrEqual => vars.GetInt(key) <= targetInt,
                E_Condition.Trigger => vars.IsTriggerSet(key),
                E_Condition.TriggerNotSet => !vars.IsTriggerSet(key),
                _ => false,
            };
        }

        /// <summary>
        /// 是否为会消耗触发器的条件
        /// </summary>
        public bool IsTriggerCondition => e_Condition == E_Condition.Trigger;

        public static bool IsFloatThresholdType(E_Condition type) =>
            type is E_Condition.FloatGreater or E_Condition.FloatLess
                or E_Condition.FloatEquals or E_Condition.FloatNotEquals
                or E_Condition.FloatGreaterOrEqual or E_Condition.FloatLessOrEqual;

        public static bool IsIntThresholdType(E_Condition type) =>
            type is E_Condition.IntEquals or E_Condition.IntNotEquals
                or E_Condition.IntGreater or E_Condition.IntLess
                or E_Condition.IntGreaterOrEqual or E_Condition.IntLessOrEqual;

        /// <summary>
        /// 根据参数类型获取可用条件
        /// </summary>
        public static E_Condition[] GetConditionOptions(E_DialogueParameterType parameterType) =>
            parameterType switch
            {
                E_DialogueParameterType.Float => new[]
                {
                    E_Condition.FloatGreater,
                    E_Condition.FloatLess,
                },
                E_DialogueParameterType.Int => new[]
                {
                    E_Condition.IntGreater,
                    E_Condition.IntLess,
                    E_Condition.IntEquals,
                    E_Condition.IntNotEquals,
                },
                E_DialogueParameterType.Bool => new[]
                {
                    E_Condition.BoolTrue,
                    E_Condition.BoolFalse,
                },
                E_DialogueParameterType.Trigger => new[]
                {
                    E_Condition.Trigger,
                    E_Condition.TriggerNotSet,
                },
                _ => Array.Empty<E_Condition>(),
            };

        /// <summary>
        /// Animator 风格显示名
        /// </summary>
        public static string GetDisplayLabel(E_Condition condition) =>
            condition switch
            {
                E_Condition.FloatGreater => "大于",
                E_Condition.FloatLess => "小于",
                E_Condition.IntGreater => "大于",
                E_Condition.IntLess => "小于",
                E_Condition.IntEquals => "等于",
                E_Condition.IntNotEquals => "不等于",
                E_Condition.BoolTrue => "是",
                E_Condition.BoolFalse => "否",
                E_Condition.Trigger => "触发",
                E_Condition.TriggerNotSet => "不触发",
                E_Condition.None => "无",
                _ => condition.ToString(),
            };
    }
}
