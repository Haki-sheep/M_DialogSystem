namespace Miemie.DialogSystem.Quest
{
  public partial class QuestManager
  {
    /// <summary>
    /// 保存任务
    /// </summary>
    public void SaveQuests()
    {
      var saveFile = new QuestSaveFile();
      foreach (var runtime in stateDict.Values)
      {
        float remainSeconds = GetRemainSeconds(runtime);
        saveFile.questDataList.Add(runtime.ToSaveData(remainSeconds));
      }

      QuestSaveService.Save(saveFile);
    }

    /// <summary>
    /// 读取任务
    /// </summary>
    public bool LoadQuests()
    {
      var saveFile = QuestSaveService.Load();
      if (saveFile?.questDataList == null) return false;

      activeQuestList.Clear();
      StopAllTimeLimit();

      foreach (var saveData in saveFile.questDataList)
      {
        if (saveData == null) continue;
        if (!stateDict.TryGetValue(saveData.questId, out var runtime)) continue;

        runtime.ApplySaveData(saveData);

        if (runtime.eQuestState != EQuestState.执行中) continue;

        activeQuestList.Add(runtime);
        if (runtime.quest.HasTimeLimit)
          StartTimeLimit(runtime.quest.QuestId, saveData.remainSeconds);
      }

      return true;
    }

    /// <summary>
    /// 清空任务存档
    /// </summary>
    public void ClearQuestSave()
    {
      QuestSaveService.Clear();
    }
  }
}
