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
    }
}
