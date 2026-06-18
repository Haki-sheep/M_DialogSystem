using System.Collections.Generic;

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
        /// <summary> 布局 </summary>
        public LayoutJson layout;
        /// <summary> 链接列表 </summary>
        public List<DialogueLinkJson> linkList = new();
        /// <summary> 选项列表 </summary>
        public List<DialogueChoiceJson> choiceList = new();
    }

    /// <summary>
    /// 布局Json类
    /// </summary>
    public class LayoutJson
    {
        /// <summary> X坐标 </summary>
        public float x;
        /// <summary> Y坐标 </summary>
        public float y;
    }

    /// <summary>
    /// 对话链接Json类
    /// </summary>
    public class DialogueLinkJson
    {
        /// <summary> 目标节点ID </summary>
        public int toNodeId;
        public DialogueConditionJson condition;
    }

    /// <summary>
    /// 对话选项Json类
    /// </summary>
    public class DialogueChoiceJson
    {
        /// <summary> 选项文本 </summary>
        public string labelText;
        /// <summary> 目标节点ID </summary>
        public int toNodeId;
        /// <summary> 条件 </summary>
        public DialogueConditionJson condition;
    }

    /// <summary>
    /// 对话条件Json类
    /// </summary>
    public class DialogueConditionJson
    {
        /// <summary> 条件类型 </summary>
        public string conditionType;
        /// <summary> 条件键 </summary>
        public string key;
        /// <summary> 目标布尔值 </summary>
        public bool targetBool;
        /// <summary> 目标浮点值 </summary>
        public float targetFloat;
    }
}
