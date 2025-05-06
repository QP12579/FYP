using System.Collections.Generic;
using UnityEngine;

public class BuffDataManager : Singleton<BuffDataManager>
{
    [System.Serializable]
    public class BuffData
    {
        public string buffName;      
        public float[] values;      
        public int maxLevel; 
        public float duration = 15f;
    }

    public Dictionary<BuffType, BuffData> buffDatabase = new Dictionary<BuffType, BuffData>();

    private void Start()
    {
        LoadBuffDataFromTSV();
    }

    private void LoadBuffDataFromTSV()
    {
        TextAsset tsvFile = Resources.Load<TextAsset>("Buffs/BuffData");
        string[] lines = tsvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split('\t');
            if (columns.Length < 2) continue;

            string buffName = columns[0].Trim();
            if (!System.Enum.TryParse(buffName, out BuffType buffType))
            {
                Debug.LogError($"Unknown BuffType: {buffName}");
                continue;
            }

            List<float> values = new List<float>();
            for (int j = 1; j < columns.Length; j++)
            {
                if (float.TryParse(columns[j].Trim(), out float percentValue))
                {
                    values.Add(percentValue / 100f); 
                }
            }

            if (values.Count > 0)
            {
                buffDatabase[buffType] = new BuffData
                {
                    buffName = buffName,
                    values = values.ToArray(),
                    maxLevel = values.Count,
                    duration = 15f
                };
            }
        }

        Debug.Log($"Loaded {buffDatabase.Count} buffs from TSV.");
    }

    public BuffData GetBuffData(BuffType type)
    {
        return buffDatabase.ContainsKey(type) ? buffDatabase[type] : null;
    }
}