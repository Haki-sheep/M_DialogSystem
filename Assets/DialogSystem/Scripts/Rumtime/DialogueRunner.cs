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

            variables?.ApplyDefaults(dialogueGraph.Parameters);
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

            var transition = currentNode.NextTransition;
            if (transition?.toNode == null)
            {
                Debug.Log("没有下一节点");
                return;
            }

            if (!transition.CanPass(variables))
            {
                Debug.Log("连线条件未满足");
                return;
            }

            GoTo(transition.toNode);
        }

        /// <summary>
        /// 选择选项
        /// </summary>
        public void SelectOption(int index)
        {
            RefreshAvailableChoices();
            if (index < 0 || index >= availableChoices.Count) return;

            var choice = availableChoices[index];
            GoTo(choice.toNode);
        }

        /// <summary>
        /// 刷新可用选项
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
