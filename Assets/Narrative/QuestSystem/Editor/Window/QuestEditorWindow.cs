#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Miemie.Narrative.GraphViewFrame.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Miemie.DialogSystem.Quest.Editor
{
  /// <summary>
  /// 任务编辑器 配置 + Play GM
  /// </summary>
  public class QuestEditorWindow : EditorWindow
  {
    private enum Tab { 配置, GM }

    private Tab tab = Tab.配置;
    private OdinMenuTree menuTree;
    private Quest selectedQuest;
    private Vector2 gmScroll;
    private readonly Dictionary<int, string> gmTipDict = new();

    /// <summary>
    /// 打开任务编辑器窗口
    /// </summary>
    [MenuItem("Tools/MmQuestWindow")]
    private static void Open()
    {
      var w = GetWindow<QuestEditorWindow>();
      w.titleContent = new GUIContent("Quest System");
      w.minSize = new Vector2(720, 420);
      w.Show();
    }

    /// <summary>
    /// 窗口启用时重建菜单
    /// </summary>
    private void OnEnable()
    {
      EditorApplication.projectChanged += RebuildMenuTree;
      RebuildMenuTree();
    }

    /// <summary>
    /// 窗口关闭时释放资源
    /// </summary>
    private void OnDisable()
    {
      EditorApplication.projectChanged -= RebuildMenuTree;
      menuTree = null;
    }

    /// <summary>
    /// 绘制主界面
    /// </summary>
    private void OnGUI()
    {
      tab = (Tab)GUILayout.Toolbar((int)tab, new[] { "配置", "GM" });
      EditorGUILayout.Space(4);

      if (tab == Tab.配置)
        DrawConfigTab();
      else
        DrawGmTab();
    }

    #region 配置

    /// <summary>
    /// 绘制配置页
    /// </summary>
    private void DrawConfigTab()
    {
      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("新建任务", GUILayout.Width(80)))
        CreateQuest();

      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.BeginVertical(GUILayout.Width(240));
      GraphViewFramePanelStyles.DrawPanelHeader("任务", selectedQuest != null ? selectedQuest.name : "未选中");
      GraphViewFramePanelStyles.BeginPaddedContent();
      menuTree?.DrawMenuTree();
      menuTree?.HandleKeyboardMenuNavigation();
      GraphViewFramePanelStyles.EndPaddedContent();
      EditorGUILayout.EndVertical();

      EditorGUILayout.BeginVertical();
      GraphViewFramePanelStyles.DrawPanelHeader("属性", selectedQuest != null ? selectedQuest.Title : "未选中");
      GraphViewFramePanelStyles.BeginPaddedContent();
      DrawSelectedQuestInspector();
      GraphViewFramePanelStyles.EndPaddedContent();
      EditorGUILayout.EndVertical();
      EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 绘制选中任务属性
    /// </summary>
    private void DrawSelectedQuestInspector()
    {
      if (selectedQuest == null)
      {
        EditorGUILayout.HelpBox("左侧选中任务 或直接新建", MessageType.Info);
        return;
      }

      var so = new SerializedObject(selectedQuest);
      so.Update();
      EditorGUILayout.PropertyField(so.FindProperty("questId"));
      EditorGUILayout.PropertyField(so.FindProperty("category"));
      EditorGUILayout.PropertyField(so.FindProperty("title"));
      EditorGUILayout.PropertyField(so.FindProperty("description"));
      EditorGUILayout.PropertyField(so.FindProperty("acceptMode"));
      EditorGUILayout.PropertyField(so.FindProperty("timeLimit"));
      EditorGUILayout.PropertyField(so.FindProperty("objectiveList"), true);
      EditorGUILayout.PropertyField(so.FindProperty("prerequisiteIdList"), true);
      so.ApplyModifiedProperties();
    }

    /// <summary>
    /// 重建左侧任务菜单树
    /// </summary>
    private void RebuildMenuTree()
    {
      menuTree = new OdinMenuTree(false)
      {
        Config =
        {
          DrawSearchToolbar = true,
          DefaultMenuStyle = GraphViewFramePanelStyles.CreateMenuStyle(),
        },
      };
      menuTree.Selection.SelectionChanged += _ =>
      {
        selectedQuest = menuTree.Selection.SelectedValue as Quest;
        Repaint();
      };

      GraphAssetMenuTreeUtility.AddAllAssetsByType(menuTree, "任务", typeof(Quest));
      menuTree.EnumerateTree().AddThumbnailIcons(true);

      if (selectedQuest != null)
      {
        var item = menuTree.EnumerateTree().FirstOrDefault(x => x.Value == selectedQuest);
        if (item != null)
        {
          menuTree.Selection.Clear();
          menuTree.Selection.Add(item);
        }
      }
    }

    /// <summary>
    /// 新建任务资产
    /// </summary>
    private void CreateQuest()
    {
      QuestEditorPaths.EnsureQuestFolder();
      string path = AssetDatabase.GenerateUniqueAssetPath($"{QuestEditorPaths.QuestAssetPath}/New Quest.asset");
      var quest = CreateInstance<Quest>();
      AssetDatabase.CreateAsset(quest, path);
      AssetDatabase.SaveAssets();
      selectedQuest = quest;
      RebuildMenuTree();
      Repaint();
    }

    #endregion

    #region GM

    /// <summary>
    /// 绘制 GM 调试页
    /// </summary>
    private void DrawGmTab()
    {
      if (!Application.isPlaying)
      {
        EditorGUILayout.HelpBox("进入 Play 后使用 GM 面板查看状态并调试", MessageType.Info);
        return;
      }

      var mgr = QuestManager.Instance;
      if (mgr == null)
      {
        EditorGUILayout.HelpBox("场景中没有 QuestManager", MessageType.Warning);
        return;
      }

      int total = mgr.RegisteredQuests.Count;
      int missing = mgr.RegisteredQuests.Count(q => q == null);
      EditorGUILayout.LabelField($"运行时任务 {total - missing}/{total}", EditorStyles.boldLabel);
      if (missing > 0)
        EditorGUILayout.HelpBox("questList 有丢失引用 请检查 QuestScene", MessageType.Warning);
      EditorGUILayout.Space(4);

      gmScroll = EditorGUILayout.BeginScrollView(gmScroll);
      foreach (var quest in mgr.RegisteredQuests)
      {
        if (quest == null) continue;
        DrawQuestGmRow(mgr, quest);
      }
      EditorGUILayout.EndScrollView();

      if (Event.current.type == EventType.Repaint)
        Repaint();
    }

    /// <summary>
    /// 绘制单个任务的 GM 行
    /// </summary>
    private void DrawQuestGmRow(QuestManager mgr, Quest quest)
    {
      EditorGUILayout.BeginVertical("box");
      var state = mgr.GetState(quest.QuestId);
      string stateText = state == EQuestState.提交 ? "已完成" : state.ToString();
      string timeText = TimeText(mgr, quest, state);
      if (!string.IsNullOrEmpty(timeText))
        stateText = $"{stateText}  {timeText}";
      EditorGUILayout.LabelField($"[{quest.QuestId}] {quest.Title}  —  {stateText}", EditorStyles.boldLabel);

      var objectives = quest.GetObjectives();
      for (int i = 0; i < objectives.Count; i++)
      {
        var o = objectives[i];
        int prog = mgr.GetObjectiveProgress(quest.QuestId, i);
        int need = o != null && o.count > 0 ? o.count : 1;
        string hint = ObjectiveHint(o);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"  {i + 1}. {o?.type} {hint}  {prog}/{need}");
        if (state == EQuestState.执行中 && o != null && prog < need)
        {
          if (GUILayout.Button(QuestGmSimulation.ButtonLabel(o), GUILayout.Width(52)))
            QuestGmSimulation.FireOnce(o);
        }
        EditorGUILayout.EndHorizontal();
      }

      if (gmTipDict.TryGetValue(quest.QuestId, out string tip) && !string.IsNullOrEmpty(tip))
        EditorGUILayout.HelpBox(tip, MessageType.Warning);

      EditorGUILayout.BeginHorizontal();
      if (CanAccept(state))
      {
        if (GUILayout.Button(state == EQuestState.可用 ? "接受" : "重接", GUILayout.Width(52)))
        {
          gmTipDict.Remove(quest.QuestId);
          mgr.Accept(quest.QuestId);
        }
      }
      if (state == EQuestState.执行中)
      {
        if (GUILayout.Button("真实提交", GUILayout.Width(64)))
        {
          if (mgr.TrySubmit(quest.QuestId))
            gmTipDict.Remove(quest.QuestId);
          else
            gmTipDict[quest.QuestId] = "任务尚未完成 请继续";
        }
        if (GUILayout.Button("GM提交", GUILayout.Width(56)))
        {
          gmTipDict.Remove(quest.QuestId);
          mgr.GmSubmit(quest.QuestId);
        }
        if (GUILayout.Button("失败", GUILayout.Width(44)))
          mgr.Fail(quest.QuestId);
      }
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.EndVertical();
      EditorGUILayout.Space(4);
    }

    /// <summary>
    /// 当前状态是否可接受
    /// </summary>
    private static bool CanAccept(EQuestState state) =>
      state is EQuestState.可用 or EQuestState.提交 or EQuestState.失败;

    /// <summary>
    /// 目标匹配提示文本
    /// </summary>
    private static string ObjectiveHint(QuestObjective o)
    {
      if (o == null) return "";
      if (o.type != EQuestObjectiveType.对话) return o.targetKey;

      string graphName = o.dialogueGraph != null ? o.dialogueGraph.name : "未选择对话图";
      return $"{graphName} / {o.dialogueEventKey}";
    }

    /// <summary>
    /// 限时任务显示文本
    /// </summary>
    private static string TimeText(QuestManager mgr, Quest quest, EQuestState state)
    {
      if (!quest.HasTimeLimit) return "";
      if (state != EQuestState.执行中) return $"限时 {FormatTime(quest.TimeLimit)}";

      float remainSeconds = mgr.GetQuestRemainSeconds(quest.QuestId);
      return $"剩余 {FormatTime(remainSeconds)}";
    }

    /// <summary>
    /// 格式化时间
    /// </summary>
    private static string FormatTime(float seconds)
    {
      int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
      int minute = totalSeconds / 60;
      int second = totalSeconds % 60;
      return $"{minute:00}:{second:00}";
    }

    #endregion
  }
}
#endif
