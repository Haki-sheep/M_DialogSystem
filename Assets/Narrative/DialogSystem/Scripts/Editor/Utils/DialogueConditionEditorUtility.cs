#if UNITY_EDITOR
using System;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 条件 Editor 显示与选项
    /// </summary>
    static class DialogueConditionEditorUtility
    {
        public static bool IsFloatThresholdType(ECondition type) =>
            type is ECondition.FloatGreater or ECondition.FloatLess;

        public static bool IsIntThresholdType(ECondition type) =>
            type is ECondition.IntGreater or ECondition.IntLess
                or ECondition.IntEquals or ECondition.IntNotEquals;

        /// <summary>
        /// 根据变量类型获取可用条件
        /// </summary>
        public static ECondition[] GetConditionOptions(EDialogueVariableType variableType) =>
            variableType switch
            {
                EDialogueVariableType.Float => new[]
                {
                    ECondition.FloatGreater,
                    ECondition.FloatLess,
                },
                EDialogueVariableType.Int => new[]
                {
                    ECondition.IntGreater,
                    ECondition.IntLess,
                    ECondition.IntEquals,
                    ECondition.IntNotEquals,
                },
                EDialogueVariableType.Bool => new[]
                {
                    ECondition.BoolTrue,
                    ECondition.BoolFalse,
                },
                _ => Array.Empty<ECondition>(),
            };

        /// <summary>
        /// 条件显示名
        /// </summary>
        public static string GetDisplayLabel(ECondition condition) =>
            condition switch
            {
                ECondition.BoolTrue => "True",
                ECondition.BoolFalse => "False",
                ECondition.FloatGreater => "大于",
                ECondition.FloatLess => "小于",
                ECondition.IntGreater => "大于",
                ECondition.IntLess => "小于",
                ECondition.IntEquals => "等于",
                ECondition.IntNotEquals => "不等于",
                ECondition.None => "无",
                _ => condition.ToString(),
            };
    }
}
#endif
