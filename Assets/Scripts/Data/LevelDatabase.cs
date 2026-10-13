using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    public List<LevelData> levels = new List<LevelData>();

    public int Count => levels.Count;

    public bool HasLevel(int index) => index >= 0 && index < levels.Count;

    /// Lấy dữ liệu màn theo index sai index thì trả về null.
    public LevelData GetData(int index)
    {
        return HasLevel(index) ? levels[index] : null;
    }

    /// Tìm index của màn theo levelID, không thấy thì trả về -1.
    public int IndexOfID(int levelID)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i] != null && levels[i].levelID == levelID) return i;
        }
        return -1;
    }
}
