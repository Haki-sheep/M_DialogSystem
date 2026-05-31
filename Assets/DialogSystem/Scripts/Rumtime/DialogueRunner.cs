using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Miemie.DialogSystem
{
    public class DialogueRunner : MonoBehaviour
    {
        [SerializeField] private DialogueGraph dialogueGraph;
        [SerializeField] private DialogueVariables variables;
        [SerializeField, ReadOnly] private DialogueNode currentNode;

        readonly List<DialogueChoice> availableChoices = new();

        public DialogueNode CurrentNode => currentNode;

        void Start()
        {
            StartDialog();
        }

        void Update()
        {
            if (currentNode == null) return;

            if (Input.GetKeyDown(KeyCode.Space))
                Advance();

            if (!currentNode.IsOptionNode) return;

            RefreshAvailableChoices();
            for (int i = 0; i < availableChoices.Count && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    SelectOption(i);
            }
        }

        /// <summary>
        /// 开始对话
        /// </summary>
        public void StartDialog()
        {
            if (dialogueGraph == null)
            {
                Debug.LogError("Dialogue graph is null");
                return;
            }

            if (dialogueGraph.StartNode == null)
            {
                Debug.LogError("Start node is null");
                return;
            }
            GoTo(dialogueGraph.StartNode);
        }

        /// <summary>
        /// 播放指定对话图（编辑器预览用）
        /// </summary>
        public void PlayGraph(DialogueGraph graph)
        {
            dialogueGraph = graph;
            StartDialog();
        }

        /// <summary>
        /// 跳转到节点
        /// </summary>
        /// <param name="node"></param>
        public void GoTo(DialogueNode node)
        {
            if (node == null)
            {
                Debug.Log("对话结束");
                currentNode = null;
                return;
            }

            currentNode = node;
            Debug.Log($"当前节点为: {currentNode.SpeakType}");
            currentNode.PlayNode();

            if (currentNode.IsOptionNode)
                LogOptions();
        }

        /// <summary>
        /// 前进
        /// </summary>
        public void Advance()
        {
            if (currentNode == null) return;

            if (currentNode.IsOptionNode)
            {
                Debug.Log("当前是选项节点，请按数字键 1~9 选择");
                return;
            }

            var link = PickLink(currentNode);
            if (link == null)
            {
                Debug.Log("没有满足条件的出口");
                return;
            }

            // 跳转到连线节点
            GoTo(link.toNode);
        }

        /// <summary>
        /// 选择选项
        /// </summary>
        /// <param name="index"></param>
        public void SelectOption(int index)
        {
            RefreshAvailableChoices();
            if (index < 0 || index >= availableChoices.Count) return;

            GoTo(availableChoices[index].toNode);
        }

        /// <summary>
        /// 选择连线
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public DialogueLink PickLink(DialogueNode node)
        {
            if (node.LinkList == null) return null;

            foreach (var link in node.LinkList)
            {
                if (link == null || link.toNode == null) continue;
                if (link.CanPass(variables))
                    return link;
            }
            return null;
        }

        /// <summary>
        /// 刷新可用选项
        /// 该方法用于刷新当前节点可用的选项
        /// </summary>
        public void RefreshAvailableChoices()
        {
            availableChoices.Clear();
            if (currentNode?.ChoiceList == null) return;

            foreach (var c in currentNode.ChoiceList)
            {
                if (c == null || c.toNode == null) continue;
                if (c.CanPass(variables))
                    availableChoices.Add(c);
            }
        }

        /// <summary>
        /// 日志选项
        /// 该方法用于日志当前节点可用的选项
        /// </summary>
        public void LogOptions()
        {
            RefreshAvailableChoices();
            if (availableChoices.Count == 0)
            {
                Debug.Log("没有可用选项");
                return;
            }

            for (int i = 0; i < availableChoices.Count; i++)
                Debug.Log($"  [{i + 1}] {availableChoices[i].labelText}");
        }
    }
}