using UnityEngine;

namespace Miemie.DialogSystem.Quest
{
    /// <summary>
    /// 简易任务追踪 OnGUI
    /// </summary>
    public class QuestTrackerUI : MonoBehaviour
    {
        [SerializeField]
        Vector2 screenOffset = new(12, 12);

        [SerializeField]
        float panelWidth = 300;

        GUIStyle titleStyle;
        GUIStyle doneStyle;

        #region 绘制

        void OnGUI()
        {
            var manager = QuestManager.Instance;
            if (manager == null)
                return;

            EnsureStyles();

            float y = screenOffset.y;
            foreach (var state in manager.EnumerateStates())
            {
                if (state.State != EQuestState.Active)
                    continue;

                float height = 28 + state.ObjectiveList.Count * 22;
                var rect = new Rect(screenOffset.x, y, panelWidth, height);
                GUI.Box(rect, GUIContent.none);

                GUILayout.BeginArea(rect);
                GUILayout.Label(state.Def.QuestTitle, titleStyle);

                for (int i = 0; i < state.ObjectiveList.Count; i++)
                {
                    var objective = state.ObjectiveList[i];
                    if (objective == null)
                        continue;

                    int required = objective.requiredCount > 0 ? objective.requiredCount : 1;
                    int progress = state.ProgressList[i];
                    bool done = progress >= required;
                    string label = $"{state.GetObjectiveLabel(i)}  {progress}/{required}";
                    GUILayout.Label(label, done ? doneStyle : GUI.skin.label);
                }

                GUILayout.EndArea();
                y += height + 8;
            }
        }

        void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            doneStyle ??= new GUIStyle(GUI.skin.label) { normal = { textColor = new Color(0.5f, 0.9f, 0.5f) } };
        }

        #endregion
    }
}
