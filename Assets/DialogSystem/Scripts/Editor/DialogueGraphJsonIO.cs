#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Editor
{
    /// <summary>
    /// 对话图 JSON 导入与导出
    /// </summary>
    static class DialogueGraphJsonIO
    {
        static readonly JsonSerializerSettings ExportSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
        };

        static readonly JsonSerializerSettings ImportSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
        };

        public static bool ExportWithSaveDialog(DialogueGraph graph)
        {
            if (graph == null)
            {
                Debug.LogWarning("未指定要导出的对话图");
                return false;
            }

            DialogueEditorPaths.EnsureExportFolder();

            string path = EditorUtility.SaveFilePanel(
                "导出对话图 JSON",
                DialogueEditorPaths.ExportFolderAbsolute,
                graph.name,
                "json");

            if (string.IsNullOrEmpty(path))
                return false;

            WriteJsonFile(path, Serialize(graph));
            Debug.Log($"对话图已导出为 JSON: {path}");
            EditorUtility.RevealInFinder(path);
            return true;
        }

        public static bool ImportWithOpenDialog(DialogueGraph targetGraph, out DialogueGraph importedGraph)
        {
            importedGraph = targetGraph;

            string defaultDir = Directory.Exists(DialogueEditorPaths.ExportFolderAbsolute)
                ? DialogueEditorPaths.ExportFolderAbsolute
                : Path.GetDirectoryName(Application.dataPath);

            string path = EditorUtility.OpenFilePanel("导入对话图 JSON", defaultDir, "json");
            if (string.IsNullOrEmpty(path))
                return false;

            return ImportFromFile(path, targetGraph, out importedGraph);
        }

        public static bool ImportFromFile(string absolutePath, DialogueGraph targetGraph, out DialogueGraph importedGraph)
        {
            importedGraph = targetGraph;

            if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
            {
                Debug.LogError($"找不到 JSON 文件: {absolutePath}");
                return false;
            }

            DialogueGraphJson model;
            try
            {
                model = JsonConvert.DeserializeObject<DialogueGraphJson>(File.ReadAllText(absolutePath), ImportSettings);
            }
            catch (JsonException ex)
            {
                Debug.LogError($"JSON 解析失败: {ex.Message}");
                return false;
            }

            if (model?.nodes == null)
            {
                Debug.LogError("JSON 内容无效或缺少 nodes");
                return false;
            }

            if (targetGraph != null)
            {
                if (!EditorUtility.DisplayDialog(
                        "导入 JSON",
                        $"将把 JSON 导入到「{targetGraph.name}」。\n同 nodeId 会更新 缺失 nodeId 会新建 JSON 中没有的节点会从图中移除但资产保留。\n是否继续？",
                        "导入",
                        "取消"))
                    return false;

                ApplyModel(targetGraph, model);
                importedGraph = targetGraph;
            }
            else
            {
                importedGraph = CreateGraph(model);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"对话图 JSON 导入完成: {importedGraph.name}（{model.nodes.Count} 个节点）");
            return true;
        }

        static string Serialize(DialogueGraph graph) =>
            JsonConvert.SerializeObject(ToModel(graph), ExportSettings);

        static void WriteJsonFile(string absolutePath, string json)
        {
            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(absolutePath, json, Encoding.UTF8);

            if (absolutePath.StartsWith(Application.dataPath))
                AssetDatabase.Refresh();
        }

        static DialogueGraphJson ToModel(DialogueGraph graph)
        {
            var model = new DialogueGraphJson
            {
                graphId = graph.GraphId,
                graphName = graph.GraphName,
                assetName = graph.name,
                assetPath = AssetDatabase.GetAssetPath(graph),
                startNodeId = graph.StartNode != null ? graph.StartNode.NodeId : 0,
            };

            if (graph.NodeList == null)
                return model;

            foreach (var node in graph.NodeList)
            {
                if (node != null)
                    model.nodes.Add(ToNodeModel(graph, node));
            }

            return model;
        }

        static DialogueNodeJson ToNodeModel(DialogueGraph graph, DialogueNode node)
        {
            var layout = DialogueGraphLayoutStore.GetPosition(graph, node);
            var nodeJson = new DialogueNodeJson
            {
                nodeId = node.NodeId,
                assetName = node.name,
                speakType = node.SpeakType.ToString(),
                speakerName = node.SpeakerName,
                dialogText = node.DialogText,
                isOptionNode = node.IsOptionNode,
                layout = new LayoutJson { x = layout.x, y = layout.y },
            };

            if (node.IsOptionNode && node.ChoiceList != null)
            {
                foreach (var choice in node.ChoiceList)
                {
                    if (choice == null)
                        continue;

                    nodeJson.choiceList.Add(new DialogueChoiceJson
                    {
                        labelText = choice.labelText,
                        toNodeId = choice.toNode != null ? choice.toNode.NodeId : 0,
                        condition = ToConditionModel(choice.condition),
                    });
                }
            }
            else if (node.LinkList != null)
            {
                foreach (var link in node.LinkList)
                {
                    if (link == null)
                        continue;

                    nodeJson.linkList.Add(new DialogueLinkJson
                    {
                        toNodeId = link.toNode != null ? link.toNode.NodeId : 0,
                        condition = ToConditionModel(link.condition),
                    });
                }
            }

            return nodeJson;
        }

        static DialogueConditionJson ToConditionModel(DialogueCondition condition)
        {
            if (condition == null)
                return new DialogueConditionJson { conditionType = E_Condition.None.ToString() };

            return new DialogueConditionJson
            {
                conditionType = condition.e_Condition.ToString(),
                key = condition.key,
                targetBool = condition.targetBool,
                targetFloat = condition.targetFloat,
            };
        }

        static DialogueGraph CreateGraph(DialogueGraphJson model)
        {
            DialogueEditorPaths.EnsureGraphAssetFolder();

            string name = string.IsNullOrWhiteSpace(model.assetName) ? "Imported Dialogue Graph" : model.assetName.Trim();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{DialogueEditorPaths.GraphAssetPath}/{name}.asset");
            var graph = ScriptableObject.CreateInstance<DialogueGraph>();
            AssetDatabase.CreateAsset(graph, assetPath);
            ApplyModel(graph, model);
            return graph;
        }

        static void ApplyModel(DialogueGraph graph, DialogueGraphJson model)
        {
            var existingById = new Dictionary<int, DialogueNode>();
            if (graph.NodeList != null)
            {
                foreach (var node in graph.NodeList)
                {
                    if (node != null)
                        existingById[node.NodeId] = node;
                }

                graph.NodeList.Clear();
            }

            var idMap = new Dictionary<int, DialogueNode>();
            foreach (var nodeJson in model.nodes)
            {
                if (nodeJson == null)
                    continue;

                if (!existingById.TryGetValue(nodeJson.nodeId, out var node))
                {
                    node = CreateNodeAsset(nodeJson);
                    existingById[nodeJson.nodeId] = node;
                }

                ApplyNodeScalars(node, nodeJson);
                graph.AddNode(node);
                idMap[nodeJson.nodeId] = node;
            }

            foreach (var nodeJson in model.nodes)
            {
                if (nodeJson == null || !idMap.TryGetValue(nodeJson.nodeId, out var node))
                    continue;

                if (nodeJson.isOptionNode)
                    ApplyChoices(node, nodeJson.choiceList, idMap);
                else
                    ApplyLinks(node, nodeJson.linkList, idMap);
            }

            var graphSo = new SerializedObject(graph);
            graphSo.FindProperty("graphId").intValue = model.graphId;
            graphSo.FindProperty("graphName").stringValue = model.graphName ?? string.Empty;
            graphSo.FindProperty("startNode").objectReferenceValue =
                model.startNodeId != 0 && idMap.TryGetValue(model.startNodeId, out var startNode) ? startNode : null;
            graphSo.ApplyModifiedPropertiesWithoutUndo();

            var importedLayouts = new List<(DialogueNode node, Vector2 position)>();
            foreach (var nodeJson in model.nodes)
            {
                if (nodeJson?.layout == null || !idMap.TryGetValue(nodeJson.nodeId, out var node))
                    continue;

                importedLayouts.Add((node, new Vector2(nodeJson.layout.x, nodeJson.layout.y)));
            }

            DialogueGraphLayoutStore.ReplaceGraphLayouts(graph, importedLayouts);

            EditorUtility.SetDirty(graph);
            foreach (var node in idMap.Values)
                EditorUtility.SetDirty(node);
        }

        static DialogueNode CreateNodeAsset(DialogueNodeJson nodeJson)
        {
            string fileName = string.IsNullOrWhiteSpace(nodeJson.assetName) ? "New Dialog Node" : nodeJson.assetName.Trim();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{DialogueEditorPaths.GraphAssetPath}/{fileName}.asset");
            var node = ScriptableObject.CreateInstance<DialogueNode>();
            AssetDatabase.CreateAsset(node, assetPath);
            return node;
        }

        static void ApplyNodeScalars(DialogueNode node, DialogueNodeJson data)
        {
            var so = new SerializedObject(node);
            so.FindProperty("nodeId").intValue = data.nodeId;
            so.FindProperty("speakerName").stringValue = data.speakerName ?? string.Empty;
            so.FindProperty("dialogText").stringValue = data.dialogText ?? string.Empty;
            so.FindProperty("isOptionNode").boolValue = data.isOptionNode;

            if (!string.IsNullOrEmpty(data.speakType) && System.Enum.TryParse(data.speakType, out SpeakEnums speakType))
                so.FindProperty("speakType").enumValueIndex = (int)speakType;

            so.FindProperty("linkList").ClearArray();
            so.FindProperty("choiceList").ClearArray();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ApplyLinks(DialogueNode node, List<DialogueLinkJson> links, Dictionary<int, DialogueNode> idMap)
        {
            var so = new SerializedObject(node);
            var array = so.FindProperty("linkList");
            array.ClearArray();
            FillConnectionArray(array, links, idMap, isChoice: false);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ApplyChoices(DialogueNode node, List<DialogueChoiceJson> choices, Dictionary<int, DialogueNode> idMap)
        {
            var so = new SerializedObject(node);
            var array = so.FindProperty("choiceList");
            array.ClearArray();
            FillConnectionArray(array, choices, idMap, isChoice: true);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void FillConnectionArray<T>(SerializedProperty array, List<T> items, Dictionary<int, DialogueNode> idMap, bool isChoice)
            where T : class
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                array.InsertArrayElementAtIndex(array.arraySize);
                var elem = array.GetArrayElementAtIndex(array.arraySize - 1);

                if (isChoice && item is DialogueChoiceJson choice)
                {
                    elem.FindPropertyRelative("labelText").stringValue = choice.labelText ?? string.Empty;
                    elem.FindPropertyRelative("toNode").objectReferenceValue = ResolveNode(choice.toNodeId, idMap);
                    WriteCondition(elem.FindPropertyRelative("condition"), choice.condition);
                }
                else if (!isChoice && item is DialogueLinkJson link)
                {
                    elem.FindPropertyRelative("toNode").objectReferenceValue = ResolveNode(link.toNodeId, idMap);
                    WriteCondition(elem.FindPropertyRelative("condition"), link.condition);
                }
            }
        }

        static DialogueNode ResolveNode(int nodeId, Dictionary<int, DialogueNode> idMap) =>
            nodeId != 0 && idMap.TryGetValue(nodeId, out var node) ? node : null;

        static void WriteCondition(SerializedProperty conditionProp, DialogueConditionJson conditionJson)
        {
            if (conditionProp == null)
                return;

            var conditionType = E_Condition.None;
            if (conditionJson != null && !string.IsNullOrEmpty(conditionJson.conditionType))
                System.Enum.TryParse(conditionJson.conditionType, out conditionType);

            conditionProp.FindPropertyRelative("e_Condition").enumValueIndex = (int)conditionType;
            conditionProp.FindPropertyRelative("key").stringValue = conditionJson?.key ?? string.Empty;
            conditionProp.FindPropertyRelative("targetBool").boolValue = conditionJson?.targetBool ?? false;
            conditionProp.FindPropertyRelative("targetFloat").floatValue = conditionJson?.targetFloat ?? 0f;
        }
    }
}
#endif
