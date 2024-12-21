using System.Collections.Generic;
using UnityEngine;

namespace Skill
{
    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public Sprite icon;
        public string description;
        public ItemT itemType;
        public float heal;
        public int number;
        public GameObject itemPrefab;
    }

    [CreateAssetMenu(fileName = "Item", menuName = "Data/ItemData", order = 2)]
    public class Item : ScriptableObject
    {
        public List<ItemData> Items = null;
    }

    public enum ItemT
    {
        drink,
        food,
        wine
    }
}