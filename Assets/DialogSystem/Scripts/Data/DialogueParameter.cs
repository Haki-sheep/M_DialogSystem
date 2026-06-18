using System;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话参数类型
    /// </summary>
    public enum E_DialogueParameterType
    {
        Float,
        Int,
        Bool,
        Trigger,
    }

    /// <summary>
    /// 对话图参数定义
    /// </summary>
    [Serializable]
    public class DialogueParameterDefinition
    {
        public string name;
        public E_DialogueParameterType parameterType;
        public float defaultFloat;
        public int defaultInt;
        public bool defaultBool;
    }
}
