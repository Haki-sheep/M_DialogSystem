using System.Collections.Generic;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话图Json类
    /// </summary>
    public class DialogueGraphJson
    {
        /// <summary> 对话图ID </summary>
        public int graphId;

        /// <summary> 对话图名称 </summary>
        public string graphName;

        /// <summary> 对话图资产名称 </summary>
        public string assetName;

        /// <summary> 对话图资产路径 </summary>
        public string assetPath;

        /// <summary> 开始节点ID </summary>
        public int startNodeId;

        /// <summary> 对话节点列表 </summary>
        public List<DialogueNodeJson> nodes = new();

        /// <summary> 图变量声明 </summary>
        public List<DialogueVariableJson> variables = new();
    }

    /// <summary>
    /// 对话变量Json类
    /// </summary>
    public class DialogueVariableJson
    {
        /// <summary> 变量名 </summary>
        public string name;

        /// <summary> 变量类型 </summary>
        public string variableType;

        /// <summary> 默认浮点 </summary>
        public float defaultFloat;

        /// <summary> 默认整数 </summary>
        public int defaultInt;

        /// <summary> 默认布尔 </summary>
        public bool defaultBool;
    }

    /// <summary>
    /// 对话节点Json类
    /// </summary>
    public class DialogueNodeJson
    {
        /// <summary> 节点ID </summary>
        public int nodeId;
        /// <summary> 节点资产名称 </summary>
        public string assetName;
        /// <summary> 说话类型 </summary>
        public string speakType;
        /// <summary> 说话者名称 </summary>
        public string speakerName;
        /// <summary> 对话文本 </summary>
        public string dialogText;
        /// <summary> 是否是选项节点 </summary>
        public bool isOptionNode;
        /// <summary> 布局位置 </summary>
        public Vector2 layout;
        /// <summary> 普通节点下一跳 </summary>
        public int nextNodeId;
        /// <summary> 普通节点下一跳条件 </summary>
        public List<DialogueConditionJson> transitionConditionList = new();
        /// <summary> 选项列表 </summary>
        public List<DialogueTransitionJson> choiceList = new();
    }

    /// <summary>
    /// 跳转 Json 选项出口带 labelText
    /// </summary>
    public class DialogueTransitionJson
    {
        /// <summary> 选项文本 </summary>
        public string labelText;
        /// <summary> 目标节点ID </summary>
        public int toNodeId;
        public List<DialogueConditionJson> conditionList = new();
    }

    /// <summary>
    /// 对话条件Json类
    /// </summary>
    public class DialogueConditionJson
    {
        /// <summary> 条件类型 </summary>
        public string conditionType;

        /// <summary> 变量名 </summary>
        public string variableName;

        /// <summary> 目标浮点值 </summary>
        public float targetFloat;

        /// <summary> 目标整数值 </summary>
        public int targetInt;
    }
}
