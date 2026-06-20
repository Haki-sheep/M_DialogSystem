using System.Collections.Generic;
using Sirenix.Serialization;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话变量
    /// 用于存储对话中的变量值
    /// </summary>
    public class DialogueVariables
    {
        [OdinSerialize]
        private Dictionary<string, bool> boolDict = new();

        [OdinSerialize]
        private Dictionary<string, float> floatDict = new();

        [OdinSerialize]
        private Dictionary<string, int> intDict = new();

        /// <summary>
        /// 用对话图参数定义初始化默认值
        /// </summary>
        public void ApplyDefaults(IReadOnlyList<DialogueParameterDefinition> parameterList)
        {
            if (parameterList == null)
                return;

            foreach (var param in parameterList)
            {
                if (param == null || string.IsNullOrEmpty(param.name))
                    continue;

                switch (param.parameterType)
                {
                    case EDialogueParameterType.Float:
                        SetFloat(param.name, param.defaultFloat);
                        break;
                    case EDialogueParameterType.Int:
                        SetInt(param.name, param.defaultInt);
                        break;
                    case EDialogueParameterType.Bool:
                        SetBool(param.name, param.defaultBool);
                        break;
                }
            }
        }

        /// <summary>
        /// 获取布尔值
        /// </summary>
        public bool GetBool(string key, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            return boolDict.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 设置布尔值
        /// </summary>
        public void SetBool(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            boolDict[key] = value;
        }

        /// <summary>
        /// 获取浮点值
        /// </summary>
        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            return floatDict.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 设置浮点值
        /// </summary>
        public void SetFloat(string key, float value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            floatDict[key] = value;
        }

        /// <summary>
        /// 获取整数值
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            return intDict.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// 设置整数值
        /// </summary>
        public void SetInt(string key, int value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            intDict[key] = value;
        }
    }
}
