using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Miemie.DialogSystem
{
    public class DialogueRunner : SerializedMonoBehaviour
    {
        [SerializeField]
        DialogueGraph dialogueGraph;

        [OdinSerialize]
        DialogueVariablesStore variables = new();

        [SerializeField, ReadOnly]
        DialogueNode currentNode;

        readonly List<DialogueTransition> availableChoiceList = new();

        public DialogueNode CurrentNode => currentNode;

        #region 生命周期

        void Start() => StartDialog();

        void Update()
        {
            if (currentNode == null) return;

            if (Input.GetKeyDown(KeyCode.Space))
                Advance();

            if (!currentNode.IsOptionNode) return;

            RefreshAvailableChoices();
            for (int i = 0; i < availableChoiceList.Count && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    SelectOption(i);
            }
        }

        #endregion

        #region 对话流程

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

            variables?.ApplyDefaults(dialogueGraph.Variables);
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
                EndDialog();
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
                EndDialog();
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
            if (index < 0 || index >= availableChoiceList.Count) return;

            var choice = availableChoiceList[index];
            if (!string.IsNullOrEmpty(choice.eventKey))
                GameNotify.DialogueEvent(dialogueGraph, choice.eventKey);

            GoTo(choice.toNode);
        }

        void EndDialog()
        {
            Debug.Log("对话结束");
            currentNode = null;
            if (dialogueGraph != null)
                GameNotify.DialogueFinished(dialogueGraph);
        }

        #endregion

        #region 选项

        /// <summary>
        /// 刷新可用选项
        /// </summary>
        public void RefreshAvailableChoices()
        {
            availableChoiceList.Clear();
            if (currentNode?.ChoiceList == null) return;

            foreach (var c in currentNode.ChoiceList)
            {
                if (c == null || c.toNode == null) continue;
                if (c.CanPass(variables))
                    availableChoiceList.Add(c);
            }
        }

        /// <summary>
        /// 日志选项
        /// </summary>
        public void LogOptions()
        {
            RefreshAvailableChoices();
            if (availableChoiceList.Count == 0)
            {
                Debug.Log("没有可用选项");
                return;
            }

            for (int i = 0; i < availableChoiceList.Count; i++)
                Debug.Log($"  [{i + 1}] {availableChoiceList[i].labelText}");
        }

        #endregion
    }
}
