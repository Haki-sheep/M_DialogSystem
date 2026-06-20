using System;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话变量类型
    /// </summary>
    public enum EDialogueVariableType
    {
        Float,
        Int,
        Bool,
    }

    /// <summary>
    /// 对话图变量声明
    /// Editor 窗口 Variables 面板中一条
    /// </summary>
    [Serializable]
    public class DialogueVariableDef
    {
        /// <summary> 变量名 </summary>
        [SerializeField]
        public string name;

        /// <summary> 变量类型 </summary>
        [SerializeField]
        public EDialogueVariableType variableType;

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
