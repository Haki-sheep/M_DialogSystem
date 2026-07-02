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
    enum Tab { 配置, GM }

    Tab tab = Tab.配置;
    OdinMenuTree menuTree;
    Quest selectedQuest;
    Vector2 gmScroll;
    readonly Dictionary<int, string> gmTipDict = new();

    [MenuItem("Tools/MmQuestWindow")]
    static void Open()
    {
      var w = GetWindow<QuestEditorWindow>();
      w.titleContent = new GUIContent("Quest System");
      w.minSize = new Vector2(720, 420);
      w.Show();
    }

    void OnEnable()
    {
      EditorApplication.projectChanged += RebuildMenuTree;
      RebuildMenuTree();
    }

    void OnDisable()
    {
      EditorApplication.projectChanged -= RebuildMenuTree;
      menuTree = null;
    }

    void OnGUI()
    {
      tab = (Tab)GUILayout.Toolbar((int)tab, new[] { "配置", "GM" });
      EditorGUILayout.Space(4);

      if (tab == Tab.配置)
        DrawConfigTab();
      else
        DrawGmTab();
    }

    #region 配置

    void DrawConfigTab()
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

    void DrawSelectedQuestInspector()
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

    void RebuildMenuTree()
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

    void CreateQuest()
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

    void DrawGmTab()
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

    void DrawQuestGmRow(QuestManager mgr, Quest quest)
    {
      EditorGUILayout.BeginVertical("box");
      var state = mgr.GetState(quest.QuestId);
      string stateText = state == EQuestState.提交 ? "已完成" : state.ToString();
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

    static bool CanAccept(EQuestState state) =>
      state is EQuestState.可用 or EQuestState.提交 or EQuestState.失败;

    static string ObjectiveHint(QuestObjective o)
    {
      if (o == null) return "";
      return o.type == EQuestObjectiveType.对话 ? o.targetId.ToString() : o.targetKey;
    }

    #endregion
  }
}
#endif
