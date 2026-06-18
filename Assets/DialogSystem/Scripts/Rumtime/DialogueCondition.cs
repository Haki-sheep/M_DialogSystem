using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 条件类型
    /// </summary>
    public enum E_Condition
    {
        None,
        BoolEquals,
        FloatEquals,
        FloatNotEquals,
        FloatGreater,
        FloatGreaterOrEqual,
        FloatLess,
        FloatLessOrEqual,
    }

    /// <summary>
    /// 对话条件
    /// </summary>
    [Serializable]
    public class DialogueCondition
    {
        public E_Condition e_Condition;
        public string key;

        [ShowIf(nameof(IsBoolCondition))]
        public bool targetBool;

        [ShowIf(nameof(IsFloatCondition))]
        public float targetFloat;

        public bool NoneContion => e_Condition == E_Condition.None;

        bool IsBoolCondition => e_Condition == E_Condition.BoolEquals;
        bool IsFloatCondition => IsFloatCompare(e_Condition);

        /// <summary>
        /// 判断条件是否满足
        /// </summary>
        public bool MeetCondition(DialogueVariables vars)
        {
            if (NoneContion || vars == null)
                return true;

            return e_Condition switch
            {
                E_Condition.BoolEquals => vars.GetBool(key) == targetBool,
                E_Condition.FloatEquals => Mathf.Approximately(vars.GetFloat(key), targetFloat),
                E_Condition.FloatNotEquals => !Mathf.Approximately(vars.GetFloat(key), targetFloat),
                E_Condition.FloatGreater => vars.GetFloat(key) > targetFloat,
                E_Condition.FloatGreaterOrEqual => vars.GetFloat(key) >= targetFloat,
                E_Condition.FloatLess => vars.GetFloat(key) < targetFloat,
                E_Condition.FloatLessOrEqual => vars.GetFloat(key) <= targetFloat,
                _ => false,
            };
        }

        public static bool IsFloatCompare(E_Condition type) =>
            type is E_Condition.FloatEquals
                or E_Condition.FloatNotEquals
                or E_Condition.FloatGreater
                or E_Condition.FloatGreaterOrEqual
                or E_Condition.FloatLess
                or E_Condition.FloatLessOrEqual;
    }

    /// <summary>
    /// 对话连线
    /// </summary>
    [Serializable]
    public class DialogueLink
    {
        public DialogueNode toNode;
        public DialogueCondition condition;

        public bool CanPass(DialogueVariables vars) =>
            condition == null || condition.NoneContion || condition.MeetCondition(vars);
    }

    /// <summary>
    /// 对话选项
    /// </summary>
    [Serializable]
    public class DialogueChoice
    {
        public string labelText;
        public DialogueNode toNode;
        public DialogueCondition condition;

        public bool CanPass(DialogueVariables vars) =>
            condition == null || condition.NoneContion || condition.MeetCondition(vars);
    }
}
