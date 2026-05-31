using System.Collections.Generic;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话图 JSON 导出/导入用的纯数据模型（不依赖 Unity 资产引用）。
    /// </summary>
    public class DialogueGraphJson
    {
        public int graphId;
        public string graphName;
        public string assetName;
        public string assetPath;
        public int startNodeId;
        public List<DialogueNodeJson> nodes = new();
    }

    public class DialogueNodeJson
    {
        public int nodeId;
        public string assetName;
        public string speakType;
        public string speakerName;
        public string dialogText;
        public bool isOptionNode;
        public LayoutJson layout;
        public List<DialogueLinkJson> links = new();
        public List<DialogueChoiceJson> choices = new();
    }

    public class LayoutJson
    {
        public float x;
        public float y;
    }

    public class DialogueLinkJson
    {
        public int toNodeId;
        public DialogueConditionJson condition;
    }

    public class DialogueChoiceJson
    {
        public string labelText;
        public int toNodeId;
        public DialogueConditionJson condition;
    }

    public class DialogueConditionJson
    {
        public string conditionType;
        public string key;
        public bool targetBool;
    }
}
