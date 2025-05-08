using System.Collections.Generic;
using UnityEngine;

public class DebuffDataManager : Singleton<DebuffDataManager>
{
    [System.Serializable]
    public class DebuffData
    {
        public string debuffName;
        public float[] values;
        public int maxLevel;
        public float duration = 15f;
    }

    public Dictionary<DeBuffType, DebuffData> debuffDatabase = new Dictionary<DeBuffType, DebuffData>();

    private void Start()
    {
        LoadBuffDataFromTSV();
    }

    private void LoadBuffDataFromTSV()
    {
        TextAsset tsvFile = Resources.Load<TextAsset>("Buffs/DebuffData");
        string[] lines = tsvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split('\t');
            if (columns.Length < 2) continue;

            string buffName = columns[0].Trim();
            if (!System.Enum.TryParse(buffName, out DeBuffType buffType))
            {
                Debug.LogError($"Unknown DeBuffType: {buffName}");
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
                debuffDatabase[buffType] = new DebuffData
                {
                    debuffName = buffName,
                    values = values.ToArray(),
                    maxLevel = values.Count,
                    duration = 15f
                };
            }
        }

        Debug.Log($"Loaded {debuffDatabase.Count} buffs from TSV.");
    }

    public DebuffData GetDeBuffData(DeBuffType type)
    {
        return debuffDatabase.ContainsKey(type) ? debuffDatabase[type] : null;
    }
}