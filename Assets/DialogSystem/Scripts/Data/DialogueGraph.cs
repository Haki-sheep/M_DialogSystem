  using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Miemie.DialogSystem
{
    /// <summary>
    /// 对话图
    /// 该图包含自身信息 图参数 连接节点
    /// 图的运行从startNode开始,因此其一定不能为null
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue Graph", menuName = "Dialog System/Dialogue Graph")]
    public class DialogueGraph : ScriptableObject
    {
        #region 字段
        /// <summary> 对话图ID </summary>
        [SerializeField]
        private int graphId;
        /// <summary> 对话图名称 </summary>
        [SerializeField]
        private string graphName;
        /// <summary> 开始节点 </summary>
        [SerializeField]
        private DialogueNode startNode;
        /// <summary> 节点列表 </summary>
        [SerializeField]
        private List<DialogueNode> nodeList = new();
        
        /// <summary> 图参数 </summary>
        [SerializeField, HideInInspector]
        private List<DialogueParameterDefinition> parameterList = new();
        #endregion

        #region 属性
        public int GraphId => graphId;
        public string GraphName => graphName;
        public DialogueNode StartNode => startNode;
        public List<DialogueNode> NodeList => nodeList;
        public List<DialogueParameterDefinition> Parameters => parameterList;
        #endregion

        #region 方法
        /// <summary>
        /// 添加节点
        /// </summary>
        public void AddNode(DialogueNode node)
        {
            if (nodeList is null)
                nodeList = new List<DialogueNode>();
            nodeList.Add(node);
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        public void RemoveNode(DialogueNode node)
        {
            if (nodeList is null)
            {
                Debug.LogError("Node list is null, please add node first");
                return;
            }
            nodeList.Remove(node);
        }

        /// <summary>
        /// 查找参数定义
        /// </summary>
        public DialogueParameterDefinition FindParameter(string paramName)
        {
            if (string.IsNullOrEmpty(paramName) || parameterList == null)
                return null;

            foreach (var param in parameterList)
            {
                if (param != null && param.name == paramName)
                    return param;
            }

            return null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 设置开始节点
        /// </summary>
        public void SetStartNodeInEditorWindow(DialogueNode node)
        {
            startNode = node;
        }
#endif
        #endregion
    }
}
