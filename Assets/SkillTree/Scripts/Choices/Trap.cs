using System.Collections.Generic;
using UnityEngine;

namespace Skill
{
    [System.Serializable]
    public class TrapData
    {
        public string trapName;
        public Sprite icon;
        public string description;
        public TrapT trapType;
        public float attact;
        public int number;
        public GameObject trapPrefab;
    }

    [CreateAssetMenu(fileName = "Trap", menuName = "Data/Traps", order = 4)]

public class Trap : ScriptableObject
    {
        public List<TrapData> trapData = null;
    }

    public enum TrapT
    {
        Boom,
        Enemy
    }
}
