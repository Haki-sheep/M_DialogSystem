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
    }

    /// <summary>
    /// 对话图参数定义
    /// </summary>
    [Serializable]
    public class DialogueParameterDefinition
    {
        /// <summary> 参数名 </summary>
        [SerializeField]
        public string name;
        /// <summary> 参数类型 </summary>
        [SerializeField]
        public E_DialogueParameterType parameterType;
        /// <summary> 默认浮点 </summary>
        [SerializeField]
        public float defaultFloat;
        /// <summary> 默认整数 </summary>
        [SerializeField]
        public int defaultInt;
        /// <summary> 默认布尔 </summary>
        [SerializeField]
        public bool defaultBool;
    }
}
