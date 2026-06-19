using System;
using System.Collections.Generic;
using UnityEngine;

namespace Miemie.DialogSystem
{
    [Serializable]
    public class DialogueVariables
    {
        [SerializeField] List<BoolEntry> boolEntries = new();
        [SerializeField] List<FloatEntry> floatEntries = new();
        [SerializeField] List<IntEntry> intEntries = new();

        /// <summary>
        /// 用对话图参数定义初始化默认值
        /// </summary>
        public void ApplyDefaults(IReadOnlyList<DialogueParameterDefinition> parameters)
        {
            if (parameters == null)
                return;

            foreach (var param in parameters)
            {
                if (param == null || string.IsNullOrEmpty(param.name))
                    continue;

                switch (param.parameterType)
                {
                    case E_DialogueParameterType.Float:
                        SetFloat(param.name, param.defaultFloat);
                        break;
                    case E_DialogueParameterType.Int:
                        SetInt(param.name, param.defaultInt);
                        break;
                    case E_DialogueParameterType.Bool:
                        SetBool(param.name, param.defaultBool);
                        break;
                }
            }
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            foreach (var entry in boolEntries)
            {
                if (entry.key == key)
                    return entry.value;
            }

            return defaultValue;
        }

        public void SetBool(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            for (int i = 0; i < boolEntries.Count; i++)
            {
                if (boolEntries[i].key != key)
                    continue;

                boolEntries[i] = new BoolEntry { key = key, value = value };
                return;
            }

            boolEntries.Add(new BoolEntry { key = key, value = value });
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            foreach (var entry in floatEntries)
            {
                if (entry.key == key)
                    return entry.value;
            }

            return defaultValue;
        }

        public void SetFloat(string key, float value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            for (int i = 0; i < floatEntries.Count; i++)
            {
                if (floatEntries[i].key != key)
                    continue;

                floatEntries[i] = new FloatEntry { key = key, value = value };
                return;
            }

            floatEntries.Add(new FloatEntry { key = key, value = value });
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            foreach (var entry in intEntries)
            {
                if (entry.key == key)
                    return entry.value;
            }

            return defaultValue;
        }

        public void SetInt(string key, int value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            for (int i = 0; i < intEntries.Count; i++)
            {
                if (intEntries[i].key != key)
                    continue;

                intEntries[i] = new IntEntry { key = key, value = value };
                return;
            }

            intEntries.Add(new IntEntry { key = key, value = value });
        }

        [Serializable]
        struct BoolEntry
        {
            public string key;
            public bool value;
        }

        [Serializable]
        struct FloatEntry
        {
            public string key;
            public float value;
        }

        [Serializable]
        struct IntEntry
        {
            public string key;
            public int value;
        }
    }
}
