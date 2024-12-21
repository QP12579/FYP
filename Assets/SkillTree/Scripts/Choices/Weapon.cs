using System.Collections.Generic;
using UnityEngine;

namespace Skill
{
    [System.Serializable]
    public class WeaponData
    {
        public string weaponName;
        public int Level;
        public Sprite icon;
        public string description;
        public WeaponType type;
        public float damage;
        public GameObject weaponObject;
    }

    [CreateAssetMenu(fileName = "Weapon", menuName = "Data/WeaponData", order = 1)]
    public class Weapon : ScriptableObject
    {
        public List<WeaponData> data_weapon = null;
    }

    public enum WeaponType
    {
        Sword,
        Magic
    }
}