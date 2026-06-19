using System;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 跳转参数类型枚举
    /// </summary>
    public enum EDialogueParameterType
    {
        Float,
        Int,
        Bool,
    }

    /// <summary>
    /// 对话图参数类
    /// 就是Editor窗口中Parameters面板中一条信息
    /// </summary>
    [Serializable]
    public class DialogueParameterDefinition
    {
        /// <summary> 参数名 </summary>
        [SerializeField]
        public string name;

        /// <summary> 参数类型 </summary>
        [SerializeField]
        public EDialogueParameterType parameterType;

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
