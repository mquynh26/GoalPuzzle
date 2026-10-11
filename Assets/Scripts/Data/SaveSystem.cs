using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveSystem
{
    const string Key = "FootballPuzzle.Save";

    [Serializable]
    class SaveData
    {
        public int highestUnlockedLevel = 1;
        public List<int> completedLevels = new List<int>();
    }

    static SaveData data;

    // Đọc từ PlayerPrefs ở lần truy cập đầu tiên
    static SaveData Data
    {
        get
        {
            if (data == null) Load();
            return data;
        }
    }

    // ---------- Đọc ----------

    /// Màn cao nhất đã mở khóa (màn 1 luôn mở).
    public static int HighestUnlockedLevel => Data.highestUnlockedLevel;

    public static IReadOnlyList<int> CompletedLevels => Data.completedLevels;

    public static bool IsUnlocked(int levelID) => levelID <= Data.highestUnlockedLevel;

    public static bool IsCompleted(int levelID) => Data.completedLevels.Contains(levelID);

    // ---------- Ghi ----------

    /// Đánh dấu hoàn thành màn levelID và mở khóa màn kế tiếp (không vượt quá totalLevels).
    public static void CompleteLevel(int levelID, int totalLevels = int.MaxValue)
    {
        if (!Data.completedLevels.Contains(levelID)) Data.completedLevels.Add(levelID);

        int next = Mathf.Min(levelID + 1, totalLevels);
        if (next > Data.highestUnlockedLevel) Data.highestUnlockedLevel = next;

        Save();
    }

    /// Xóa toàn bộ tiến trình, về như mới chơi lần đầu.
    public static void ResetProgress()
    {
        data = new SaveData();
        Save();
    }

    // ---------- Lưu / nạp ----------

    static void Load()
    {
        data = null;

        string json = PlayerPrefs.GetString(Key, "");
        if (!string.IsNullOrEmpty(json))
        {
            try { data = JsonUtility.FromJson<SaveData>(json); }
            catch (Exception) { data = null; }   // file hỏng: bỏ qua, dùng dữ liệu mới
        }

        if (data == null) data = new SaveData();
        if (data.completedLevels == null) data.completedLevels = new List<int>();
        if (data.highestUnlockedLevel < 1) data.highestUnlockedLevel = 1;
    }

    static void Save()
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }
}