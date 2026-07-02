using System.Collections.Generic;
using Sirenix.Serialization;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话变量运行时存储
    /// </summary>
    public class DialogueVariablesStore
    {
        [OdinSerialize]
        private Dictionary<string, bool> boolDict = new();

        [OdinSerialize]
        private Dictionary<string, float> floatDict = new();

        [OdinSerialize]
        private Dictionary<string, int> intDict = new();

        /// <summary>
        /// 用变量声明初始化默认值
        /// </summary>
        public void ApplyDefaults(IReadOnlyList<DialogueVariableDef> variableList)
        {
            if (variableList == null)
                return;

            foreach (var def in variableList)
            {
                if (def == null || string.IsNullOrEmpty(def.name))
                    continue;

                switch (def.variableType)
                {
                    case EDialogueVariableType.Float:
                        SetFloat(def.name, def.defaultFloat);
                        break;
                    case EDialogueVariableType.Int:
                        SetInt(def.name, def.defaultInt);
                        break;
                    case EDialogueVariableType.Bool:
                        SetBool(def.name, def.defaultBool);
                        break;
                }
            }
        }

        /// <summary>
        /// 获取布尔值
        /// </summary>
        public bool GetBool(string variableName, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(variableName))
                return defaultValue;

            return boolDict.TryGetValue(variableName, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 设置布尔值
        /// </summary>
        public void SetBool(string variableName, bool value)
        {
            if (string.IsNullOrEmpty(variableName))
                return;

            boolDict[variableName] = value;
        }

        /// <summary>
        /// 获取浮点值
        /// </summary>
        public float GetFloat(string variableName, float defaultValue = 0f)
        {
            if (string.IsNullOrEmpty(variableName))
                return defaultValue;

            return floatDict.TryGetValue(variableName, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 设置浮点值
        /// </summary>
        public void SetFloat(string variableName, float value)
        {
            if (string.IsNullOrEmpty(variableName))
                return;

            floatDict[variableName] = value;
        }

        /// <summary>
        /// 获取整数值
        /// </summary>
        public int GetInt(string variableName, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(variableName))
                return defaultValue;

            return intDict.TryGetValue(variableName, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 设置整数值
        /// </summary>
        public void SetInt(string variableName, int value)
        {
            if (string.IsNullOrEmpty(variableName))
                return;

            intDict[variableName] = value;
        }
    }
}
